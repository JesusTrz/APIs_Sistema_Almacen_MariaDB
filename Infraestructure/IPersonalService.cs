using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IPersonalService
    {
        void AgregarPersona(PersonalDatos persona);
        void EditarPersona(int id, PersonalDatos persona);
        void EliminarPersonal(int id);
        bool EliminarTodoElPersonal(int idSede);
        List<PersonalDto> GetAllPersonal();
        List<PersonalDto> GetPersonalById(int id);
        List<PersonalDto> GetUsuariosByIdSede(int idSede);
    }
}
