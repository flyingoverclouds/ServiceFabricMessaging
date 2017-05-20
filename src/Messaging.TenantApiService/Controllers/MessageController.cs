﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Messaging.ServiceInterfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;


namespace Messaging.TenantApiService.Controllers
{
    /// <summary>
    /// This controller implement all api action concerning messages.
    /// </summary>
    [Route("api/[Controller]")]
    public class MessageController : Controller
    {
        // HACK : hardcoded tenant name for start dev
        const string applicationName = "ServiceFabricMessaging";
        const string tenantSvcName = "QT_T001";
        //const string queueName = "queueDeTest";

            
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
        public async Task<string> GetMessage([FromQuery]string queue)
        {
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName,queue);
            var msg = await queueSvcProxy.GetAsync("TEST").ConfigureAwait(false);
            return (msg!=null)?msg.Payload:$"";
        }


        /// <summary>
        /// delete a message from the queue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteMessage")]
        public async Task<string> DeleteMessage([FromQuery]string queue,[FromQuery]string popReceipt)
        {
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName, queue);
            var result = await queueSvcProxy.DeleteAsync(popReceipt);
            return result.ToString();
        }

        /// <summary>
        /// Insert a message in the queue
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PutMessage")]
        public async Task<string> PutMessage([FromQuery]string queue,[FromQuery]string payload,[FromQuery]string clientId)
        {
            var queueSvcProxy = GetQueueServiceProxy(tenantSvcName, queue);
            var msg=await queueSvcProxy.PutAsync(payload + " " + DateTime.Now.ToLongTimeString(), clientId??"*unknow*").ConfigureAwait(false);
            // TODO : check if null return (abnormal condition)

            // TODO : return the MEssage as json
            return $"{msg?.Id} {msg?.Payload}";
            
        }
    }
}
