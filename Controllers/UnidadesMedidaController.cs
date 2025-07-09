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
    }
}
