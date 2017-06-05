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
        private ITenantService GetTenantServiceProxy(string tenantName)
        {
            // TODO : implement security on remoting endpoint

            var svcUrl = $"fabric:/{applicationName}/{tenantName}";
            var queueSvcProxy = ServiceProxy.Create<ITenantService>(new Uri(svcUrl), new ServicePartitionKey());
            return queueSvcProxy;
        }

        /// <summary>
        /// Creation of a servicefabric remoting proxy to QueueService
        /// </summary>
        /// <returns></returns>
        private IQueueService GetQueueServiceProxy(string tenantName, string queueName)
        {
            // TODO : implement optionnal security on remoting endpoint
            var svcUrl = $"fabric:/{applicationName}/{tenantName}_{queueName}";
            var queueSvcProxy = ServiceProxy.Create<IQueueService>(new Uri(svcUrl), new ServicePartitionKey());
            return queueSvcProxy;
        }



        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create([FromQuery]string queueName)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueController:Create");

            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:Create: Invalid queueName : {queueName}");
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
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:Create: unmanaged exception : {ex.ToString()}");

                return StatusCode(500, "'Server Error. Logged and analysing ...");
            }
        }



        [HttpGet]
        [Route("Remove")]
        public async Task<IActionResult> Remove([FromQuery]string queueName)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueController:Remove");

            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:Remove: Invalid queueName : {queueName}");
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
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:Remove: unmanaged exception : {ex.ToString()}");
                return StatusCode(500, "'Server Error. Logged and analysing ...");
            }


        }

        [HttpGet]
        [Route("SetRetentionTime")]
        public async Task<IActionResult> SetRetentionDuration([FromQuery]string queueName,[FromQuery] int durationInSeconds)
        {
            ServiceEventSource.Current.ServiceRequestStart($"QueueController:SetRetentionTime");
            if (string.IsNullOrEmpty(queueName))
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:SetRetentionTime: Invalid queueName : {queueName}");
                return BadRequest("invalid queueName");
            }
            if (durationInSeconds < 1 || durationInSeconds > 2678400) // id duration is neg or higher than 31days --> error
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:SetRetentionTime: Invalid duration : {durationInSeconds}");
                return BadRequest("invalid duration (should between 1sec and 31days)");
            }

            try
            {
                var queueProxy = GetQueueServiceProxy(tenantSvcName,queueName);
                var res = await queueProxy.SetQueueRetentionTimeAsync(durationInSeconds,Guid.Empty);
                return Ok(res);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceRequestStop($"QueueController:SetRetentionTime: unmanaged exception : {ex.ToString()}");
                return StatusCode(500, "'Server Error. Logged and analysing ...");
            }

        }
    }
}
