using Dapper;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service
{
    public class UsuariosService : IUsuariosService
    {
        private readonly string _connectionString;

        public UsuariosService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Usuarios
        public List<UsuariosDto> GetAllUsuarios()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Usuario, Nombre_Usuario, Contrasenia, ID_Roles, ID_Sede FROM Usuarios";
                return connection.Query<UsuariosDto>(query).ToList();
            }
        }
        #endregion

        #region Obtener Usuarios por Sede
        public List<UsuariosDto> GetUsuarioBySedeId(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_Usuario, Nombre_Usuario, Contrasenia, ID_Roles, ID_Sede 
                         FROM Usuarios 
                         WHERE ID_Sede = @ID_Sede";
                return connection.Query<UsuariosDto>(query, new { ID_Sede = idSede }).ToList();
            }
        }
        #endregion

        #region Crear Usuarios
        public void CrearUsuarios(UsuariosDatos users, int idUsuarioActual)
        {
            if(!UsuarioEsAdministrador(idUsuarioActual))
                throw new Exception("No tiene permisos para crear usuarios");

            if (!ContraseniaValida(users.Contrasenia))
                throw new Exception("La contraseña no cumple con los requisitos mínimos.");

            if(!RolExiste(users.ID_Roles))
                 throw new Exception("El Rol Seleccionado no Existe.");

            if(!SedeExiste(users.ID_Sede))
                throw new Exception("La Sede Seleccionada no Existe.");

            if(UsuarioExiste(users.Nombre_Usuario))
                throw new Exception("El Nombre de Usuario ya Existe!");

            string passwordHasheada = HashPassword(users.Contrasenia);

            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Usuarios (Nombre_Usuario, Contrasenia, ID_Roles, ID_Sede) VALUES (@Nombre_Usuario, @Contrasenia, @ID_Roles, @ID_Sede)";
                connection.Execute(query, new 
                { 
                    Nombre_Usuario = users.Nombre_Usuario, 
                    Contrasenia = passwordHasheada,
                    ID_Roles = users.ID_Roles,
                    ID_Sede = users.ID_Sede
                });
            }
        }
        #endregion

        #region Cambiar contraseña
        public void CambiarContrasenia(int idUsuario, string ViejaContrasenia, string NuevaContrasenia)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string queryGet = "SELECT Contrasenia FROM Usuarios WHERE ID_Usuario = @ID_Usuario";
                string contraseniaAlmacenada = connection.QueryFirstOrDefault<string>(queryGet, new { ID_Usuario = idUsuario });

                if (string.IsNullOrEmpty(contraseniaAlmacenada))
                    throw new Exception("Usuario no encontrado.");

                string hashActualIngresado = HashPassword(ViejaContrasenia);
                if (!string.Equals(contraseniaAlmacenada, hashActualIngresado, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("La contraseña actual es incorrecta.");

                if (!ContraseniaValida(NuevaContrasenia))
                    throw new Exception("La nueva contraseña no cumple con los requisitos de seguridad.");

                string hashNueva = HashPassword(NuevaContrasenia);

                string queryUpdate = "UPDATE Usuarios SET Contrasenia = @Contrasenia WHERE ID_Usuario = @ID_Usuario";
                connection.Execute(queryUpdate, new { Contrasenia = hashNueva, ID_Usuario = idUsuario });
            }
        }
        #endregion

        #region Login
        public UsuariosDto LoginUsuarios(string nombreUsuario, string contrasenia)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                u.ID_Usuario, 
                u.Nombre_Usuario, 
                u.Contrasenia, 
                u.ID_Roles, 
                u.ID_Sede,
                r.Nombre_Rol AS NombreRol
            FROM Usuarios u
            JOIN Roles r ON u.ID_Roles = r.ID_Roles
            WHERE u.Nombre_Usuario = @Nombre_Usuario";

                var usuario = connection.QueryFirstOrDefault<UsuariosDto>(query, new
                {
                    Nombre_Usuario = nombreUsuario
                });

                if (usuario == null)
                    throw new Exception("Usuario no encontrado.");

                string hashIngresado = HashPassword(contrasenia);

                if (!usuario.Contrasenia.Equals(hashIngresado, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Contraseña incorrecta.");

                usuario.Contrasenia = null; // nunca devolver la contraseña

                return usuario;
            }
        }

        #endregion

        #region Metodo para Encriptar Contraseña y Validar Contraseña
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool ContraseniaValida(string password)
        {
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            return regex.IsMatch(password);
        }
        #endregion

        #region Verificar si la Sede, Rol y Usuarios Existe
        private bool SedeExiste(int? idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM Sedes WHERE ID_Sede = @Id";
                return connection.ExecuteScalar<int>(query, new { Id = idSede }) > 0;
            }
        }

        private bool RolExiste(int? idRol)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM Roles WHERE ID_Roles = @Id";
                return connection.ExecuteScalar<int>(query, new { Id = idRol }) > 0;
            }
        }

        private bool UsuarioExiste(string nombreUsuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM Usuarios WHERE Nombre_Usuario = @Nombre_Usuario";
                return connection.ExecuteScalar<int>(query, new { Nombre_Usuario = nombreUsuario }) > 0;
            }
        }

        #endregion

        /////////////////////////////////////////////////////

        #region Validar si usuario es Administrador
        private bool UsuarioEsAdministrador(int idUsuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"SELECT r.Nombre_Rol
                               FROM Usuarios u
                               JOIN Roles r ON u.ID_Roles = r.ID_Roles
                               WHERE u.ID_Usuario = @ID_Usuario";

                var rol = connection.QueryFirstOrDefault<string>(sql, new { ID_Usuario = idUsuario });
                return rol == "Administrador";
            }
        }
        #endregion
    }
}