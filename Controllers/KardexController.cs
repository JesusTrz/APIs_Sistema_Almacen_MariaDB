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
    public class KardexController : ApiController
    {
        private readonly IKardexService _kardexService;

        public KardexController()
        {
            _kardexService = new KardexService();
        }

        [HttpGet]
        [Route("api/kardex")]

        public IHttpActionResult ObtenerKardexPorArticulo(int idSede, int idArticulo)
        {
            try
            {
                var kardex = _kardexService.ObtenerKardexPorArticulo(idSede, idArticulo);
                return Ok(kardex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
