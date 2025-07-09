using MySql.Data.MySqlClient;
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
    public class UnidadesMedidaController : ApiController
    {
        private readonly IUnidadesMedidaService _unidadesMedidaService;

        public UnidadesMedidaController()
        {
            _unidadesMedidaService = new UnidadesMedidaService();
        }

        #region Obtener Datos
        [HttpGet]
        [Route("api/unidades_medida/all")]

        public IHttpActionResult GetAllUMedida()
        {
            try
            {
                var medida = _unidadesMedidaService.GetAllUMedida();
                return Ok(medida);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/unidades_medida/id")]

        public IHttpActionResult GetMedidaById(int id)
        {
            try
            {
                var medida = _unidadesMedidaService.GetMedidaById(id);
                if (medida == null)
                    return NotFound();
                return Ok(medida);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Unidad de Medida
        [HttpPost]
        [Route("api/create/unidad_medida")]

        public IHttpActionResult AgregarUnidadMedida(UnidadesMedidaNombre medida)
        {
            if (medida == null || string.IsNullOrWhiteSpace(medida.Nombre_Unidad))
                return BadRequest("El Nombre no debe ser Nulo");
            try
            {
                _unidadesMedidaService.AgregarUnidadMedida(medida);
                return Ok("Unidad de Medida Agregada correctamente!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Unidad de Medida
        [HttpPut]
        [Route("api/unidades-medida/{id}")]

        public IHttpActionResult EditarUnidadMedida(int id, string nuevoNombre)
        {
            try
            {
                _unidadesMedidaService.EditarUnidadMedida(id, nuevoNombre);
                return Ok("Unidad de medida actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Unidad de Medida
        [HttpDelete]
        [Route("api/unidades-medida/{id}")]
        public IHttpActionResult EliminarUnidad(int id)
        {
            try
            {
                _unidadesMedidaService.EliminarUnidadMedida(id);
                return Ok("Unidad de medida eliminada. Los artículos afectados ahora usan 'Desconocido'.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
