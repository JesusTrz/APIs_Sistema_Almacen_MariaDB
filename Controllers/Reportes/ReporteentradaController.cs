using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Infraestructure.Reportes;
using Sistema_Almacen_MariaDB.Models;
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
    public class ReporteentradaController : ApiController
    {
        private readonly IEntradaService _entradaService;

        public ReporteentradaController()
        {
            _entradaService = new EntradaService();
        }

        #region Reporte Entradas Por Fechas #2
        [HttpGet]
        [Route("api/reporteentrada/entradas/fechas")]
        public HttpResponseMessage DescargarReporteSumarizado(int? idSede = null, string fechaInicio = null, string fechaFin = null)
        {
            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio))
                fechaInicioParsed = DateTime.Parse(fechaInicio);

            if (!string.IsNullOrEmpty(fechaFin))
                fechaFinParsed = DateTime.Parse(fechaFin);

            var entradas = _entradaService.ObtenerEntradasFiltradas(idSede, fechaInicioParsed, fechaFinParsed);

            var pdfBytes = new ReporteEntradaService().GenerarReporteSumarizado(entradas, fechaInicioParsed, fechaFinParsed);


            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Entradas_Fechas.pdf"
            };

            return response;
        }

        #endregion

        #region Entradas por ID_Entrada #1 
        [HttpGet]
        [Route("api/reporte/entrada/ID")]
        public HttpResponseMessage DescargarReportePorEntrada(int idSede, int idEntrada)
        {
            var entrada = _entradaService.ObtenerEntradaPorId(idEntrada, idSede);

            if (entrada == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("No se encontró la entrada con ese ID y sede.")
                };

            var pdfBytes = new ReporteEntradaService().GenerarReportePorEntrada(entrada);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"Entrada_{idEntrada}.pdf"
            };

            return response;
        }


        #endregion

        #region Entradas Por Proveedor #3
        [HttpGet]
        [Route("api/reporte/entradas/proveedor")]
        public HttpResponseMessage DescargarReporteEntradasPorProveedor(
         int? idProveedor = null,
         string fechaInicio = null,
         string fechaFin = null,
         int? idSede = null) // 👈 nuevo parámetro aquí
        {
            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio))
                fechaInicioParsed = DateTime.Parse(fechaInicio);

            if (!string.IsNullOrEmpty(fechaFin))
                fechaFinParsed = DateTime.Parse(fechaFin);

            // Obtener entradas dependiendo de los filtros
            List<GetEntradasDto> entradas;

            if (idProveedor.HasValue)
            {
                // Si hay proveedor, usa el método actual
                entradas = _entradaService.ObtenerEntradasPorProveedor(idProveedor.Value, fechaInicioParsed, fechaFinParsed, idSede.Value);
            }
            else
            {
                // Si no hay proveedor, pero se puede filtrar por sede
                entradas = _entradaService.ObtenerEntradasFiltradas(idSede, fechaInicioParsed, fechaFinParsed);
            }

            var pdfBytes = new ReporteEntradaService().GenerarReportePorProveedor(entradas, fechaInicioParsed, fechaFinParsed);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = idProveedor.HasValue ? "Entradas_Por_Proveedor.pdf" : "Entradas_Agrupadas_Por_Proveedor.pdf"
            };

            return response;
        }


        #endregion

        #region REPORTE ENTRADAS POR ARTICULO #4
        [HttpGet]
        [Route("api/reporte/entradas/articulo")]
        public HttpResponseMessage DescargarReporteEntradasPorArticulo(
     int? idArticulo = null,
     string fechaInicio = null,
     string fechaFin = null,
     int? idSede = null) // 👈 nuevo parámetro aquí
        {
            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio))
                fechaInicioParsed = DateTime.Parse(fechaInicio);

            if (!string.IsNullOrEmpty(fechaFin))
                fechaFinParsed = DateTime.Parse(fechaFin);

            var entradas = _entradaService.ObtenerEntradasPorArticulo(
                idArticulo,
                fechaInicioParsed,
                fechaFinParsed,
                idSede // 👈 pasamos sede
            );

            var pdfBytes = new ReporteEntradaService()
                .GenerarReportePorArticulo(entradas, fechaInicioParsed, fechaFinParsed);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Entradas_Por_Articulo.pdf"
            };

            return response;
        }


        #endregion

        #region ENTRADAS POR MOVIMIENTOS #5
        [HttpGet]
        [Route("api/reporte/entradas/movimiento")]
        public HttpResponseMessage DescargarReporteEntradasPorMovimiento(
    int? idMovimiento = null,
    string fechaInicio = null,
    string fechaFin = null,
    int? idSede = null) // 👈 Nuevo parámetro
        {
            DateTime? fechaInicioParsed = null;
            DateTime? fechaFinParsed = null;

            if (!string.IsNullOrEmpty(fechaInicio))
                fechaInicioParsed = DateTime.Parse(fechaInicio);

            if (!string.IsNullOrEmpty(fechaFin))
                fechaFinParsed = DateTime.Parse(fechaFin);

            var entradas = _entradaService.ObtenerEntradasPorMovimiento(
                idMovimiento,
                fechaInicioParsed,
                fechaFinParsed,
                idSede // 👈 Pasamos sede
            );

            var pdfBytes = new ReporteEntradaService()
                .GenerarReportePorMovimiento(entradas, fechaInicioParsed, fechaFinParsed);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Entradas_Por_Movimiento.pdf"
            };

            return response;
        }


        #endregion

        #region 156
        [HttpGet]
        [Route("api/reporte/entradas/filtrado")]
        public HttpResponseMessage DescargarReporteEntradasFiltrado(
    string fechaInicio = null,
    string fechaFin = null,
    int? folioInicio = null,
    int? folioFin = null,
    int? idProveedor = null,
    int? idArticulo = null,
    int? idSede = null
)
        {
            DateTime? fi = string.IsNullOrEmpty(fechaInicio) ? (DateTime?)null : DateTime.Parse(fechaInicio);
            DateTime? ff = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

            var entradas = _entradaService.ObtenerEntradasFiltradas(
                fi, ff, folioInicio, folioFin, idProveedor, idArticulo, idSede
            );

            var pdfBytes = new ReporteEntradaService().GenerarReporteEntradasFiltrado(entradas, fi, ff);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Entradas_Filtrado.pdf"
            };

            return response;
        }

        #endregion

        #region reporte entradas por proveedor y articulos 
        [HttpGet]
        [Route("api/reporte/entradas/ProveedorPorArticulo")]
        public HttpResponseMessage DescargarReporteEntradasPorProveedor(
    int? idSede = null,
    int? idProveedor = null,
    string fechaInicio = null,
    string fechaFin = null,
    int? folioInicio = null,
    int? folioFin = null)
        {
            DateTime? fi = string.IsNullOrEmpty(fechaInicio) ? (DateTime?)null : DateTime.Parse(fechaInicio);
            DateTime? ff = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

            // 1. Obtener los datos desde el servicio
            var entradas = _entradaService.ObtenerEntradasPorProveedorYArticulo(
                idSede,
                idProveedor,
                fi,
                ff,
                folioInicio,
                folioFin
            );

            // 2. Generar PDF
            var pdfBytes = new ReporteEntradaService().GenerarReporteEntradasPorProveedor(
                entradas,
                idProveedor,
                fi,
                ff
            );

            // 3. Preparar la respuesta
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Reporte_Entradas_Por_Proveedor.pdf"
            };

            return response;
        }

        #endregion
    }
}
