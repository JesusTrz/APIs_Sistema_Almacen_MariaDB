using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IProveedoresService
    {
        void AgregarProveedor(ProveedoresDatos proveedores);
        void EditarProveedor(int id, ProveedoresDatos proveedores);
        void EliminarProveedor(int id);
        List<ProveedoresDto> GetAllProveedores();
        List<ProveedoresDto> GetProveedoresById(int id);
    }
}
