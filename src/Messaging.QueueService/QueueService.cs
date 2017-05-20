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

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }


        public async Task<Message> PutAsync(string messagePayload, string clientId)
        {
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<Message>>("mainQueue").ConfigureAwait(false);
            var msg = new Message()
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                DequeueCount = 0,
                InsertionDate = DateTime.UtcNow,
                ExpirationDate=DateTime.MinValue,
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
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<Message>>("mainQueue").ConfigureAwait(false);
            using (var tx = this.StateManager.CreateTransaction())
            {
                var msgCV = await queue.TryDequeueAsync(tx).ConfigureAwait(false);
                if (msgCV.HasValue)
                {
                    // TODO add msg ID to popedMessageQueue
                    // TODO add msg to PopedMEssageDictionnary
                    await tx.CommitAsync().ConfigureAwait(false);
                    return msgCV.Value;
                }
            }
            return null;
        }

        public async Task<bool> DeleteAsync(string popReceipt)
        {
            // check ig message is in PopepMessageDictionnary.
            throw new NotImplementedException();
        }



    }
}
