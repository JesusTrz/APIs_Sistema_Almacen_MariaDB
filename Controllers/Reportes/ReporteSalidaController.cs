using Sistema_Almacen_MariaDB.Infraestructure;
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
    public class ReporteSalidaController : ApiController
    {
        private readonly ISalidaService _salidaService;

        public ReporteSalidaController()
        {
            _salidaService = new SalidaService();
        }

        #region REPORTE SALIDA  POR FECHA Y ID #1
        [HttpGet]
        [Route("api/reporte/salidas")]
        public HttpResponseMessage DescargarReporteSalidas(
       int? idSalida = null,
       string fechaInicio = null,
       string fechaFin = null,
       int? idSede = null)
        {
            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio))
                fechaInicioParsed = DateTime.Parse(fechaInicio);

            if (!string.IsNullOrEmpty(fechaFin))
                fechaFinParsed = DateTime.Parse(fechaFin);

            var salidas = _salidaService.ObtenerSalidas(
                idSalida,
                fechaInicioParsed,
                fechaFinParsed,
                idSede
            );

            var pdfBytes = new ReporteSalidaService()
                .GenerarReporteSalidas(salidas, fechaInicioParsed, fechaFinParsed);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Salidas.pdf"
            };

            return response;
        }

        #endregion

        #region REPORTE DE SALIDAS POR RANGO DE FECHAS, RNAGO DE FOLIOS, IDCENTROCOSTO,IDARTICULO,IDUNIDAD #2
        [HttpGet]
        [Route("api/reporte/salidas/filtrado")]
        public HttpResponseMessage DescargarReporteSalidasFiltrado(
      string fechaInicio = null,
      string fechaFin = null,
      int? folioInicio = null,
      int? folioFin = null,
      int? idCentroCosto = null,
      int? idUnidad = null,
      int? idArticulo = null,
      int? idSede = null // Nuevo parámetro para filtrar por sede
  )
        {
            DateTime? fi = string.IsNullOrEmpty(fechaInicio) ? (DateTime?)null : DateTime.Parse(fechaInicio);
            DateTime? ff = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

            var salidas = _salidaService.ObtenerSalidasFiltradas(
                fi, ff, folioInicio, folioFin, idCentroCosto, idUnidad, idArticulo, idSede
            );

            var pdfBytes = new ReporteSalidaService().GenerarReporteSalidasFiltrado(salidas, fi, ff);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Salidas_Filtrado.pdf"
            };

            return response;
        }

        #endregion
        #region SALIDAS POR ARTICULO   #3
        [HttpGet]
        [Route("api/reporte/salidas/articulo")]
        public HttpResponseMessage DescargarReporteSalidasPorArticulo(
           string fechaInicio = null,
           string fechaFin = null,
           int? idArticulo = null,
           int? idSede = null)
        {
            DateTime? fi = string.IsNullOrEmpty(fechaInicio) ? (DateTime?)null : DateTime.Parse(fechaInicio);
            DateTime? ff = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

            var salidas = _salidaService.ObtenerSalidasPorArticulo(fi, ff, idArticulo, idSede);

            var pdfBytes = new ReporteSalidaService()
                .GenerarReporteSalidasPorArticulo(salidas, fi, ff);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Salidas_Por_Articulo.pdf"
            };

            return response;
        }

        #endregion

        #region REPORTE SALIDAS POR MOVIMIENTO #4
        [HttpGet]
        [Route("api/reporte/salidas/movimiento")]
        public HttpResponseMessage DescargarReporteSalidasPorMovimiento(
            string fechaInicio = null,
            string fechaFin = null,
            int? idMovimiento = null,
            int? idSede = null) // 👈 nuevo parámetro
        {
            DateTime? fi = string.IsNullOrEmpty(fechaInicio) ? (DateTime?)null : DateTime.Parse(fechaInicio);
            DateTime? ff = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

            var salidas = _salidaService.ObtenerSalidasPorMovimiento(fi, ff, idMovimiento, idSede);

            var pdfBytes = new ReporteSalidaService()
                .GenerarReporteSalidasPorMovimiento(salidas, fi, ff);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Salidas_Por_Movimiento.pdf"
            };

            return response;
        }

        #endregion

    }
}
