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
    public class InventarioController : ApiController
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController()
        {
            _inventarioService = new InventarioService();
        }

        #region Agregar Articulo a Inventario
        [HttpPost]
        [Route("api/inventario/add")]

        public IHttpActionResult AgregarArticuloaInventario(AgregarArticuloaInventario invArt)
        {
            if (invArt == null || invArt.ID_Sede <= 0)
                return BadRequest("El articulo debe tener una sede valida");
            try
            {
                _inventarioService.AgregarArticuloaInventario(invArt);
                return Ok("Articulo Agregado al inventario exitosamente!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Obtener articulos por inventario y sede
        [HttpGet]
        [Route("sede/{idSede:int}")]
        public IHttpActionResult GetInventarioPorSede(int idSede)
        {
            try
            {
                var inventario = _inventarioService.GetInventarioPorSede(idSede);
                return Ok(inventario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Stock Bajo
        [HttpGet]
        [Route("api/inventario/stockbajo")]
        public IHttpActionResult VerificarStockBajo(int idSede)
        {
            try
            {
                var stockBajo = _inventarioService.VerificarStockBajo(idSede);
                return Ok(stockBajo);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Stock Alto
        [HttpGet]
        [Route("api/inventario/stockalto")]

        public IHttpActionResult VerificarStockAlto(int idSede)
        {
            try
            {
                var stockAlto = _inventarioService.VerificarStockAlto(idSede);
                return Ok(stockAlto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Inventario de Articulo
        [HttpPut]
        [Route("editar")]
        public IHttpActionResult EditarArticuloInventario(int idInv, AgregarArticuloaInventario invArt)
        {
            try
            {
                if (invArt == null)
                    return BadRequest("Los datos del inventario son requeridos.");

                _inventarioService.EditarArticuloInventario(idInv, invArt);
                return Ok("Inventario actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al editar inventario: {ex.Message}");
            }
        }
        #endregion

        #region Eliminar Articulo de Inventario
        [HttpDelete]
        [Route("api/inventario/eliminar/{id}")]

        public IHttpActionResult EliminarArticulodeInventario(int id)
        {
            try
            {
                _inventarioService.EliminarArticulodeInventario(id);
                return Ok("Articulo Eliminado del Inventario Exitosamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Actualizar Stock
        [HttpPost]
        [Route("api/inventario/actualizar-stock")]
        public IHttpActionResult ActualizarStock([FromBody] StockEntrada entrada)
        {
            try
            {
                if (entrada == null)
                    return BadRequest("Datos inválidos.");

                var inventarioRepo = new InventarioService();

                bool actualizado = inventarioRepo.ActualizarStockArticulo(entrada);

                if (actualizado)
                    return Ok("Stock actualizado correctamente.");
                else
                    return BadRequest("No se pudo actualizar el stock.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region Base de Datos
        [HttpPut]
        [Route("api/inventario/reiniciarstocks")]

        public IHttpActionResult ReiniciarInventarioPorSede(int idSede)
        {
            try
            {
                _inventarioService.ReiniciarInventarioPorSede(idSede);
                return Ok("El Stock de Inventario fue Reiniciado Exitosamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/inventario/reiniciarcostossaldos")]

        public IHttpActionResult ReiniciarCostoSaldo(int idSede)
        {
            try
            {
                _inventarioService.ReiniciarCostoSaldo(idSede);
                return Ok("Los Costos y Saldos Fueron Reiniciados.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/inventario/ReiniciarInventario")]

        public IHttpActionResult ReiniciarInventario(int idSede)
        {
            try
            {
                _inventarioService.ReiniciarInventario(idSede);
                return Ok("El Inventario de los Articulos fué reiniciado Exitosamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/inventario/EliminarTodoslosArticulos")]

        public IHttpActionResult EliminarTodosArticulosInventario(int idSede)
        {
            try
            {
                _inventarioService.EliminarTodosArticulosInventario(idSede);
                return Ok("El Inventario se Vacio Exitosamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

    }
}
