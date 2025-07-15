using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface ISedesService
    {
        void CrearSede(NombreSedeDto sede, int idUsuarioActual);
        List<SedesDto> GetAllSedes();
        SedesDto GetSedeById(int id);
    }
}
