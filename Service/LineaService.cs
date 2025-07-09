using Dapper;
using Microsoft.Ajax.Utilities;
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
    public class LineaService : ILineaService
    {
        private readonly string _connectionString;

        public LineaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Lineas
        public List<LineaDto> GetAllLineas()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        l.ID_Linea,
                        l.Nombre_Linea, 
                        l.Descripcion_Linea, 
                        l.ID_Cuenta,
                        c.Nombre_Cuenta
                    FROM Linea l
                    INNER JOIN Cuenta c ON l.ID_Cuenta = c.ID_Cuenta";
                return connection.Query<LineaDto>(query).ToList();
            }
        }

        public List<LineaDto> GetLineasById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        l.ID_Linea,
                        l.Nombre_Linea, 
                        l.Descripcion_Linea, 
                        l.ID_Cuenta,
                        c.Nombre_Cuenta
                    FROM Linea l
                    INNER JOIN Cuenta c ON l.ID_Cuenta = c.ID_Cuenta
                    WHERE l.ID_Linea = @ID_Linea ";
                return connection.Query<LineaDto>(query, new { ID_Linea = id }).ToList();
            }
        }
        #endregion

        #region Agregar nueva Linea
        public void AgregarLinea(LineaDatos linea)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Linea (Nombre_Linea, Descripcion_Linea, ID_Cuenta) VALUES (@Nombre_Linea, @Descripcion_Linea, @ID_Cuenta)";
                connection.Execute(query, new
                {
                    Nombre_Linea = linea.Nombre_Linea,
                    Descripcion_Linea = linea.Descripcion_Linea,
                    ID_Cuenta = linea.ID_Cuenta
                });
            }
        }
        #endregion

        #region Editar Linea
        public void EditarLinea(int id, LineaDatos linea)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var lineaActual = connection.QueryFirstOrDefault<LineaDatos>(
                    "SELECT ID_Linea, Nombre_Linea, Descripcion_Linea, ID_Cuenta FROM Linea WHERE ID_Linea = @ID",
                    new { ID = id });

                if(lineaActual == null)
                    throw new Exception("La linea no existe.");

                string nombreLinea = !string.IsNullOrEmpty(linea.Nombre_Linea) ? linea.Nombre_Linea : lineaActual.Nombre_Linea;
                string descripcion = !string.IsNullOrEmpty(linea.Descripcion_Linea) ? linea.Descripcion_Linea : lineaActual.Descripcion_Linea;
                int idCuenta = (int)(linea.ID_Cuenta != 0 ? linea.ID_Cuenta : lineaActual.ID_Cuenta);

                string updateQuery = @"UPDATE Linea 
                               SET Nombre_Linea = @Nombre_Linea,
                                   Descripcion_Linea = @Descripcion_Linea,
                                   ID_Cuenta = @ID_Cuenta
                               WHERE ID_Linea = @ID_Linea";

                connection.Execute(updateQuery, new
                {
                    Nombre_Linea = nombreLinea,
                    Descripcion_Linea = descripcion,
                    ID_Cuenta = idCuenta,
                    ID_Linea = id
                });
            }
        }
        #endregion
    }
}