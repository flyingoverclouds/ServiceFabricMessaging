using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.ServiceInterfaces
{
    public interface IQueueService : IService
    {
        /// <summary>
        /// Insert a new message in the queue
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="messagePayload"></param>
        /// <returns>instance of the Message created by the service</returns>
        Task<Message> PutAsync(string messagePayload,string clientId);

        /// <summary>
        /// Return the first message available. 
        /// Null if queue is empty
        /// </summary>
        /// <param name="clientId">id of client app</param>
        /// <returns>message or null if empty queue</returns>
        Task<Message> GetAsync(string clientId);

        /// <summary>
        /// delete the message 
        /// </summary>
        /// <param name="popReceipt">receipt of the Get operation</param>
        /// <returns>true : deletion succeeded, false otherwise (message back to queue or already deleted)</returns>
        Task<bool> DeleteAsync(string popReceipt);   
    }
}
