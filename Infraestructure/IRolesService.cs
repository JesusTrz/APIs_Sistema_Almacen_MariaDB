using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IRolesService
    {
        List<RolesDto> GetAllRoles();
        List<RolesDto> GetRolesById(int id);
    }
}
