using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface ILineaService
    {
        void AgregarLinea(LineaDatos linea);
        void EditarLinea(int id, LineaDatos linea);
        void EliminarCuenta(int id);
        List<LineaDto> GetAllLineas();
        List<LineaDto> GetLineasById(int id);
    }
}
