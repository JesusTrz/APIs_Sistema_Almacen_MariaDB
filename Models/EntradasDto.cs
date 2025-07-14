using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class EntradasDto
    {
        public Nullable<int> ID_Movimiento { get; set; }
        public Nullable<int> ID_Proveedores { get; set; }
        public Nullable<int> ID_Sede { get; set; }
        public string Comentarios { get; set; }
        public List<DestallesEntradasDto> Detalles { get; set; }
    }

    public class DestallesEntradasDto
    {
        public Nullable<int> ID_Articulo { get; set; }
        public Nullable<int> Cantidad { get; set; }
        public Nullable<decimal> Precio_Unitario { get; set; }
    }

    public class GetEntradasDto
    {
        public int ID_Entradas { get; set; }
        public DateTime? Fecha { get; set; }
        public TimeSpan? Hora { get; set; }
        public string Comentarios { get; set; }
        public int? ID_Proveedores { get; set; }
        public string Razon_Social { get; set; }
        public int? ID_Movimiento { get; set; }
        public string Nombre_Movimiento { get; set; }
        public string Descripcion_Movimiento { get; set; }
        public int? ID_Sede { get; set; }
        public List<GetDetallesEntradasDto> Detalles { get; set; }
    }

    public class GetDetallesEntradasDto
    {
        public int? ID_Articulo { get; set; }
        public string Nombre_Articulo { get; set; }
        public string Nombre_Unidad { get; set; }
        public int? Cantidad { get; set; }
        public decimal? Precio_Unitario { get; set; }
        public decimal? Total { get; set; }
    }

    

}