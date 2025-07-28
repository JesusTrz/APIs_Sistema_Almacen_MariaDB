using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IUsuariosService
    {
        void CambiarContrasenia(int idUsuario, string ViejaContrasenia, string NuevaContrasenia);
        void CrearUsuarios(UsuariosDatos users, int idUsuarioActual);
        List<UsuariosDto> GetAllUsuarios();
        List<UsuariosDto> GetUsuarioBySedeId(int idSede);
        UsuariosDto LoginUsuarios(string nombreUsuario, string contrasenia);
    }
}
