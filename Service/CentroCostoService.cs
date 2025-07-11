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
    public class CentroCostoService : ICentroCostoService
    {
        private readonly string _connectionString;

        public CentroCostoService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Centros de Costo
        public List<CentroCostoDto> GetAllCenCost()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_CenCost, Nombre_CenCost, Descripcion_CenCost FROM Centro_Costo";
                return connection.Query<CentroCostoDto>(query).ToList();
            }
        }

        public List<CentroCostoDto> GetCenCostById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_CenCost, Nombre_CenCost, Descripcion_CenCost FROM Centro_Costo WHERE ID_CenCost = @ID_CenCost";
                return connection.Query<CentroCostoDto>(query, new { ID_CenCost = id}).ToList();
            }
        }
        #endregion

        #region Agregar un Nuevo Centro de Costo
        public void AgregarCentro(CenCostoDatos centro)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Centro_Costo WHERE Nombre_CenCost = @Nombre_CenCost", 
                    new { centro.Nombre_CenCost });

                if (existe > 0)
                    throw new Exception("Ya existe un centro de costo con ese nombre.");

                string query = @"INSERT INTO Centro_Costo (Nombre_CenCost, Descripcion_CenCost) 
                             VALUES (@Nombre_CenCost, @Descripcion_CenCost)";
                connection.Execute(query, new
                {
                    centro.Nombre_CenCost,
                    centro.Descripcion_CenCost
                });
            }
        }
        #endregion

        #region Editar Centro de Costo
        public void EditarCentrodeCosto(int id, CenCostoDatos centro)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var centroActual = connection.QueryFirstOrDefault<CenCostoDatos>(
                    @"SELECT ID_CenCost, Nombre_CenCost, Descripcion_CenCost 
                  FROM Centro_Costo 
                  WHERE ID_CenCost = @ID_CenCost",
                    new { ID_CenCost = id });

                if (centroActual == null)
                    throw new Exception("El Centro de Costo no existe.");

                // Usar los valores nuevos si están definidos; de lo contrario, conservar los actuales
                string nuevoNombre = string.IsNullOrWhiteSpace(centro.Nombre_CenCost) ? centroActual.Nombre_CenCost : centro.Nombre_CenCost;
                string nuevaDescripcion = string.IsNullOrWhiteSpace(centro.Descripcion_CenCost) ? centroActual.Descripcion_CenCost : centro.Descripcion_CenCost;

                string query = @"UPDATE Centro_Costo 
                             SET Nombre_CenCost = @Nombre_CenCost, 
                                 Descripcion_CenCost = @Descripcion_CenCost 
                             WHERE ID_CenCost = @ID_CenCost";

                connection.Execute(query, new
                {
                    Nombre_CenCost = nuevoNombre,
                    Descripcion_CenCost = nuevaDescripcion,
                    ID_CenCost = id
                });
            }
        }
        #endregion

        #region Eliminar Centro de Costo
        public void EliminarCentro(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Verificar si está en uso
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Salidas WHERE ID_CenCost = @ID_CenCost",
                    new { ID_CenCost = id });

                if (enUso > 0)
                    throw new Exception("No se puede eliminar el Centro de Costo porque está en uso.");

                // Verificar si existe
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Centro_Costo WHERE ID_CenCost = @ID_CenCost",
                    new { ID_CenCost = id });

                if (existe == 0)
                    throw new Exception("El Centro de Costo no existe.");

                string query = "DELETE FROM Centro_Costo WHERE ID_CenCost = @ID_CenCost";
                connection.Execute(query, new { ID_CenCost = id });
            }
        }
        #endregion
    }

}