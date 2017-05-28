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
        // HACK : hardcoded application and tenant name for start dev

        // TODO : replace harcoded application with extraction from current Context
        const string applicationName = "ServiceFabricMessaging";

        // TODO : replace harcoded tenant name with extraction from service name (and remove trailing _API), or from InitializationData
        const string tenantSvcName = "QT_T001";


        /// <summary>
        /// Creation of a servicefabric remoting proxy to TenantService 
        /// </summary>
        /// <returns></returns>
        ITenantService GetTenantServiceProxy(string tenantName)
        {
            // TODO : implement security on remoting endpoint

            var svcUrl = $"fabric:/{applicationName}/{tenantName}";
            var queueSvcProxy = ServiceProxy.Create<ITenantService>(new Uri(svcUrl), new ServicePartitionKey());
            return queueSvcProxy;
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create([FromQuery]string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed($"QueueController:Create: Invalid queueName : {queueName}");
                return BadRequest("invalid queueName");
            }
            try
            {
                var tenantProxy = GetTenantServiceProxy(tenantSvcName);
                var res = await tenantProxy.CreateQueueAsync(queueName);
                return Ok(res);
            }
            catch(Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed($"QueueController:Create: unmanaged exception : {ex.ToString()}");

                return StatusCode(500, "'Server Error. Logged and analysing ...");
            }
        }



        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Remove([FromQuery]string queueName)
        {
            // TODO : Update Exception management to return http response
            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed($"QueueController:Create: Invalid queueName : {queueName}");
                return BadRequest("invalid queueName");
            }

            try
            {
                var tenantProxy = GetTenantServiceProxy(tenantSvcName);
                var res = await tenantProxy.DeleteQueueAsync(queueName);
                return Ok(res);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed($"QueueController:Remove: unmanaged exception : {ex.ToString()}");
                return StatusCode(500, "'Server Error. Logged and analysing ...");
            }


        }

    }
}
