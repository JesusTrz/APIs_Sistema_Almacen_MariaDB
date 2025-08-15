using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IBase_de_Datos_Service
    {
        void VaciarBaseDeDatos(int idRol);
    }
}
