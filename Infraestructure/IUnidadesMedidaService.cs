using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IUnidadesMedidaService
    {
        void AgregarUnidadMedida(UnidadesMedidaNombre medida);
        void EditarUnidadMedida(int id, string nuevoNombre, string nuevaDesc);
        void EliminarUnidadMedida(int id);
        List<UnidadesMedidaDto> GetAllUMedida();
        List<UnidadesMedidaDto> GetMedidaById(int id);
    }
}
