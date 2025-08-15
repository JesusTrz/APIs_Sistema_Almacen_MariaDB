using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Infraestructure.Reportes;
using Sistema_Almacen_MariaDB.Service;
using Sistema_Almacen_MariaDB.Service.Reportes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers.Reportes
{
    public class ReporteInventarioController : ApiController
    {

        private readonly IInventarioService _inventarioService;
        private readonly IReporteinventarioService _reporteService;

        public ReporteInventarioController()
        {
            _inventarioService = new InventarioService();
            _reporteService = new ReporteInventarioService();
        }

        #region REPORTE INVENTARIO FILTRADO #1
        [HttpGet]
        [Route("api/reportes/inventario")]
        public HttpResponseMessage GetReporteInventario(
     int idSede,
     int? idArticulo = null,
     bool agruparPorUbicacion = false,
     bool agruparPorLinea = false,
     string fechaInicio = null,
     string fechaFin = null)
        {

            var inventario = _inventarioService.ObtenerInventarioPorSede(idSede);


            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out DateTime fi))
                fechaInicioParsed = fi;

            if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out DateTime ff))
                fechaFinParsed = ff;

            if (idArticulo.HasValue)
                inventario = inventario.Where(i => i.ID_Articulo == idArticulo.Value).ToList();


            var pdfBytes = _reporteService.GenerarReporteInventario(
                inventario,
                idArticulo,
                agruparPorUbicacion,
                agruparPorLinea,
                fechaInicioParsed,
                fechaFinParsed
            );


            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(pdfBytes);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Inventario.pdf"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }



        #endregion
        #region FUERA DE ESTOCK #2

        [HttpGet]
        [Route("api/reportes/articulos-fuera-stock")]
        public HttpResponseMessage GetReporteFueraStock(int idSede, bool menoresMinimo = true, bool mayoresMaximo = false)
        {
            var inventario = _inventarioService.ObtenerInventarioPorSede(idSede);

            var pdfBytes = _reporteService.GenerarReporteArticulosFueraDeStock(inventario, menoresMinimo, mayoresMaximo);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(pdfBytes);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Articulos_Fuera_Stock.pdf"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }
    }

    #endregion
}

