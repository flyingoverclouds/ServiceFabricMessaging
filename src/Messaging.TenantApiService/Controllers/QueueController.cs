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
    public class QueueController : Controller
    {
        // HACK : hardcoded tenant name for start dev
        const string applicationName = "ServiceFabricMessaging";
        const string tenantSvcName = "QT_T001";

            
        ///// <summary>
        ///// Creation of a servicefabric remoting proxy to QueueService
        ///// </summary>
        ///// <returns></returns>
        //IQueueService GetQueueServiceProxy(string tenantName,string queueName)
        //{
        //    // TODO : implement optionnal security on remoting endpoint
        //    var svcUrl = $"fabric:/{applicationName}/{tenantName}_{queueName}";
        //    var queueSvcProxy = ServiceProxy.Create<IQueueService>(new Uri(svcUrl), new ServicePartitionKey());
        //    return queueSvcProxy;
        //}


        [HttpGet]
        [Route("Create")]
        public async Task<string> Create([FromQuery]string newQueueName)
        {
            return "Not implemented";
        }



        [HttpGet]
        [Route("Delete")]
        public async Task<string> Delete([FromQuery]string queueName)
        {
            return "Not implemented";
        }

    }
}
