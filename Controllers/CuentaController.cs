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
    public class CuentaController : ApiController
    {
        private readonly ICuentaService _cuentaService;

        public CuentaController()
        {
            _cuentaService = new CuentaService();
        }

        #region Obtener Cuentas
        [HttpGet]
        [Route("api/cuentas/all")]

        public IHttpActionResult GetAllCuentas()
        {
            try
            {
                var cuenta = _cuentaService.GetAllCuentas();
                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/cuenta/id")]

        public IHttpActionResult GetCuentaById(int id)
        {
            try
            {
                var cuenta = _cuentaService.GetCuentaById(id);
                if(cuenta == null)
                    return NotFound();
                return Ok(cuenta);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Agregar Nueva Cuenta
        [HttpPost]
        [Route("api/create/cuenta")]

        public IHttpActionResult AgregarCuenta(DatosCuenta cuenta)
        {
            if (cuenta == null || string.IsNullOrWhiteSpace(cuenta.Nombre_Cuenta) || string.IsNullOrEmpty(cuenta.Cuenta_Entrada) || string.IsNullOrEmpty(cuenta.Cuenta_Salida))
                return BadRequest("El Nombre y las Cuentas de Entrada y Salida no pueden ser Nulos.");
            try
            {
                _cuentaService.AgregarCuenta(cuenta);
                return Ok("Cuenta Agregada con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Cuenta
        [HttpPut]
        [Route("api/cuentas/{id}")]
        public IHttpActionResult EditarCuenta(int id, [FromBody] DatosCuenta cuenta)
        {
            try
            {
                _cuentaService.EditarCuenta(id, cuenta);
                return Ok("Cuenta actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Eliminar Cuenta
        [HttpDelete]
        [Route("api/cuentas/{id}")]
        public IHttpActionResult EliminarCuenta(int id)
        {
            try
            {
                _cuentaService.EliminarCuenta(id);
                return Ok("Cuenta eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

    }
}
