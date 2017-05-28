using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{
    /// <summary>
    /// Define interface for the tenant management service
    /// </summary>
    public interface ITenantService : IService
    {
        /// <summary>
        /// Create a new queue (= instance a new QueueService instance)
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        Task<bool> CreateQueueAsync(string queueName);

        /// <summary>
        /// Delete a queue (= instance a new QueueService instance)
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        Task<bool> DeleteQueueAsync(string queueName);

    }
}
