using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Messaging.TenantApiService.Controllers
{
    /// <summary>
    /// This controller implement all api action concerning messages.
    /// </summary>
    [Route("api/[Controller]")]
    public class MessageController : Controller
    {
        // HACK : hardcoded tenant name for start dev
        const string tenantSvcName = "QT_T001";
        const string queueSvcName = "QT_T001_queueDeTest";

        /// <summary>
        /// Get a message from the queue. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMessage")]
        public async Task<string> GetMessage()
        {
            return "Message de l'api " + DateTime.Now.ToLongTimeString();
        }


        /// <summary>
        /// delete a message from the queue
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("DeleteMessage")]
        public async Task<string> DeleteMessage(string id)
        {
            return "Message de l'api " + DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        /// Insert a message in the queue
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpGet("{payload}")]
        [Route("PutMessage")]
        public async Task<string> PutMessage(string payload)
        {
            return "new message : " + payload;
        }
    }
}
