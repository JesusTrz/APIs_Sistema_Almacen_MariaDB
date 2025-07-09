using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class CuentaDto
    {
        public int ID_Cuenta { get; set; }
        public string Nombre_Cuenta { get; set; }
        public string Cuenta_Entrada { get; set; }
        public string Cuenta_Salida { get; set; }
    }

    public class DatosCuenta
    {
        public string Nombre_Cuenta { get; set; }
        public string Cuenta_Entrada { get; set; }
        public string Cuenta_Salida { get; set; }
    }
}