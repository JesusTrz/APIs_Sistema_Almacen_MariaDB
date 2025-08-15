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
    public class ReporteInventarioService : IReporteinventarioService
    {
        #region REPORTE INVENTARIO FILTRADO #1
        public byte[] GenerarReporteInventario(
     List<InventarioArticulos> inventario,
     int? idArticulo = null,
     bool agruparPorUbicacion = false,
     bool agruparPorLinea = false,
     DateTime? fechaInicio = null,
     DateTime? fechaFin = null)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE INVENTARIO", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy"), fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    doc.Add(new Paragraph($"FILTRO POR RANGO DE FECHAS: {fechaInicio.Value.ToString("dd/MM/yyyy")} - {fechaFin.Value.ToString("dd/MM/yyyy")}", fontSubtitulo));
                    doc.Add(new Paragraph("\n"));
                }

                if (agruparPorLinea)
                    doc.Add(new Paragraph("REPORTE AGRUPADO POR LÍNEA", fontSubtitulo));
                else if (agruparPorUbicacion)
                    doc.Add(new Paragraph("REPORTE AGRUPADO POR UBICACIÓN", fontSubtitulo));
                else if (idArticulo.HasValue)
                    doc.Add(new Paragraph("REPORTE FILTRADO POR ARTÍCULO", fontSubtitulo));

                doc.Add(new Paragraph("\n"));

                // 👉 Aplicar filtro por fechas antes de agrupar
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    inventario = inventario.Where(i =>
                        i.Ultima_Compra.HasValue &&
                        i.Ultima_Compra.Value.Date >= fechaInicio.Value.Date &&
                        i.Ultima_Compra.Value.Date <= fechaFin.Value.Date
                    ).ToList();
                }

                if (agruparPorLinea)
                {
                    var grupos = inventario.GroupBy(i => i.Nombre_Linea ?? "SIN LÍNEA");
                    foreach (var grupo in grupos)
                    {
                        doc.Add(new Paragraph("LÍNEA: " + grupo.Key, fontEncabezado));
                        doc.Add(GenerarTablaInventario(grupo.ToList(), fontEncabezado, fontNormal, culturaMX));
                        doc.Add(new Paragraph("\n"));
                    }
                }
                else if (agruparPorUbicacion)
                {
                    var grupos = inventario.GroupBy(i => i.Ubicacion ?? "SIN UBICACIÓN");
                    foreach (var grupo in grupos)
                    {
                        doc.Add(new Paragraph("UBICACIÓN: " + grupo.Key, fontEncabezado));
                        doc.Add(GenerarTablaInventario(grupo.ToList(), fontEncabezado, fontNormal, culturaMX));
                        doc.Add(new Paragraph("\n"));
                    }
                }
                else
                {
                    doc.Add(GenerarTablaInventario(inventario, fontEncabezado, fontNormal, culturaMX));
                }

                doc.Close();
                return ms.ToArray();
            }
        }




        private PdfPTable GenerarTablaInventario(List<InventarioArticulos> datos, Font fontEncabezado, Font fontNormal, CultureInfo culturaMX)
        {
            PdfPTable tabla = new PdfPTable(6);
            tabla.WidthPercentage = 100;
            tabla.SetWidths(new float[] { 3f, 2f, 1f, 1.5f, 1.5f, 2f });

            tabla.AddCell(new Phrase("DESCRIPCIÓN", fontEncabezado));
            tabla.AddCell(new Phrase("UNIDAD DE MEDIDA", fontEncabezado));
            tabla.AddCell(new Phrase("ID", fontEncabezado));
            tabla.AddCell(new Phrase("EXISTENCIA", fontEncabezado));
            tabla.AddCell(new Phrase("COSTO PROM.", fontEncabezado));
            tabla.AddCell(new Phrase("SALDO", fontEncabezado));

            foreach (var item in datos)
            {
                tabla.AddCell(new Phrase(item.Nombre_Articulo ?? "-", fontNormal));
                tabla.AddCell(new Phrase(item.Nombre_Unidad ?? "-", fontNormal));
                tabla.AddCell(new Phrase(item.ID_Articulo?.ToString() ?? "-", fontNormal));

                PdfPCell celdaExistencia = new PdfPCell(new Phrase(item.Stock_Actual?.ToString() ?? "0", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                PdfPCell celdaCosto = new PdfPCell(new Phrase(item.Costo_Promedio?.ToString("C2", culturaMX) ?? "$0.00", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };
                PdfPCell celdaSaldo = new PdfPCell(new Phrase(item.Saldo?.ToString("C2", culturaMX) ?? "$0.00", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT };

                tabla.AddCell(celdaExistencia);
                tabla.AddCell(celdaCosto);
                tabla.AddCell(celdaSaldo);
            }

            return tabla;
        }
        #endregion

        #region FUERA DE ESTOCK #2
        public byte[] GenerarReporteArticulosFueraDeStock(List<InventarioArticulos> inventario, bool menoresMinimo, bool mayoresMaximo)
        {
            var fueraDeStock = inventario.Where(a =>
                (menoresMinimo && a.Stock_Actual < a.Stock_Minimo) ||
                (mayoresMaximo && a.Stock_Actual > a.Stock_Maximo)).ToList();

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var fontEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);
                CultureInfo culturaMX = new CultureInfo("es-MX");

                // Agregar Logo y Encabezado
                string logoPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Logo_Thh.png");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);

                PdfPTable encabezado = new PdfPTable(2);
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 1.2f, 4f });

                encabezado.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, Rowspan = 3 });
                encabezado.AddCell(new PdfPCell(new Phrase("TRANSPORTES HIDRO HIDALGUENSES S.A DE C.V", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("REPORTE DE INVENTARIO FUERA DE STOCK", fontTitulo)) { Border = Rectangle.NO_BORDER });
                encabezado.AddCell(new PdfPCell(new Phrase("GENERADO EL: " + DateTime.Now.ToString("dd/MM/yyyy", culturaMX), fontNormal)) { Border = Rectangle.NO_BORDER });

                doc.Add(encabezado);
                doc.Add(new Paragraph("\n"));

                // Tabla
                PdfPTable tabla = new PdfPTable(6) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 3, 2, 1, 1, 1, 1 });

                // Encabezados
                string[] headers = { "Descripción", "Unidad de Medida", "ID", "Existencia", "Stock Mínimo", "Stock Máximo" };
                foreach (var header in headers)
                {
                    PdfPCell celdaEncabezado = new PdfPCell(new Phrase(header, fontEncabezado))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tabla.AddCell(celdaEncabezado);
                }

                // Datos
                foreach (var item in fueraDeStock)
                {
                    tabla.AddCell(new Phrase(item.Descripcion_Articulo, fontNormal));
                    tabla.AddCell(new Phrase(item.Nombre_Unidad, fontNormal));
                    tabla.AddCell(new Phrase(item.ID_Articulo?.ToString() ?? "", fontNormal));
                    tabla.AddCell(new Phrase(item.Stock_Actual?.ToString() ?? "0", fontNormal));
                    tabla.AddCell(new Phrase(item.Stock_Minimo?.ToString() ?? "0", fontNormal));
                    tabla.AddCell(new Phrase(item.Stock_Maximo?.ToString() ?? "0", fontNormal));
                }

                doc.Add(tabla);

                // Conteo al final
                doc.Add(new Paragraph("\n"));
                Paragraph total = new Paragraph($"TOTAL DE ARTÍCULOS FUERA DE STOCK: {fueraDeStock.Count}", fontEncabezado);
                total.Alignment = Element.ALIGN_RIGHT;
                doc.Add(total);

                doc.Close();

                return ms.ToArray();
            }
        }

        #endregion
    }
}