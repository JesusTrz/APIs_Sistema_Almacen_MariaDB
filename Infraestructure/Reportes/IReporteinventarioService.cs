using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure.Reportes
{
    public interface IReporteinventarioService
    {
        byte[] GenerarReporteArticulosFueraDeStock(List<InventarioArticulos> inventario, bool menoresMinimo, bool mayoresMaximo);
        byte[] GenerarReporteInventario(List<InventarioArticulos> inventario, int? idArticulo = null, bool agruparPorUbicacion = false, bool agruparPorLinea = false, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    }
}
