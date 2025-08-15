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
    public class SalidaController : ApiController
    {
        public readonly ISalidaService _salidaService;

        public SalidaController()
        {
            _salidaService = new SalidaService();
        }

        #region Agregar Salida
        [HttpPost]
        [Route("api/salidas/add")]

        public IHttpActionResult RegistrarSalidasyDetalles(SalidaDto salidaDto)
        {
            try
            {
                var salida = _salidaService.RegistrarSalidasyDetalles(salidaDto);
                return Ok(salida);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Obtener Salida y Detalles
        [HttpGet]
        [Route("api/salidas/get")]

        public IHttpActionResult ObtenerSalidasporSede(int idSede)
        {
            try
            {
                var salida = _salidaService.ObtenerSalidasporSede(idSede);
                return Ok(salida);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Eliminar Salidas
        [HttpDelete]
        [Route("api/salidas/delete/salidas")]

        public IHttpActionResult EliminarSalidas(int id)
        {
            try
            {
                _salidaService.EliminarSalidas(id);
                return Ok("Salida Eliminada Correctamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/salidas/delete/eliminar-articulo")]

        public IHttpActionResult EliminarArticulosSalidas(int idSalida, int idArticulo)
        {
            try
            {
                _salidaService.EliminarArticulosSalidas(idSalida, idArticulo);
                return Ok("Articulo Eliminado correctamente de la salida.");
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al eliminar el artículo: " + ex.Message));
            }
        }
        #endregion

        #region Modificar salidas con detalles
        [HttpPut]
        [Route("api/salidas/put")]

        public IHttpActionResult ActualizarSalidasyDetalles(int idSalida, GetSalidasDto dto)
        {
            try
            {
                bool actualizar = _salidaService.ActualizarSalidasyDetalles(idSalida,  dto);
                if(!actualizar)
                    return BadRequest("Error al Actualizar slaida.");
                return Ok("Salida actualizada con exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #region Json
        //{
        //  "ID_Movimiento": 1,
        //  "ID_CenCost": 1,
        //  "ID_Unidad": 1,
        //  "ID_Personal": 1,
        //  "Comentarios": "Prueba de Comentario 2",
        //  "ID_Sede": 1,
        //  "Detalles": [
        //    {
        //      "ID_Articulo": 8,
        //      "Cantidad": 7,
        //      "Precio_Unitario": 15
        //    }
        //  ]
        //}
        #endregion
        #endregion
    }
}
