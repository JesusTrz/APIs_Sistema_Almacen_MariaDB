using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class LineaDto
    {
        public int ID_Linea { get; set; }
        public string Nombre_Linea { get; set; }
        public string Descripcion_Linea { get; set; }
        public Nullable<int> ID_Cuenta { get; set; }
        public string Nombre_Cuenta { get; set; }
    }

    public class LineaDatos
    {
        public string Nombre_Linea { get; set; }
        public string Descripcion_Linea { get; set; }
        public Nullable<int> ID_Cuenta { get; set; }
    }
}