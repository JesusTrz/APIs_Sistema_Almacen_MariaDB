using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class InventarioDto
    {
        public int ID_Inventario { get; set; }
        public Nullable<int> ID_Sede { get; set; }
        public Nullable<int> ID_Articulo { get; set; }
        public Nullable<int> Stock_Actual { get; set; }
        public Nullable<int> Stock_Minimo { get; set; }
        public Nullable<int> Stock_Maximo { get; set; }
        public string Ubicacion { get; set; }
        public Nullable<decimal> Costo_Promedio { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<decimal> Ultimo_Costo { get; set; }
        public Nullable<System.DateTime> Ultima_Compra { get; set; }

    }

    public class InventarioDatos
    {
        public Nullable<int> Stock_Actual { get; set; }
        public Nullable<int> Stock_Minimo { get; set; }
        public Nullable<int> Stock_Maximo { get; set; }
        public string Ubicacion { get; set; }
        public Nullable<decimal> Costo_Promedio { get; set; }
        public Nullable<decimal> Ultimo_Costo { get; set; }
        public Nullable<System.DateTime> Ultima_Compra { get; set; }

    }

    public class InventarioArticulos
    {
        public Nullable<int> ID_Articulo { get; set; }
        public string Nombre_Articulo { get; set; }
        public string Descripcion_Articulo { get; set; }
        public Nullable<int> ID_Medida { get; set; }
        public string Nombre_Unidad { get; set; }
        public string Numero_Parte { get; set; }

        public string Ubicacion { get; set; }
        public Nullable<int> Stock_Actual { get; set; }
        public Nullable<int> Stock_Minimo { get; set; }
        public Nullable<int> Stock_Maximo { get; set; }
        public Nullable<decimal> Costo_Promedio { get; set; }
        public Nullable<decimal> Saldo { get; set; }
        public Nullable<decimal> Ultimo_Costo { get; set; }
        public Nullable<System.DateTime> Ultima_Compra { get; set; }

        public Nullable<int> ID_Linea { get; set; }
        public string Nombre_Linea { get; set; }

    }

    public class AgregarArticuloaInventario
    {
        public Nullable<int> ID_Sede { get; set; }
        public Nullable<int> ID_Articulo { get; set; }


    }

    public class StockEntrada
    {
        public int ID_Sede { get; set; }
        public int ID_Articulo { get; set; }
        public int Cantidad { get; set; }
    }



    public class InventarioFiltro
    {
        public int ID_Sede { get; set; }
        public List<string> Campos { get; set; }
    }
}