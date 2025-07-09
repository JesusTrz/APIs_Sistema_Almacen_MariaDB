using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class CentroCostoDto
    {
        public int ID_CenCost { get; set; }
        public string Nombre_CenCost { get; set; }
        public string Descripcion_CenCost { get; set; }
    }

    public class CenCostoDatos
    {
        public string Nombre_CenCost { get; set; }
        public string Descripcion_CenCost { get; set; }
    }
}