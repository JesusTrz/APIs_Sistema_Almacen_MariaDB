using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure.Reportes
{
    public interface IReporteSalidaService
    {
        #region REPORTE DE SALIDAS POR ID Y FECHA 
        byte[] GenerarReporteSalidas(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        #endregion
        #region REPORTE DE SALIDAS POR RANGO DE FECHAS, RNAGO DE FOLIOS, IDCENTROCOSTO,IDARTICULO,IDUNIDAD
        byte[] GenerarReporteSalidasFiltrado(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        byte[] GenerarReporteSalidasPorArticulo(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        byte[] GenerarReporteSalidasPorMovimiento(List<GetSalidasDto> salidas, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        #endregion
    }
}
