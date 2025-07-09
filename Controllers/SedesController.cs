using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using Sistema_Almacen_MariaDB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers
{
    public class SedesController : ApiController
    {
        private readonly ISedesService _sedesService;

        public SedesController()
        {
            _sedesService = new SedesService();
        }

        #region Obtener Sedes
        [HttpGet]
        [Route("api/sedes/all")]
        public IHttpActionResult GetAllSedes()
        {
            try
            {
                var sedes = _sedesService.GetAllSedes();
                return Ok(sedes);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }
        #endregion

        #region Obtener Sedes por Id
        [HttpGet]
        [Route("api/sedes/{id}")]
        public IHttpActionResult GetSedeById(int id)
        {
            try
            {
                var sede = _sedesService.GetSedeById(id);
                if (sede == null) 
                    return NotFound();
                return Ok(sede);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Crear Sedes
        [HttpPost]
        [Route("api/create/sedes")]

        public IHttpActionResult CrearSede(NombreSedeDto sede)
        {
            if (sede == null || string.IsNullOrWhiteSpace(sede.Nombre_Sede))
                return BadRequest("El Nombre de la Sede es Obligatorio.");

            try
            {
                _sedesService.CrearSede(sede);
                return Ok("Sede Creada con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        } 
        #endregion
    }
}
