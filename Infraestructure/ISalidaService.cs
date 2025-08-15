using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface ISalidaService
    {
        bool ActualizarSalidasyDetalles(int idSalida, GetSalidasDto dto);
        void EliminarArticulosSalidas(int idSalida, int idArticulo);
        void EliminarSalidas(int id);
        List<GetSalidasDto> ObtenerSalidas(int? idSalida, DateTime? fechaInicio, DateTime? fechaFin, int? idSede = null);
        List<GetSalidasDto> ObtenerSalidasFiltradas(DateTime? fechaInicio, DateTime? fechaFin, int? folioInicio, int? folioFin, int? idCentroCosto, int? idUnidad, int? idArticulo, int? idSede);
        List<GetSalidasDto> ObtenerSalidasPorArticulo(DateTime? fechaInicio, DateTime? fechaFin, int? idArticulo, int? idSede);
        List<GetSalidasDto> ObtenerSalidasPorMovimiento(DateTime? fechaInicio, DateTime? fechaFin, int? idMovimiento, int? idSede);
        List<GetSalidasDto> ObtenerSalidasporSede(int idSede);
        bool RegistrarSalidasyDetalles(SalidaDto salidaDto);
    }
}
