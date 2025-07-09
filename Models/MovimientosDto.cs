using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class MovimientosDto
    {
        public int ID_Movimiento { get; set; }
        public string Nombre_Movimiento { get; set; }
        public string Descripcion_Movimiento { get; set; }
        [Required]
        [RegularExpression("^(Entrada|Salida)$", ErrorMessage = "El tipo debe ser 'Entrada' o 'Salida'.")]
        public string Tipo { get; set; }
    }

    public class MovimientosDatos
    {
        public string Nombre_Movimiento { get; set; }
        public string Descripcion_Movimiento { get; set; }
        [Required]
        [RegularExpression("^(Entrada|Salida)$", ErrorMessage = "El tipo debe ser 'Entrada' o 'Salida'.")]
        public string Tipo { get; set; }
    }
}