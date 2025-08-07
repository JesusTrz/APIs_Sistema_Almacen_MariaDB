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
    public class RolesService : IRolesService
    {
        private readonly string _connectionString;

        public RolesService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        public List<RolesDto> GetAllRoles()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Roles, Nombre_Rol FROM Roles";
                return connection.Query<RolesDto>(query).ToList();
            }
        }

        public List<RolesDto> GetRolesById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Roles, Nombre_Rol FROM Roles WHERE ID_Roles = @ID_Roles";
                return connection.Query<RolesDto>(query, new { ID_Roles = id }).ToList();
            }
        }
    }
}