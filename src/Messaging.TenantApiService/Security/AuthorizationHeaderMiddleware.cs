using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messaging.TenantApiService.Security
{
    /// <summary>
    /// Owin middleware to implement basic StorageKey security check
    /// <see cref="https://docs.microsoft.com/en-us/rest/api/storageservices/authentication-for-the-azure-storage-services"/>
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
            // TODO : retrieve key from tenant configuration on service initialisation
            if (!await CheckAuthorizationSignature(context.Response.Headers[AuthorizationHeaderName]))
            {
                context.Response.StatusCode = 403; // access refused
                return;
            }
            await _next.Invoke(context);
        }


        async Task<bool> CheckAuthorizationSignature(string signature)
        {
            // TODO : implement signature compute such as describer here : https://docs.microsoft.com/en-us/rest/api/storageservices/authentication-for-the-azure-storage-services 
            // TODO : implement Share Access Signature support
            // TODO (optimization) : optionnal implement SAS cache on a short time to speedup authentication on consecutiv request

            return await Task.Run<bool>(() =>
            {
                // HACK : implementation of a basic key check.
                if (signature.StartsWith("DEBUGSharedKey ") && (signature.EndsWith("0123456789") || signature.EndsWith("9876543210")))
                    return true;
                return false;
            });
            
        }
    }
}
