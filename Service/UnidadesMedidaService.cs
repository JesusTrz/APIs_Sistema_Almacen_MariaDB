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
    public class UnidadesMedidaService : IUnidadesMedidaService
    {
        private readonly string _connectionString;

        public UnidadesMedidaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Unidades de Medida
        public List<UnidadesMedidaDto> GetAllUMedida()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Medida, Nombre_Unidad FROM Unidades_Medida";
                return connection.Query<UnidadesMedidaDto>(query).ToList();
            }
        }

        public List<UnidadesMedidaDto> GetMedidaById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_Medida, Nombre_Unidad FROM Unidades_Medida WHERE ID_Medida = @ID_Medida";
                return connection.Query<UnidadesMedidaDto>(query, new { ID_Medida = id }).ToList();
            }
        }
        #endregion
    }
}