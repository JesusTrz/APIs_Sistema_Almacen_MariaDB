using Microsoft.Ajax.Utilities;
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
    public class LineaController : ApiController
    {
        private readonly ILineaService _lineaService;

        public LineaController()
        {
            _lineaService = new LineaService();
        }

        #region Obtener Lineas
        [HttpGet]
        [Route("api/lineas/all")]

        public IHttpActionResult GetAllLineas()
        {
            try
            {
                var linea = _lineaService.GetAllLineas();
                return Ok(linea);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/lineas/id")]

        public IHttpActionResult GetLineasById(int id)
        {
            try
            {
                var linea = _lineaService.GetLineasById(id);
                if (linea == null)
                    return NotFound();
                return Ok(linea);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Linea
        [HttpPost]
        [Route("api/linea/cuenta")]
        public IHttpActionResult AgregarLinea(LineaDatos linea)
        {
            if (linea == null || string.IsNullOrWhiteSpace(linea.Nombre_Linea) && string.IsNullOrWhiteSpace(linea.Descripcion_Linea))
                return BadRequest("El Nombre y Descripcion no pueden ser Nulos.");
            try
            {
                _lineaService.AgregarLinea(linea);
                return Ok("Linea Agregada con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Linea
        [HttpPut]
        [Route("api/lineas/{id}")]

        public IHttpActionResult EditarLinea(int id, LineaDatos linea)
        {
            try
            {
                _lineaService.EditarLinea(id, linea);
                return Ok("Linea Actualizada correctamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
