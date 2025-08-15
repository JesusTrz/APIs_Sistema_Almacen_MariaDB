using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service
{
    public class Base_de_Datos_Service : IBase_de_Datos_Service
    {
        private readonly string _connectionString;

        public Base_de_Datos_Service()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Resetar Base de Datos
        public void VaciarBaseDeDatos(int idRol)
        {
            if (idRol != 1)
            {
                throw new UnauthorizedAccessException("No tienes permisos para ejecutar esta acción.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Orden: primero las tablas dependientes (detalles), luego las principales
                        connection.Execute("DELETE FROM Cuenta", transaction: transaction);
                        connection.Execute("DELETE FROM Linea", transaction: transaction);
                        connection.Execute("DELETE FROM Unidades_Medida", transaction: transaction);
                        connection.Execute("DELETE FROM Articulo", transaction: transaction);
                        connection.Execute("DELETE FROM Inventario", transaction: transaction);
                        connection.Execute("DELETE FROM Movimientos", transaction: transaction);
                        connection.Execute("DELETE FROM Proveedores", transaction: transaction);
                        connection.Execute("DELETE FROM Entradas", transaction: transaction);
                        connection.Execute("DELETE FROM Detalle_Entrada", transaction: transaction);
                        connection.Execute("DELETE FROM Centro_Costo", transaction: transaction);
                        connection.Execute("DELETE FROM Unidades", transaction: transaction);
                        connection.Execute("DELETE FROM Personal", transaction: transaction);
                        connection.Execute("DELETE FROM Salidas", transaction: transaction);
                        connection.Execute("DELETE FROM Detalle_Salida", transaction: transaction);
                        // ... Agrega aquí el resto de tablas de tu sistema

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al vaciar base de datos: " + ex.Message);
                    }
                }
            }
        }


    }

    #endregion
}
