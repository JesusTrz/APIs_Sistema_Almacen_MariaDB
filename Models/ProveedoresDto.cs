using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class ProveedoresDto
    {
        public int ID_Proveedores { get; set; }
        public string RFC { get; set; }
        public string Razon_Social { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }

    public class ProveedoresDatos
    {
        public string RFC { get; set; }
        public string Razon_Social { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }
}