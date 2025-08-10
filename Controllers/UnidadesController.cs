using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using Sistema_Almacen_MariaDB.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers
{
    public class UnidadesController : ApiController
    {
        private readonly IUnidadesService _unidadesService;

        public UnidadesController()
        {
            _unidadesService = new UnidadesService();
        }

        #region Obtener Unidades
        [HttpGet]
        [Route("api/unidades/all")]

        public IHttpActionResult GetAllUnidades()
        {
            try
            {
                var unidades = _unidadesService.GetAllUnidades();
                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/unidades/id")]

        public IHttpActionResult GetUnidadesById(int id)
        {
            try
            {
                var unidades = _unidadesService.GetUnidadesById(id);
                if (unidades == null)
                    return NotFound();
                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/unidades/sede/id")]

        public IHttpActionResult GetUnidadesByIdSede(int idSede)
        {
            try
            {
                var unidades = _unidadesService.GetUnidadesByIdSede(idSede);
                if (unidades == null)
                    return NotFound();
                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Base de datos
        [HttpDelete]
        [Route("api/eliminar/todasunidades")]
        public IHttpActionResult EliminarTodasUnidades(int idSede)
        {
            try
            {
                _unidadesService.EliminarTodasUnidades(idSede);
                return Ok("Las Unidades fueron eliminadas exitosamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Agregar nuevas Unidades
        [HttpPost]
        [Route("api/unidades/add")]

        public IHttpActionResult AgregarUnidad(UnidadesDatos unidad)
        {
            if (unidad == null || string.IsNullOrWhiteSpace(unidad.Numero_Placa)
                               || string.IsNullOrWhiteSpace(unidad.Descripcion_Unidad)
                               || unidad.ID_Sede <= 0)
                return BadRequest("No puede contener datos vacios.");
            try
            {
                _unidadesService.AgregarUnidad(unidad);
                return Ok("Unidad agregada con exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Unidades
        [HttpPut]
        [Route("api/unidad/{id}")]

        public IHttpActionResult EditarUnidades(int id, UnidadesDatos unidad)
        {
            try
            {
                _unidadesService.EditarUnidades(id, unidad);
                return Ok("Unidad actualizada correctamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Unidades
        [HttpDelete]
        [Route("api/unidad/{id}")]

        public IHttpActionResult EliminarUnidad(int id)
        {
            try
            {
                _unidadesService.EliminarUnidad(id);
                return Ok("Unidad eliminada con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
