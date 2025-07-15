using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service
{
    public class SedesService : ISedesService
    {
        private readonly string _connectionString;

        public SedesService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Todas las Sedes
        public List<SedesDto> GetAllSedes()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {

                string query = "SELECT ID_Sede, Nombre_Sede FROM Sedes";
                return connection.Query<SedesDto>(query).ToList();
            }
        }
        #endregion

        #region Obtener SEDE por Id
        public SedesDto GetSedeById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Sede, Nombre_Sede FROM Sedes WHERE ID_Sede = @ID_Sede";
                return connection.QueryFirstOrDefault<SedesDto>(query, new { ID_Sede = id });
            }
        }
        #endregion

        #region Crear Sedes
        public void CrearSede(NombreSedeDto sede, int idUsuarioActual)
        {
            if(!UsuarioEsAdministrador(idUsuarioActual))
                throw new Exception("No tiene permisos para crear nuevas Sedes.");

            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Sedes (Nombre_Sede) VALUES (@Nombre_Sede)";
                connection.Execute(query, new { Nombre_Sede = sede.Nombre_Sede });
            }
        }
        #endregion

        /////////////////////////////////////////////

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