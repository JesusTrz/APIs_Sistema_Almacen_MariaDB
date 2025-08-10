using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Models
{
    public class KardexDto
    {
        public int IdMovimiento { get; set; }
        public string TipoMovimiento { get; set; } // "Entrada" o "Salida"
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string OrigenDestino { get; set; } // Proveedor para entradas, Destino para salidas
        public int Cantidad { get; set; } // Positivo para entradas, negativo para salidas
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string TipoDocumento { get; set; } // Tipo de movimiento
        public string Comentarios { get; set; }
        public int SaldoCantidad { get; set; } // Saldo acumulado en cantidad
        public decimal SaldoValor { get; set; } // Saldo acumulado en valor
        public string NombreArticulo { get; set; }
        public string UnidadMedida { get; set; }
    }
}