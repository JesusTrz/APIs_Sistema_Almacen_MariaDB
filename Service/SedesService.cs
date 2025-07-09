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

    }
}