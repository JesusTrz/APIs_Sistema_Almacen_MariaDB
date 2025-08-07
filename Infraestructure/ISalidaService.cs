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
        List<GetSalidasDto> ObtenerSalidasporSede(int idSede);
        bool RegistrarSalidasyDetalles(SalidaDto salidaDto);
    }
}
