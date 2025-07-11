using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IUnidadesService
    {
        void AgregarUnidad(UnidadesDatos unidad);
        void EditarUnidades(int id, UnidadesDatos unidad);
        void EliminarUnidad(int id);
        List<UnidadesDto> GetAllUnidades();
        List<UnidadesDto> GetUnidadesById(int id);
        List<UnidadesDto> GetUnidadesByIdSede(int idSede);
    }
}
