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

namespace Messaging.TenantService
{
    /// <summary>
    /// This class manage a queue tenant (like a storage account in azure). 
    /// This service is responsible of queue creation/deletion/management. 
    /// It maintains a persisted list of queue it manage. 
    /// </summary>
    internal sealed class TenantService : StatefulService, ITenantService
    {
        // TODO : replace hardcoded servioceType name by a setting
        const string QueueServiceTypeName = "QueueServiceType";
        const string QueueServiceTypeVersion = "1.0.0";

        public TenantService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Create a new queueSerbvice instance in the same servicefabric application (Tenant and Queue are in the same application)
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public async Task<bool> CreateQueueAsync(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceMessage(this.Context,$"TenantService.CreateQueueAsync() : invalid parametervalue queueName='{queueName}'");
                return false;
            }
            // TODO : add queuename charset check to avoid invalid service instance name


            try
            {
                
                var newQueueSvcInstanceName = this.Context.ServiceName.ToString() + "_" + queueName; // get the tenant name
                FabricClient fc = new FabricClient();

                // TODO : check is a serviceintance already own the same name (if yes -> add a random string to unicize name)

                var svcDescription = new System.Fabric.Description.StatefulServiceDescription()
                {
                    ApplicationName = new Uri(this.Context.CodePackageActivationContext.ApplicationName),
                    ServiceName = new Uri(newQueueSvcInstanceName),
                    HasPersistedState = true,
                    ServiceTypeName = QueueServiceTypeName,
                    TargetReplicaSetSize = 3,
                    MinReplicaSetSize = 1,
                    PartitionSchemeDescription = new System.Fabric.Description.SingletonPartitionSchemeDescription()
                };

                await fc.ServiceManager.CreateServiceAsync(svcDescription);

                // TODO : add the queue name to the list of managed queue by this tenant instance

                return true;
            }
            catch(Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.CreateQueueAsunc() : EXCEPTION : {ex.ToString()}");
                return false;
            }
        }

        public async Task<bool> DeleteQueueAsync(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.DeleteQueueAsync() : invalid parametervalue queueName='{queueName}'");
                return false;
            }

            try
            {

                var newQueueSvcInstanceName = this.Context.ServiceName.ToString() + "_" + queueName; // get the tenant name
                FabricClient fc = new FabricClient();
                var dsdQueue = new System.Fabric.Description.DeleteServiceDescription(new Uri(newQueueSvcInstanceName));
                await fc.ServiceManager.DeleteServiceAsync(dsdQueue);

                // TODO : remove the queue name to the list of managed queue by this tenant instance

                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.CreateQueueAsunc() : EXCEPTION : {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Add Remoting listener for ITenantService.
        /// </summary>
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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //using (var tx = this.StateManager.CreateTransaction())
                //{
                //    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                //    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                //        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                //    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                //    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                //    // discarded, and nothing is saved to the secondary replicas.
                //    await tx.CommitAsync();
                //}

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
