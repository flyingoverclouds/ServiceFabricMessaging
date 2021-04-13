using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Messaging.ServiceInterfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.AspNetCore.Authorization;

namespace Messaging.TenantApiService.Controllers
{
    /// <summary>
    /// This controller implement all api action concerning messages.
    /// </summary>
    [Route("api/[Controller]")]
    public class MessageController : Controller
    {
        // HACK : hardcoded tenant name for start dev
        const string applicationName = "ServiceFabricMessaging"; // HACK : to replace by a service setting or InitializationData
        const string tenantSvcName = "QT-T001"; // HACK : to replace by a service setting or InitializationData

            
        /// <summary>
        /// Creation of a servicefabric remoting proxy to QueueService
        /// </summary>
        /// <returns></returns>
        IQueueService GetQueueServiceProxy(string tenantName,string queueName)
        {
            // TODO : implement optionnal security on remoting endpoint
            var svcUrl = $"fabric:/{applicationName}/{tenantName}_{queueName}";
            var queueSvcProxy = ServiceProxy.Create<IQueueService>(new Uri(svcUrl), new ServicePartitionKey());
            return queueSvcProxy;
        }


        /// <summary>
        /// Get a message from the queue. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMessage")]
        public async Task<IActionResult> GetMessage([FromQuery]string queue)
        {
            ServiceEventSource.Current.ServiceRequestStart($"MessageController:GetMessage");
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName,queue);
            var msg = await queueSvcProxy.GetAsync("TEST").ConfigureAwait(false);

            // TODO : replace res by a json model
            string res = null; // HACK
            if (msg!=null)
                res=$"{msg?.Id} {msg?.Payload}";

            ServiceEventSource.Current.ServiceRequestStop($"MessageController:GetMessage: ");

            return Ok(res);
        }


        /// <summary>
        /// delete a message from the queue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromQuery]string queue,[FromQuery]string popReceipt)
        {
            ServiceEventSource.Current.ServiceRequestStart($"MessageController:DeleteMessage");
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName, queue);
            var result = await queueSvcProxy.DeleteAsync(popReceipt);
            ServiceEventSource.Current.ServiceRequestStop($"MessageController:DeleteMessage: ");
            return Ok(result.ToString());
        }

        /// <summary>
        /// Insert a message in the queue
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PutMessage")]
        public async Task<IActionResult> PutMessage([FromQuery]string queue,[FromQuery]string payload,[FromQuery]string clientId)
        {
            ServiceEventSource.Current.ServiceRequestStart($"MessageController:PutMessage");
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName, queue);
            var msg=await queueSvcProxy.PutAsync(payload + " " + DateTime.Now.ToLongTimeString(), clientId??"*unknow*").ConfigureAwait(false);
           
            // TODO : check if null return (abnormal condition)

            // TODO : return the MEssage as json

            ServiceEventSource.Current.ServiceRequestStop($"MessageController:PutMessage: ");
            return Ok($"{msg?.Id} {msg?.Payload}");
            
        }
    }
}
