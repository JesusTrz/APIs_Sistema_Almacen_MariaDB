using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class PersonalDto
    {
        public int ID_Personal { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }

    public class PersonalDatos
    {
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public Nullable<int> ID_Sede { get; set; }
    }
}