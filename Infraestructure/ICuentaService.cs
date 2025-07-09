using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface ICuentaService
    {
        void AgregarCuenta(DatosCuenta cuenta);
        void EditarCuenta(int id, DatosCuenta cuenta);
        void EliminarCuenta(int id);
        List<CuentaDto> GetAllCuentas();
        List<CuentaDto> GetCuentaById(int id);
    }
}
