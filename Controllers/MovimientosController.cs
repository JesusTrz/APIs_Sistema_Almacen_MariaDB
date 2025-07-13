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
    public class MovimientosController : ApiController
    {
        private readonly IMovimientosService _movimientosService;

        public MovimientosController()
        {
            _movimientosService = new MovimientosService();
        }

        #region Obtener Movimientos
        [HttpGet]
        [Route("api/movimientos/all")]

        public IHttpActionResult GetAllMovimientos()
        {
            try
            {
                var movimientos = _movimientosService.GetAllMovimientos();
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/movimientos/id")]

        public IHttpActionResult GetMovimientosById(int id)
        {
            try
            {
                var movimientos = _movimientosService.GetMovimientosById(id);
                if(movimientos == null)
                    return NotFound();  
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Movimientos
        [HttpPost]
        [Route("api/movimientos/add")]

        public IHttpActionResult AgregarNuevoMovimiento(MovimientosDatos movimiento)
        {
            if (movimiento == null || string.IsNullOrWhiteSpace(movimiento.Nombre_Movimiento)
                                   || string.IsNullOrWhiteSpace(movimiento.Descripcion_Movimiento)
                                   || string.IsNullOrWhiteSpace(movimiento.Tipo))
                return BadRequest("No puede contener datos vacios!");

            try
            {
                _movimientosService.AgregarNuevoMovimiento(movimiento);
                return Ok("Movimiento Agregado con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Movimientos
        [HttpPut]
        [Route("api/movimientos/{id}")]

        public IHttpActionResult EditarMovimiento(int id, MovimientosDatos movimiento)
        {
            try
            {
                _movimientosService.EditarMovimiento(id, movimiento);
                return Ok("Movimiento Actualizado con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Movimientos
        [HttpDelete]
        [Route("api/movimientos/{id}")]

        public IHttpActionResult EliminarMovimiento(int id)
        {
            try
            {
                _movimientosService.EliminarMovimiento(id);
                return Ok("Movimiento Actualizado con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
