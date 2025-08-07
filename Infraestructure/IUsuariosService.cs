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
        void CambiarNombreUsuario(int id, UsuariosDto usuarios);
        void CrearUsuarios(UsuariosDatos users, int idUsuarioActual);
        void EliminarUsuario(int id);
        List<UsuariosDto> GetAllUsuarios();
        UsuariosDto GetUsersById(int id);
        List<UsuariosDto> GetUsuarioBySedeId(int idSede);
        UsuariosDto LoginUsuarios(string nombreUsuario, string contrasenia);
    }
}
