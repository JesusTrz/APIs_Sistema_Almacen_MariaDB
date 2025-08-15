using iTextSharp.text;
using iTextSharp.text.pdf;
using Sistema_Almacen_MariaDB.Infraestructure.Reportes;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service.Reportes
{
    public class ReporteEntradaService : IReportesEntradaService
    {
        #region REPORTE DE ENTRADAS POR FECHAS #2
        public byte[] GenerarReporteSumarizado(List<GetEntradasDto> entradas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    Rowspan = 3
                });

                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                PdfPTable tabla = new PdfPTable(9);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 1f, 1.5f, 1.5f, 2.5f, 2f, 3f, 1.2f, 1.5f, 1.8f });

                tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                tabla.AddCell(new Phrase("HORA", fontEncabezado));
                tabla.AddCell(new Phrase("PROVEEDOR", fontEncabezado));
                tabla.AddCell(new Phrase("ESTADO", fontEncabezado));
                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("CANT", fontEncabezado));
                tabla.AddCell(new Phrase("P. UNIT", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                int totalGeneralCantidad = 0;
                decimal totalGeneralImporte = 0;

                foreach (var entrada in entradas)
                {
                    bool primeraFila = true;
                    int totalEntradaCantidad = 0;
                    decimal totalEntradaImporte = 0;

                    foreach (var detalle in entrada.Detalles)
                    {
                        if (primeraFila)
                        {
                            tabla.AddCell(new Phrase(entrada.ID_Entradas.ToString(), fontNormal));
                            tabla.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Nombre_Movimiento ?? "-", fontNormal));
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                                tabla.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                        }

                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));

                        PdfPCell celdaCantidad = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal));
                        celdaCantidad.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaCantidad);

                        PdfPCell celdaPU = new PdfPCell(new Phrase(detalle.Precio_Unitario?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        celdaPU.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaPU);

                        PdfPCell celdaTotal = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaTotal);

                        totalEntradaCantidad += detalle.Cantidad ?? 0;
                        totalEntradaImporte += detalle.Total ?? 0;
                        totalGeneralCantidad += detalle.Cantidad ?? 0;
                        totalGeneralImporte += detalle.Total ?? 0;

                        primeraFila = false;
                    }

                    PdfPCell celdaTotalEntrada = new PdfPCell(new Phrase($"TOTAL ENTRADA {entrada.ID_Entradas}: {totalEntradaCantidad} | {totalEntradaImporte.ToString("C2", culturaMX)}", fontNormal));
                    celdaTotalEntrada.Colspan = 9;
                    celdaTotalEntrada.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaTotalEntrada.Border = Rectangle.NO_BORDER;
                    celdaTotalEntrada.PaddingTop = 5f;
                    celdaTotalEntrada.PaddingBottom = 10f;
                    tabla.AddCell(celdaTotalEntrada);

                    PdfPCell separador = new PdfPCell(new Phrase(" "))
                    {
                        Colspan = 9,
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColor = BaseColor.LIGHT_GRAY,
                        MinimumHeight = 5f
                    };
                    tabla.AddCell(separador);
                }

                doc.Add(tabla);
                doc.Add(new Paragraph("\n"));

                Paragraph total = new Paragraph($"TOTAL GENERAL - CANTIDAD: {totalGeneralCantidad} | TOTAL: {totalGeneralImporte.ToString("C2", culturaMX)}", fontNormal);
                total.Alignment = Element.ALIGN_RIGHT;
                doc.Add(total);

                doc.Close();
                return ms.ToArray();
            }
            #endregion
        }
        #region REPORTE ENTRADAS POR ID #1
        public byte[] GenerarReportePorEntrada(GetEntradasDto entrada)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                CultureInfo culturaMX = new CultureInfo("es-MX");

                // Logo
                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    Rowspan = 3
                });

                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase($"ENTRADA #{entrada.ID_Entradas}", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy"), fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                // Datos generales
                PdfPTable info = new PdfPTable(2);
                info.WidthPercentage = 100;
                info.SetWidths(new float[] { 2f, 3f });

                info.AddCell(new Phrase("Fecha:", fontEncabezado));
                info.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                info.AddCell(new Phrase("Hora:", fontEncabezado));
                info.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                info.AddCell(new Phrase("Proveedor:", fontEncabezado));
                info.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal));
                info.AddCell(new Phrase("Movimiento:", fontEncabezado));
                info.AddCell(new Phrase(entrada.Nombre_Movimiento ?? "-", fontNormal));

                doc.Add(info);
                doc.Add(new Paragraph("\n"));

                // Tabla de artículos
                PdfPTable tabla = new PdfPTable(5);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 3f, 2f, 2f, 2f, 2f });

                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("UNIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("P. UNITARIO", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                foreach (var detalle in entrada.Detalles)
                {
                    tabla.AddCell(new Phrase(detalle.Nombre_Articulo, fontNormal));
                    tabla.AddCell(new Phrase(detalle.Nombre_Unidad, fontNormal));

                    var celdaCantidad = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                    var celdaPU = new PdfPCell(new Phrase(detalle.Precio_Unitario?.ToString("C2", culturaMX) ?? "$0.00", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                    var celdaTotal = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };

                    tabla.AddCell(celdaCantidad);
                    tabla.AddCell(celdaPU);
                    tabla.AddCell(celdaTotal);
                }

                doc.Add(tabla);

                doc.Add(new Paragraph("\n"));

                int totalCantidad = entrada.Detalles?.Sum(d => d.Cantidad ?? 0) ?? 0;
                decimal totalImporte = entrada.Detalles?.Sum(d => d.Total ?? 0) ?? 0;

                Paragraph resumen = new Paragraph($"TOTAL CANTIDAD: {totalCantidad} | TOTAL IMPORTE: {totalImporte.ToString("C2", culturaMX)}", fontNormal);
                resumen.Alignment = Element.ALIGN_RIGHT;
                doc.Add(resumen);

                doc.Close();
                return ms.ToArray();
            }
        }

        #endregion

        #region REPORTE ENTRADAS POR PROVEEDOR #3 
        public byte[] GenerarReportePorProveedor(List<GetEntradasDto> entradas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS POR PROVEEDOR", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                // 🔍 Detectar si hay más de un proveedor
                var proveedores = entradas
                    .Where(e => !string.IsNullOrEmpty(e.Razon_Social))
                    .GroupBy(e => e.Razon_Social)
                    .OrderBy(g => g.Key);

                foreach (var grupo in proveedores)
                {
                    doc.Add(new Paragraph($"PROVEEDOR: {grupo.Key}", fontEncabezado));

                    PdfPTable tabla = new PdfPTable(7);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 1.5f, 2f, 2f, 3f, 2f, 1.5f, 2f });

                    tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                    tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                    tabla.AddCell(new Phrase("HORA", fontEncabezado));
                    tabla.AddCell(new Phrase("PROVEEDOR", fontEncabezado));
                    tabla.AddCell(new Phrase("MOVIMIENTO", fontEncabezado));
                    tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                    int totalCantidad = 0;
                    decimal totalImporte = 0;

                    foreach (var entrada in grupo)
                    {
                        int cantidad = entrada.Detalles?.Sum(d => d.Cantidad ?? 0) ?? 0;
                        decimal importe = entrada.Detalles?.Sum(d => d.Total ?? 0) ?? 0;

                        tabla.AddCell(new Phrase(entrada.ID_Entradas.ToString(), fontNormal));
                        tabla.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Nombre_Movimiento ?? "-", fontNormal));

                        PdfPCell celdaCantidad = new PdfPCell(new Phrase(cantidad.ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                        PdfPCell celdaTotal = new PdfPCell(new Phrase(importe.ToString("C2", culturaMX), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                        tabla.AddCell(celdaCantidad);
                        tabla.AddCell(celdaTotal);

                        totalCantidad += cantidad;
                        totalImporte += importe;
                    }

                    doc.Add(tabla);
                    Paragraph subtotal = new Paragraph($"Subtotal - Cantidad: {totalCantidad} | Total: {totalImporte.ToString("C2", culturaMX)}", fontNormal);
                    subtotal.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(subtotal);
                    doc.Add(new Paragraph("\n"));
                }

                doc.Close();
                return ms.ToArray();
            }
        }


        #endregion

        #region REPORTES DE ENTRADAS POR ARTICULO #4
        public byte[] GenerarReportePorArticulo(List<GetEntradasDto> entradas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS POR ARTÍCULO", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                // Agrupar por artículo
                var articulos = entradas
                    .SelectMany(e => e.Detalles.Select(d => new { Entrada = e, Detalle = d }))
                    .GroupBy(x => x.Detalle.Nombre_Articulo)
                    .OrderBy(g => g.Key);

                int totalGeneralCantidad = 0;
                decimal totalGeneralImporte = 0;

                foreach (var grupo in articulos)
                {
                    doc.Add(new Paragraph($"ARTÍCULO: {grupo.Key}", fontEncabezado));

                    PdfPTable tabla = new PdfPTable(8);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 1.5f, 2f, 2f, 3f, 2f, 1.5f, 2f, 2f });

                    tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                    tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                    tabla.AddCell(new Phrase("HORA", fontEncabezado));
                    tabla.AddCell(new Phrase("PROVEEDOR", fontEncabezado));
                    tabla.AddCell(new Phrase("UNIDAD DE MEDIDA", fontEncabezado));
                    tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("P. UNITARIO", fontEncabezado));
                    tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                    int subtotalCantidad = 0;
                    decimal subtotalImporte = 0;

                    foreach (var item in grupo)
                    {
                        var entrada = item.Entrada;
                        var d = item.Detalle;

                        tabla.AddCell(new Phrase(entrada.ID_Entradas.ToString(), fontNormal));
                        tabla.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(d.Nombre_Unidad ?? "-", fontNormal));

                        PdfPCell celdaCant = new PdfPCell(new Phrase((d.Cantidad ?? 0).ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                        PdfPCell celdaPU = new PdfPCell(new Phrase((d.Precio_Unitario ?? 0).ToString("C2", culturaMX), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                        PdfPCell celdaTotal = new PdfPCell(new Phrase((d.Total ?? 0).ToString("C2", culturaMX), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };

                        tabla.AddCell(celdaCant);
                        tabla.AddCell(celdaPU);
                        tabla.AddCell(celdaTotal);

                        subtotalCantidad += d.Cantidad ?? 0;
                        subtotalImporte += d.Total ?? 0;
                    }

                    totalGeneralCantidad += subtotalCantidad;
                    totalGeneralImporte += subtotalImporte;

                    doc.Add(tabla);
                    Paragraph subtotal = new Paragraph($"Subtotal - Cantidad: {subtotalCantidad} | Total: {subtotalImporte.ToString("C2", culturaMX)}", fontNormal);
                    subtotal.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(subtotal);
                    doc.Add(new Paragraph("\n"));
                }

                Paragraph total = new Paragraph($"TOTAL GENERAL - CANTIDAD: {totalGeneralCantidad} | TOTAL: {totalGeneralImporte.ToString("C2", culturaMX)}", fontNormal);
                total.Alignment = Element.ALIGN_RIGHT;
                doc.Add(total);

                doc.Close();
                return ms.ToArray();
            }
        }


        #endregion

        #region REPORTE ENTRADA POR MOVIMIENTO #5
        public byte[] GenerarReportePorMovimiento(List<GetEntradasDto> entradas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS POR TIPO DE MOVIMIENTO", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                PdfPTable tabla = new PdfPTable(9);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 1f, 1.5f, 1.5f, 2f, 2f, 2.5f, 1f, 1.5f, 1.5f });

                tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                tabla.AddCell(new Phrase("HORA", fontEncabezado));
                tabla.AddCell(new Phrase("MOVIMIENTO", fontEncabezado));
                tabla.AddCell(new Phrase("PROVEEDOR", fontEncabezado));
                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("CANT", fontEncabezado));
                tabla.AddCell(new Phrase("P. UNIT", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                int totalCantidad = 0;
                decimal totalImporte = 0;

                foreach (var entrada in entradas)
                {
                    foreach (var detalle in entrada.Detalles)
                    {
                        tabla.AddCell(new Phrase(entrada.ID_Entradas.ToString(), fontNormal));
                        tabla.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Nombre_Movimiento ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal)); // PROVEEDOR
                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));

                        PdfPCell celdaCant = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal));
                        celdaCant.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaCant);

                        PdfPCell celdaPU = new PdfPCell(new Phrase(detalle.Precio_Unitario?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        celdaPU.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaPU);

                        PdfPCell celdaTotal = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaTotal);

                        totalCantidad += detalle.Cantidad ?? 0;
                        totalImporte += detalle.Total ?? 0;
                    }
                }

                doc.Add(tabla);
                doc.Add(new Paragraph("\n"));

                Paragraph resumen = new Paragraph($"TOTAL GENERAL - CANTIDAD: {totalCantidad} | TOTAL: {totalImporte.ToString("C2", culturaMX)}", fontNormal);
                resumen.Alignment = Element.ALIGN_RIGHT;
                doc.Add(resumen);

                doc.Close();
                return ms.ToArray();
            }
        }

        internal static byte[] GenerarReporteEntradasPorProveedor(List<GetEntradasDto> entradas, DateTime? fechaInicio, DateTime? fechaFin, int? idProveedor, int? idSede)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region 156
        public byte[] GenerarReporteEntradasFiltrado(List<GetEntradasDto> entradas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2) { WidthPercentage = 100 };
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }

                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                PdfPTable tabla = new PdfPTable(9) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 1f, 1.5f, 1.5f, 2f, 2.5f, 2f, 2f, 1.5f, 1.5f });

                tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                tabla.AddCell(new Phrase("HORA", fontEncabezado));
                tabla.AddCell(new Phrase("MOVIMIENTO", fontEncabezado));
                tabla.AddCell(new Phrase("PROVEEDOR", fontEncabezado));
                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("UNIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                int totalGeneralCantidad = 0;
                decimal totalGeneralImporte = 0;

                foreach (var entrada in entradas)
                {
                    bool primeraFila = true;

                    foreach (var detalle in entrada.Detalles)
                    {
                        if (primeraFila)
                        {
                            tabla.AddCell(new Phrase(entrada.ID_Entradas.ToString(), fontNormal));
                            tabla.AddCell(new Phrase(entrada.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Nombre_Movimiento ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(entrada.Razon_Social ?? "-", fontNormal));
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                                tabla.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                        }

                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(detalle.Nombre_Unidad ?? "-", fontNormal));

                        PdfPCell celdaCant = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal));
                        celdaCant.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaCant);

                        PdfPCell celdaTotal = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaTotal);

                        totalGeneralCantidad += detalle.Cantidad ?? 0;
                        totalGeneralImporte += detalle.Total ?? 0;

                        primeraFila = false;
                    }

                    // Línea separadora
                    PdfPCell separador = new PdfPCell(new Phrase(" "))
                    {
                        Colspan = 9,
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColor = BaseColor.LIGHT_GRAY,
                        MinimumHeight = 5f
                    };
                    tabla.AddCell(separador);
                }

                doc.Add(tabla);
                doc.Add(new Paragraph("\n"));

                Paragraph resumen = new Paragraph($"TOTAL GENERAL - CANTIDAD: {totalGeneralCantidad} | TOTAL: {totalGeneralImporte.ToString("C2", culturaMX)}", fontNormal);
                resumen.Alignment = Element.ALIGN_RIGHT;
                doc.Add(resumen);

                doc.Close();
                return ms.ToArray();
            }
        }

        #endregion

        #region entrdads por provedor y articulo 
        public byte[] GenerarReporteEntradasPorProveedor(
     List<GetEntradasDto> entradas,
     int? idProveedor,
     DateTime? fechaInicio,
     DateTime? fechaFin)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });
                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE ENTRADAS POR PROVEEDOR", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("Generado el: " + DateTime.Now.ToString("dd/MM/yyyy"), fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                // Agrupar por proveedor
                var entradasPorProveedor = entradas.GroupBy(e => new { e.ID_Proveedores, e.Razon_Social });

                foreach (var proveedorGroup in entradasPorProveedor)
                {
                    // Encabezado del proveedor
                    doc.Add(new Paragraph($"Proveedor: {proveedorGroup.Key.Razon_Social} (ID: {proveedorGroup.Key.ID_Proveedores})", fontEncabezado));
                    doc.Add(new Paragraph("\n"));

                    PdfPTable tabla = new PdfPTable(5);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 3f, 2f, 1f, 2f, 2f });
                    tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                    tabla.AddCell(new Phrase("UNIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("ID", fontEncabezado));
                    tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                    int totalCantidad = 0;
                    decimal totalImporte = 0;

                    foreach (var entrada in proveedorGroup)
                    {
                        foreach (var detalle in entrada.Detalles)
                        {
                            tabla.AddCell(new Phrase(detalle.Nombre_Articulo, fontNormal));
                            tabla.AddCell(new Phrase(detalle.Nombre_Unidad, fontNormal));
                            tabla.AddCell(new Phrase(detalle.ID_Articulo?.ToString() ?? "-", fontNormal));

                            var celdaCantidad = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                            var celdaTotal = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };

                            tabla.AddCell(celdaCantidad);
                            tabla.AddCell(celdaTotal);

                            totalCantidad += detalle.Cantidad ?? 0;
                            totalImporte += detalle.Total ?? 0;
                        }
                    }

                    doc.Add(tabla);
                    doc.Add(new Paragraph($"\nTOTAL CANTIDAD: {totalCantidad} | TOTAL IMPORTE: {totalImporte.ToString("C2", culturaMX)}", fontNormal));
                    doc.Add(new Paragraph("\n"));
                }

                doc.Close();
                return ms.ToArray();
            }
        }


        #endregion
    }
}