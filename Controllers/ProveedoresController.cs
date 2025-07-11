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
    public class ProveedoresController : ApiController
    {
        private readonly IProveedoresService _proveedoresService;

        public ProveedoresController()
        {
            _proveedoresService = new ProveedoresService();
        }

        #region Obtener Proveedores
        [HttpGet]
        [Route("api/proveedores/all")]

        public IHttpActionResult GetAllProveedores()
        {
            try
            {
                var proveedores = _proveedoresService.GetAllProveedores();
                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/proveedores/id")]

        public IHttpActionResult GetProveedoresById(int id)
        {
            try
            {
                var proveedores = _proveedoresService.GetProveedoresById(id);
                if (proveedores == null)
                    return NotFound();
                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Proveedor
        [HttpPost]
        [Route("api/proveedores/add")]

        public IHttpActionResult AgregarProveedor(ProveedoresDatos proveedores)
        {
            if (proveedores == null || string.IsNullOrWhiteSpace(proveedores.Razon_Social)
                || string.IsNullOrWhiteSpace(proveedores.RFC)
                || string.IsNullOrWhiteSpace(proveedores.Direccion)
                || string.IsNullOrEmpty(proveedores.Telefono)
                || string.IsNullOrWhiteSpace(proveedores.Email))
                return BadRequest("No Pueden contener datos Nulos.");
            try
            {
                _proveedoresService.AgregarProveedor(proveedores);
                return Ok("Proveedor Agregado con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Proveedores
        [HttpPut]
        [Route("api/proveedores/{id}")]

        public IHttpActionResult EditarProveedor(int id, ProveedoresDatos proveedores)
        {
            try
            {
                _proveedoresService.EditarProveedor(id, proveedores);
                return Ok("Proveedor actualizado correctramente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Proveedores
        [HttpDelete]
        [Route("api/proveedores/{id}")]

        public IHttpActionResult EliminarProveedor(int id)
        {
            try
            {
                _proveedoresService.EliminarProveedor(id);
                return Ok("Proveedor eliminado con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
