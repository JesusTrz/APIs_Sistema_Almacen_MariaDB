using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class UnidadesDto
    {
        public int ID_Unidad { get; set; }
        public string Numero_Placa { get; set; }
        public string Descripcion_Unidad { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }

    public class UnidadesDatos
    {
        public string Numero_Placa { get; set; }
        public string Descripcion_Unidad { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }
}