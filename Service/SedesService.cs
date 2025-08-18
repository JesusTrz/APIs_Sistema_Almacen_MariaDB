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
        public void CrearSede(NombreSedeDto sede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Sedes (Nombre_Sede) VALUES (@Nombre_Sede)";
                connection.Execute(query, new { Nombre_Sede = sede.Nombre_Sede });
            }
        }
        #endregion

        #region Actualizar Sede
        public void ActualizarSede(int idSede, string nuevoNombre)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "UPDATE Sedes SET Nombre_Sede = @Nombre_Sede WHERE ID_Sede = @ID_Sede";
                int filasAfectadas = connection.Execute(query, new { Nombre_Sede = nuevoNombre, ID_Sede = idSede });

                if (filasAfectadas == 0)
                    throw new Exception("No se encontró la sede especificada.");
            }
        }
        #endregion

        #region Eliminar Sede y todo lo relacionado
        public void EliminarSede(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Eliminar datos relacionados
                        connection.Execute("DELETE FROM DetallesEntradas WHERE ID_Entradas IN (SELECT ID_Entradas FROM Entradas WHERE ID_Sede = @ID_Sede)", new { ID_Sede = idSede }, transaction);
                        connection.Execute("DELETE FROM Entradas WHERE ID_Sede = @ID_Sede", new { ID_Sede = idSede }, transaction);

                        connection.Execute("DELETE FROM DetallesSalidas WHERE ID_Salidas IN (SELECT ID_Salidas FROM Salidas WHERE ID_Sede = @ID_Sede)", new { ID_Sede = idSede }, transaction);
                        connection.Execute("DELETE FROM Salidas WHERE ID_Sede = @ID_Sede", new { ID_Sede = idSede }, transaction);

                        connection.Execute("DELETE FROM Inventario WHERE ID_Sede = @ID_Sede", new { ID_Sede = idSede }, transaction);

                        connection.Execute("DELETE FROM Usuarios WHERE ID_Sede = @ID_Sede", new { ID_Sede = idSede }, transaction);

                        // Finalmente eliminar la sede
                        connection.Execute("DELETE FROM Sedes WHERE ID_Sede = @ID_Sede", new { ID_Sede = idSede }, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw new Exception("Ocurrió un error al eliminar la sede y sus datos relacionados.");
                    }
                }
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