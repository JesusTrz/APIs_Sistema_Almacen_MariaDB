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
    public class CuentaService : ICuentaService
    {
        private readonly string _connectionString;

        public CuentaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Servicio
        public List<CuentaDto> GetAllCuentas()
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Cuenta, Nombre_Cuenta, Cuenta_Entrada, Cuenta_Salida FROM Cuenta";
                return connection.Query<CuentaDto>(query).ToList();
            }
        }

        public List<CuentaDto> GetCuentaById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Cuenta, Nombre_Cuenta, Cuenta_Entrada, Cuenta_Salida FROM Cuenta WHERE ID_Cuenta = @ID_Cuenta";
                return connection.Query<CuentaDto>(query, new { ID_Cuenta = id }).ToList();
            }
        }
        #endregion

        #region Agregar nueva Cuenta
        public void AgregarCuenta(DatosCuenta cuenta)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Cuenta (Nombre_Cuenta, Cuenta_Entrada, Cuenta_Salida) VALUES (@Nombre_Cuenta, @Cuenta_Entrada, @Cuenta_Salida)";
                connection.Execute(query, new 
                {
                    Nombre_Cuenta = cuenta.Nombre_Cuenta,
                    Cuenta_Entrada = cuenta.Cuenta_Entrada,
                    Cuenta_Salida= cuenta.Cuenta_Salida
                });
            }
        }
        #endregion

        #region Editar Cuenta
        public void EditarCuenta(int id, DatosCuenta cuenta)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var cuentaActual = connection.QueryFirstOrDefault<CuentaDto>(
                    "SELECT ID_Cuenta, Nombre_Cuenta, Cuenta_Entrada, Cuenta_Salida FROM Cuenta WHERE ID_Cuenta = @ID",
                    new { ID = id });

                if (cuentaActual == null)
                    throw new Exception("La cuenta no existe.");

                // Si el campo nuevo está nulo o vacío, se conserva el valor anterior
                var nombreCuenta = string.IsNullOrWhiteSpace(cuenta.Nombre_Cuenta) ? cuentaActual.Nombre_Cuenta : cuenta.Nombre_Cuenta;
                var cuentaEntrada = string.IsNullOrWhiteSpace(cuenta.Cuenta_Entrada) ? cuentaActual.Cuenta_Entrada : cuenta.Cuenta_Entrada;
                var cuentaSalida = string.IsNullOrWhiteSpace(cuenta.Cuenta_Salida) ? cuentaActual.Cuenta_Salida : cuenta.Cuenta_Salida;

                var duplicado = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Cuenta WHERE Nombre_Cuenta = @Nombre AND ID_Cuenta != @ID",
                    new { Nombre = nombreCuenta, ID = id });

                if (duplicado > 0)
                    throw new Exception("Ya existe otra cuenta con ese nombre.");

                // Ejecutar actualización
                string query = @"
                    UPDATE Cuenta 
                    SET Nombre_Cuenta = @Nombre_Cuenta, 
                        Cuenta_Entrada = @Cuenta_Entrada, 
                        Cuenta_Salida = @Cuenta_Salida
                    WHERE ID_Cuenta = @ID_Cuenta";

                connection.Execute(query, new
                {
                    ID_Cuenta = id,
                    Nombre_Cuenta = nombreCuenta,
                    Cuenta_Entrada = cuentaEntrada,
                    Cuenta_Salida = cuentaSalida
                });
            }
        }
        #endregion

        #region Eliminar Cuenta
        public void EliminarCuenta(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Linea WHERE ID_Cuenta = @ID_Cuenta",
                    new { ID_Cuenta = id });

                if (enUso > 0)
                    throw new Exception("No se puede eliminar la cuenta porque está en uso.");

                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Cuenta WHERE ID_Cuenta = @ID_Cuenta",
                    new { ID_Cuenta = id });

                    if (existe == 0)
                        throw new Exception("La cuenta no existe.");

                // Eliminar
                string query = "DELETE FROM Cuenta WHERE ID_Cuenta = @ID_Cuenta";
                connection.Execute(query, new { ID_Cuenta = id });
            }
        }
        #endregion
    }
}