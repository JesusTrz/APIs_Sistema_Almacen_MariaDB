using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IMovimientosService
    {
        void AgregarNuevoMovimiento(MovimientosDatos movimiento);
        void EditarMovimiento(int id, MovimientosDatos movimiento);
        void EliminarMovimiento(int id);
        List<MovimientosDto> GetAllMovimientos();
        List<MovimientosDto> GetMovimientosById(int id);
    }
}
