using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class UnidadesMedidaDto
    {
        public int ID_Medida { get; set; }
        public string Nombre_Unidad { get; set; }
        public string Descripcion_Unidad { get; set;}
    }

    public class UnidadesMedidaNombre
    {
        public string Nombre_Unidad { get; set; }
        public string Descripcion_Unidad { get; set; }
    }
}