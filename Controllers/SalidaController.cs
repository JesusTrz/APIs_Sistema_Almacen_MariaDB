using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers
{
    public class SalidaController : ApiController
    {
        public readonly ISalidaService _salidaService;

        public SalidaController()
        {
            _salidaService = new SalidaService();
        }
    }
}
