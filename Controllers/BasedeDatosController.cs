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
    public class BasedeDatosController : ApiController
    {
        private readonly IBase_de_Datos_Service _db;

        public BasedeDatosController()
        {
            _db = new Base_de_Datos_Service();
        }

        [HttpDelete]
        [Route("api/basedatos/eliminartodo")]

        public IHttpActionResult VaciarBaseDeDatos(int idRol)
        {
            try
            {
                _db.VaciarBaseDeDatos(idRol);
                return Ok("Toda la Base de datos fué Reiniciada.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
