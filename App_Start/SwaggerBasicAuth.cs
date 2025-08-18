using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Sistema_Almacen_MariaDB.App_Start
{
    public class SwaggerBasicAuth : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var path = actionContext.Request.RequestUri.AbsolutePath.ToLower();

            // Solo proteger rutas que tengan /swagger
            if (!path.Contains("/swagger"))
            {
                return; // no aplica seguridad al resto de la API
            }

            var authHeader = actionContext.Request.Headers.Authorization;

            if (authHeader != null && authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                var user = credentials[0];
                var password = credentials[1];

                if (user == "ADMIN" && password == "ADMIN123")
                {
                    return;
                }
            }
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            actionContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Swagger\"");
        }
    }
}