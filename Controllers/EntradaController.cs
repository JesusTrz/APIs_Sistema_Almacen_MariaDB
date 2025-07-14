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
    public class EntradaController : ApiController
    {
        public readonly IEntradaService _entradaService;

        public EntradaController()
        {
            _entradaService = new EntradaService();
        }

        #region Agregar Entrada y detalles
        [HttpPost]
        [Route("api/entradas/registrar")]
        public IHttpActionResult RegistrarEntradayDetalles(EntradasDto entradasdto)
        {
            try
            {
                var entrada = _entradaService.RegistrarEntradayDetalles(entradasdto);
                return Ok(entrada);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Obtener Entradas
        [HttpGet]
        [Route("api/entradas/get")]

        public IHttpActionResult ObtenerEntradasPorSede(int idSede)
        {
            try
            {
                var entrada = _entradaService.ObtenerEntradasPorSede(idSede);
                return Ok(entrada);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Modificar entrada con detalles
        [HttpPut]
        [Route("api/entrada/modificar")]

        public IHttpActionResult ActualizarEntradasyDetalles(int idEntrada, GetEntradasDto dto)
        {
            try
            {
                bool actualizar = _entradaService.ActualizarEntradasyDetalles(idEntrada, dto);
                if (!actualizar)
                    return BadRequest("");
                return Ok("");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #region Modelo que se debe de enviar
        // {
        //  "ID_Entradas": 7,
        //  "Comentarios": "prueba de Cambio",
        //  "ID_Proveedores": 1,
        //  "ID_Movimiento": 2,
        //  "ID_Sede": 1,
        //  "Detalles": [
        //{
        //  "ID_Articulo": 8,
        //  "Cantidad": 10,
        //  "Precio_Unitario": 10
        //}
        //   ]
        //}
        #endregion
        #endregion
    }
}
