using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Messaging.ServiceInterfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Globalization;

namespace Messaging.QueueService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// State description :
    ///     - queue : main queue. It contains available message for Get/Peek
    ///     - popedQueue : contains popreceipt+visibilityTime of poped message. This queue is used by the message autoreactivation  afetr timeout
    ///     - popedMessages : dictionnary that contains poped message . Used for Delete & Update feature.
    /// </summary>
    internal sealed class QueueService : StatefulService, IQueueService
    {
        private const string MessageQueueName = "mainQueue";
        private const string SettingsDictionnaryName = "settings";

        private const string RetentionDurationSettingKey = "RetentionDuration";
        private const int RetentionDurationDefaultValue = 7*24*60*60; // 7 days for default retention duration

        private const string DeleteDelaySettingKey = "DeleteDelay";
        private const int DeleteDelayDefaultValue = 60; // 60sec for delete delay by default



        /// <summary>
        /// Cached value of message retention duration (default is 60sec) before message will expire and remove from the queue.
        /// </summary>
        private int retentionDuration = RetentionDurationDefaultValue;


        /// <summary>
        /// Cached value (default is 60sec) of delete delay. 
        /// Poped message not deleted after this delay will be send back to main queue
        /// </summary>
        private int deleteDelay = DeleteDelayDefaultValue;

        public QueueService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            // TODO : implement optionnal security on remoting endpoint
            var listeners = new ServiceReplicaListener[1]  
            {
                new ServiceReplicaListener( (context) => this.CreateServiceRemotingListener(context), "ServiceEndpoint")
            };
            return listeners;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Reading settings value
            var settings = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(SettingsDictionnaryName).ConfigureAwait(false);
            using (var tx = this.StateManager.CreateTransaction())
            {
                retentionDuration = Convert.ToInt32(await settings.GetOrAddAsync(tx, RetentionDurationSettingKey,RetentionDurationDefaultValue.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false));
                deleteDelay = Convert.ToInt32(await settings.GetOrAddAsync(tx, DeleteDelaySettingKey, DeleteDelayDefaultValue.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false));
                await tx.CommitAsync().ConfigureAwait(false);
            }


            // main loop. waiting for remoting request
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        /// <summary>
        /// Insert a message into the queu
        /// </summary>
        /// <param name="messagePayload"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Message> PutAsync(string messagePayload, string clientId)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueService:PutAsync");
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<Message>>(MessageQueueName).ConfigureAwait(false);
            var utcNow = DateTime.UtcNow;
            var msg = new Message()
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                DequeueCount = 0,
                InsertionDate = utcNow,
                ExpirationDate=utcNow.AddSeconds(this.retentionDuration),
                NextVisibleDate=DateTime.MinValue,
                Payload=messagePayload,
                PopReceipt=null
            };
            using (var tx = this.StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, msg).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
            return msg;
        }

        /// <summary>
        /// Pop a message from the queue if message are available, and feed 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Message> GetAsync(string clientId)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueService:GetAsync");
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<Message>>(MessageQueueName).ConfigureAwait(false);
            var utcNow = DateTime.UtcNow; 
            using (var tx = this.StateManager.CreateTransaction())
            {
                var msgCV = await queue.TryDequeueAsync(tx).ConfigureAwait(false);
                if (msgCV.HasValue)
                {
                    // TODO : if message is expired --> ignored it and pop next message
                    // TODO : add msg ID to popedMessageQueue
                    // TODO : add msg to PopedMEssageDictionnary
                    await tx.CommitAsync().ConfigureAwait(false);
                    return msgCV.Value;
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(string popReceipt)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueService:DeleteAsync");

            // TODO : implement delete poped message

            throw new NotImplementedException();
        }

        /// <summary>
        /// Implementation of IQueueService.SetQueueRetentionTimeAsync
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durationInSeconds"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task<bool> SetQueueRetentionTimeAsync(int durationInSeconds, Guid correlationId)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueService:SetQueueRetentionAsync");

            if (durationInSeconds < 0 || durationInSeconds > 2678400) // id duration is neg or higher than 31days --> error
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueService.SetQueueRetentionTimeAsync() : invalid parametervalue durationInSeconds='{durationInSeconds}'");
                return false;
            }

            this.retentionDuration = durationInSeconds; // update cached value
            ServiceEventSource.Current.Message($"QueueService.SetQueueRetentionTimeAsync() : cached value updated.");

            var settings = await this.StateManager.GetOrAddAsync<IReliableDictionary<string,string>>(SettingsDictionnaryName).ConfigureAwait(false);
            using (var tx = this.StateManager.CreateTransaction())
            {
                await settings.AddOrUpdateAsync(tx, RetentionDurationSettingKey, 
                    durationInSeconds.ToString(CultureInfo.InvariantCulture), 
                    (k, v) => v = durationInSeconds.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
            ServiceEventSource.Current.ServiceRequestStop($"QueueService.SetQueueRetentionTimeAsync() : SUCCESS");
            return true;
        }


    }
}
