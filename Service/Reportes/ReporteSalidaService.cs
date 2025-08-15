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
    public class ReporteSalidaService : IReporteSalidaService
    {
        #region REPORTE SALIDA FECHAS Y ID #1
        public byte[] GenerarReporteSalidas(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
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
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE SALIDAS", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }

                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                PdfPTable tabla = new PdfPTable(11);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 1f, 1.5f, 1.5f, 2f, 2f, 2f, 2.5f, 2f, 2f, 1.5f, 1.5f });

                tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                tabla.AddCell(new Phrase("HORA", fontEncabezado));
                tabla.AddCell(new Phrase("MOVIMIENTO", fontEncabezado));
                tabla.AddCell(new Phrase("PLACA", fontEncabezado));
                tabla.AddCell(new Phrase("CENTRO COSTO", fontEncabezado));
                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("UNIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("PERSONAL", fontEncabezado));
                tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                int totalGeneralCantidad = 0;
                decimal totalGeneralImporte = 0;

                foreach (var salida in salidas)
                {
                    bool primeraFila = true;

                    foreach (var detalle in salida.Detalles)
                    {
                        if (primeraFila)
                        {
                            tabla.AddCell(new Phrase(salida.ID_Salida.ToString(), fontNormal));
                            tabla.AddCell(new Phrase(salida.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(salida.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(salida.Nombre_Movimiento ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(salida.Numero_Placa ?? "-", fontNormal));
                            tabla.AddCell(new Phrase(salida.Nombre_CenCost ?? "-", fontNormal));
                        }
                        else
                        {
                            for (int i = 0; i < 6; i++)
                                tabla.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                        }

                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(detalle.Nombre_Unidad ?? "-", fontNormal));
                        tabla.AddCell(new Phrase($"{salida.Nombre}".Trim(), fontNormal));

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
                        Colspan = 11,
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
        #region REPORTE DE SALIDAS POR RANGO DE FECHAS, RNAGO DE FOLIOS, IDCENTROCOSTO,IDARTICULO,IDUNIDAD #2
        public byte[] GenerarReporteSalidasFiltrado(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
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
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE SALIDAS FILTRADAS", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                decimal totalGlobal = 0;
                foreach (var salida in salidas)
                {
                    string horaStr = salida.Hora.HasValue ? salida.Hora.Value.ToString(@"hh\:mm") : "-";
                    string fechaStr = salida.Fecha.HasValue ? salida.Fecha.Value.ToString("dd/MM/yyyy") : "-";

                    doc.Add(new Paragraph($"FOLIO: {salida.ID_Salida} | FECHA: {fechaStr} | HORA: {horaStr}", fontEncabezado));

                    doc.Add(new Paragraph($"CENTRO COSTO: {salida.Nombre_CenCost} | UNIDAD: {salida.Numero_Placa} | PERSONAL: {salida.Nombre}", fontNormal));
                    doc.Add(new Paragraph($"COMENTARIO: {salida.Comentarios}", fontNormal));
                    doc.Add(new Paragraph(" "));

                    PdfPTable tabla = new PdfPTable(5);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 3f, 2f, 1f, 2f, 2f });

                    tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                    tabla.AddCell(new Phrase("UNIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("CANT", fontEncabezado));
                    tabla.AddCell(new Phrase("P. UNIT", fontEncabezado));
                    tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                    decimal totalSalida = 0;

                    foreach (var detalle in salida.Detalles)
                    {
                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(detalle.Nombre_Unidad ?? "-", fontNormal));

                        var cant = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal));
                        cant.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(cant);

                        var pu = new PdfPCell(new Phrase(detalle.Precio_Unitario?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        pu.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(pu);

                        var total = new PdfPCell(new Phrase(detalle.Total?.ToString("C2", culturaMX) ?? "$0.00", fontNormal));
                        total.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(total);

                        totalSalida += detalle.Total ?? 0;
                    }

                    doc.Add(tabla);
                    doc.Add(new Paragraph($"TOTAL SALIDA: {totalSalida.ToString("C2", culturaMX)}", fontEncabezado) { Alignment = Element.ALIGN_RIGHT });
                    doc.Add(new Paragraph("\n"));

                    totalGlobal += totalSalida;
                }

                doc.Add(new Paragraph($"TOTAL GENERAL DE TODAS LAS SALIDAS: {totalGlobal.ToString("C2", culturaMX)}", fontEncabezado) { Alignment = Element.ALIGN_RIGHT });
                doc.Close();
                return ms.ToArray();
            }
        }

        #endregion
        #region REPORTE DE SALIDAS POR ARTICULO #3
        public byte[] GenerarReporteSalidasPorArticulo(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
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
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE SALIDAS POR ARTÍCULO", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                PdfPTable tabla = new PdfPTable(5);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 3f, 2f, 1f, 1f, 2f });

                tabla.AddCell(new Phrase("ARTÍCULO", fontEncabezado));
                tabla.AddCell(new Phrase("UNIDAD DE MEDIDA", fontEncabezado));
                tabla.AddCell(new Phrase("FOLIO DE SALIDA", fontEncabezado));
                tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                int totalCantidad = 0;
                decimal totalImporte = 0;

                foreach (var salida in salidas)
                {
                    foreach (var detalle in salida.Detalles)
                    {
                        tabla.AddCell(new Phrase(detalle.Nombre_Articulo ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(detalle.Nombre_Unidad ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(salida.ID_Salida.ToString() ?? "-", fontNormal));

                        PdfPCell celdaCantidad = new PdfPCell(new Phrase(detalle.Cantidad?.ToString() ?? "0", fontNormal));
                        celdaCantidad.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaCantidad);

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

        #endregion
        #region REPORTE SALIDAS POR MOVIMIENTO #4
        public byte[] GenerarReporteSalidasPorMovimiento(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
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
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE SALIDAS POR MOVIMIENTO", fontTitulo)) { Border = Rectangle.NO_BORDER });

                string fechaGeneracion = "GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy");
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    fechaGeneracion += $" | PERIODO: {fechaInicio.Value:dd/MM/yyyy} - {fechaFin.Value:dd/MM/yyyy}";
                }
                encabezado.AddCell(new PdfPCell(new Phrase(fechaGeneracion, fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                var movimientos = salidas.GroupBy(s => s.Nombre_Movimiento);

                int totalCantidadGlobal = 0;
                decimal totalImporteGlobal = 0;

                foreach (var grupo in movimientos)
                {
                    Paragraph tituloMovimiento = new Paragraph($"TIPO DE MOVIMIENTO: {grupo.Key}", fontEncabezado);
                    tituloMovimiento.SpacingBefore = 10f;
                    tituloMovimiento.SpacingAfter = 5f;
                    tituloMovimiento.Alignment = Element.ALIGN_LEFT;
                    doc.Add(tituloMovimiento);

                    PdfPTable tabla = new PdfPTable(5);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 1.5f, 2f, 1.5f, 1.5f, 2f });

                    tabla.AddCell(new Phrase("FOLIO", fontEncabezado));
                    tabla.AddCell(new Phrase("FECHA", fontEncabezado));
                    tabla.AddCell(new Phrase("HORA", fontEncabezado));
                    tabla.AddCell(new Phrase("CANTIDAD", fontEncabezado));
                    tabla.AddCell(new Phrase("TOTAL", fontEncabezado));

                    foreach (var salida in grupo)
                    {
                        int totalCantidad = salida.Detalles?.Sum(d => d.Cantidad ?? 0) ?? 0;
                        decimal totalImporte = salida.Detalles?.Sum(d => d.Total ?? 0) ?? 0;

                        tabla.AddCell(new Phrase(salida.ID_Salida.ToString(), fontNormal));
                        tabla.AddCell(new Phrase(salida.Fecha?.ToString("dd/MM/yyyy") ?? "-", fontNormal));
                        tabla.AddCell(new Phrase(salida.Hora?.ToString(@"hh\:mm") ?? "-", fontNormal));

                        PdfPCell celdaCantidad = new PdfPCell(new Phrase(totalCantidad.ToString(), fontNormal));
                        celdaCantidad.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaCantidad);

                        PdfPCell celdaTotal = new PdfPCell(new Phrase(totalImporte.ToString("C2", culturaMX), fontNormal));
                        celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                        tabla.AddCell(celdaTotal);

                        totalCantidadGlobal += totalCantidad;
                        totalImporteGlobal += totalImporte;
                    }

                    doc.Add(tabla);
                }

                doc.Add(new Paragraph("\n"));
                Paragraph resumen = new Paragraph($"TOTAL GENERAL - CANTIDAD: {totalCantidadGlobal} | TOTAL: {totalImporteGlobal.ToString("C2", culturaMX)}", fontNormal);
                resumen.Alignment = Element.ALIGN_RIGHT;
                doc.Add(resumen);

                doc.Close();
                return ms.ToArray();
            }
        }
        #endregion
    }
}