using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IEntradaService
    {
        bool ActualizarEntradasyDetalles(int idEntrada, GetEntradasDto dto);
        void EliminarEntrada(int id);
        void EliminarArticuloEntrada(int idEntrada, int idArticulo);
        List<GetEntradasDto> ObtenerEntradasPorSede(int idSede);
        bool RegistrarEntradayDetalles(EntradasDto entradasdto);
        GetEntradasDto ObtenerEntradaPorId(int idEntrada, int idSede);
        List<GetEntradasDto> ObtenerEntradasFiltradas(int? idSede = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        List<GetEntradasDto> ObtenerEntradasPorProveedor(int idProveedor, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? idSede = null);
        List<GetEntradasDto> ObtenerEntradasPorArticulo(int? idArticulo, DateTime? fechaInicio, DateTime? fechaFin, int? idSede = null);
        List<GetEntradasDto> ObtenerEntradasPorMovimiento(int? idMovimiento, DateTime? fechaInicio, DateTime? fechaFin, int? idSede = null);
        List<GetEntradasDto> ObtenerEntradasFiltradas(DateTime? fechaInicio, DateTime? fechaFin, int? folioInicio, int? folioFin, int? idProveedor, int? idArticulo, int? idSede);
        List<GetEntradasDto> ObtenerEntradasPorProveedorYArticulo(int? idSede, int? idProveedor, DateTime? fechaInicio, DateTime? fechaFin, int? folioInicio, int? folioFin);
    }
}
