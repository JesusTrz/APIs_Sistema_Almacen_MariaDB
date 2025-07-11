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
    public class CentroCostoController : ApiController
    {
        private readonly ICentroCostoService _centroCostoService;

        public CentroCostoController()
        {
            _centroCostoService = new CentroCostoService();
        }

        #region Obtener Centros de Costo
        [HttpGet]
        [Route("api/centro/all")]

        public IHttpActionResult GetAllCenCost()
        {
            try
            {
                var centro = _centroCostoService.GetAllCenCost();
                return Ok(centro);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/centro/id")] 

        public IHttpActionResult GetCenCostById(int id)
        {
            try
            {
                var centro = _centroCostoService.GetCenCostById(id);
                if (centro == null)
                    return NotFound();
                return Ok(centro);  
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Centro de costo
        [HttpPost]
        [Route("api/CentroCosto/cuenta")]

        public IHttpActionResult AgregarCentro(CenCostoDatos centro)
        {
            if(centro == null || string.IsNullOrWhiteSpace(centro.Nombre_CenCost) || string.IsNullOrWhiteSpace(centro.Descripcion_CenCost))
                return BadRequest("El Nombre y Descripcion no pueden ser Nulos.");

            try
            {
                _centroCostoService.AgregarCentro(centro);
                return Ok("Centro de Costo agregado con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Centro de Costo
        [HttpPut]
        [Route("api/centro/{id}")]

        public IHttpActionResult EditarCentrodeCosto(int id, CenCostoDatos centro)
        {
            try
            {
                _centroCostoService.EditarCentrodeCosto(id, centro);
                return Ok("Centro de costo actualizado correctamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Centro de Costo
        [HttpDelete]
        [Route("api/centro/{id}")]

        public IHttpActionResult EliminarCentro(int id)
        {
            try
            {
                _centroCostoService.EliminarCentro(id);
                return Ok("Centro de Costo eliminado correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
