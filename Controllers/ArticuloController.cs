using Mysqlx.Prepare;
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
    public class ArticuloController : ApiController
    {
        private readonly IArticulosService _articulosService;

        public ArticuloController()
        {
            _articulosService = new ArticulosService();
        }

        #region Obtener Articulos
        [HttpGet]
        [Route("api/articulos/all")]

        public IHttpActionResult GetAllArticulos()
        {
            try
            {
                var articulo = _articulosService.GetAllArticulos();
                return Ok(articulo);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/articulos/id")]

        public IHttpActionResult GetArticulosById(int id)
        {
            try
            {
                var articulo = _articulosService.GetArticulosById(id);
                if (articulo == null)
                    return NotFound();
                return Ok(articulo);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Articulo
        [HttpPost]
        [Route("api/articulo/add")]

        public IHttpActionResult AgregarArticulos(ArticulosDatos articulo)
        {
            if(articulo == null || string.IsNullOrWhiteSpace(articulo.Nombre_Articulo)
                                || string.IsNullOrWhiteSpace(articulo.Descripcion_Articulo)
                                || string.IsNullOrWhiteSpace(articulo.Numero_Parte)
                                || articulo.ID_Medida <= 0 || articulo.ID_Linea <= 0)
                return BadRequest("El Articulo no debe contener datos Vacios!");

            try
            {
                _articulosService.AgregarArticulos(articulo);
                return Ok("Articulo Agreado Exitosamente!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Articulo
        [HttpPut]
        [Route("api/articulo/{id}")]

        public IHttpActionResult EditarArticulo(int id, ArticulosDatos articulo)
        {
            try
            {
                _articulosService.EditarArticulo(id, articulo);
                return Ok("Articulo Actualizado con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Articulo
        [HttpDelete]
        [Route("api/articulo/{id}")]

        public IHttpActionResult EliminarArticulo(int id)
        {
            try
            {
                _articulosService.EliminarArticulo(id);
                return Ok("Articulo eliminado con exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
