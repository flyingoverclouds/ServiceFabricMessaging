using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messaging.TenantApiService.Security
{
    /// <summary>
    /// Owin middleware to implementing basic StorageKey security check
    /// </summary>
    public class AuthorizationHeaderMiddleware
    {
        const string AuthorizationHeaderName = "Authorization";
        private readonly RequestDelegate _next;

        public AuthorizationHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(AuthorizationHeaderName))
            {
                context.Response.StatusCode = 401; // no authentication
                return;
            }
            // TODO : retrieve key from tenant configuration
            if (context.Response.Headers[AuthorizationHeaderName] == "0123456789" ||
                context.Response.Headers[AuthorizationHeaderName]== "9876543210")
            {
                context.Response.StatusCode = 403; // access refuses
                return;
            }
            await _next.Invoke(context);

        }
    }
}
