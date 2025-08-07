using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class SalidaDto
    {
        public Nullable<int> ID_Movimiento { get; set; }
        public Nullable<int> ID_CenCost { get; set; }
        public Nullable<int> ID_Unidad { get; set; }
        public Nullable<int> ID_Personal { get; set; }
        public string Comentarios { get; set; }
        public Nullable<int> ID_Sede { get; set; }
        public List<DetallesSalidasDto> Detalles { get; set; }
    }

    public class DetallesSalidasDto
    {
        public Nullable<int> ID_Articulo { get; set; }
        public Nullable<int> Cantidad { get; set; }
        public Nullable<decimal> Precio_Unitario { get; set; }
    }

    public class GetSalidasDto
    {
        public int ID_Salida { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<System.TimeSpan> Hora { get; set; }
        public Nullable<int> ID_Movimiento { get; set; }
        public string Nombre_Movimiento { get; set; }
        public string Descripcion_Movimiento { get; set; }
        public Nullable<int> ID_CenCost { get; set; }
        public string Nombre_CenCost { get; set; }
        public Nullable<int> ID_Unidad { get; set; }
        public string Numero_Placa { get; set; }
        public Nullable<int> ID_Personal { get; set; }
        public string Nombre { get; set; }
        public string Comentarios { get; set; }
        public Nullable<int> ID_Sede { get; set; }
        public List<GetDetalleSalidasDto> Detalles { get; set; }
    }

    public class GetDetalleSalidasDto
    {
        public Nullable<int> ID_Articulo { get; set; }
        public string Nombre_Articulo { get; set; }
        public string Nombre_Unidad { get; set; }
        public Nullable<int> Cantidad { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<decimal> Precio_Unitario { get; set; }
    }
}