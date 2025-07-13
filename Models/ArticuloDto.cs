using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class ArticuloDto
    {
        public int ID_Articulo { get; set; }
        public string Nombre_Articulo { get; set; }
        public string Descripcion_Articulo { get; set; }
        public string Numero_Parte { get; set; }
        public Nullable<int> ID_Linea { get; set; }
        public string Nombre_Linea { get; set; }
        public Nullable<int> ID_Medida { get; set; }
        public string Nombre_Unidad { get; set; }
    }

    public class ArticulosDatos
    {
        public string Nombre_Articulo { get; set; }
        public string Descripcion_Articulo { get; set; }
        public string Numero_Parte { get; set; }
        public Nullable<int> ID_Linea { get; set; }
        public Nullable<int> ID_Medida { get; set; }
    }

    public class ArticulosModel
    {
        public int ID_Articulo { get; set; }
        public string Nombre_Articulo { get; set; }
        public string Descripcion_Articulo { get; set; }
        public string Numero_Parte { get; set; }
        public Nullable<int> ID_Linea { get; set; }
        public Nullable<int> ID_Medida { get; set; }
    }
}