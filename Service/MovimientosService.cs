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
    public class MovimientosService : IMovimientosService
    {
        private readonly string _connectionString;
        public MovimientosService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Movimientos
        public List<MovimientosDto> GetAllMovimientos()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Movimiento, Nombre_Movimiento, Descripcion_Movimiento, Tipo FROM Movimientos";
                return connection.Query<MovimientosDto>(query).ToList();
            }
        }

        public List<MovimientosDto> GetMovimientosById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Movimiento, Nombre_Movimiento, Descripcion_Movimiento, Tipo FROM Movimientos WHERE ID_Movimiento = @ID_Movimiento";
                return connection.Query<MovimientosDto>(query, new { ID_Movimiento = id }).ToList();
            }
        }

        public List<MovimientosDto> GetMovimientosEntrada()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Movimiento, Nombre_Movimiento, Descripcion_Movimiento, Tipo FROM movimientos WHERE Tipo = 'Entrada'";
                return connection.Query<MovimientosDto>(query).ToList();
            }
        }

        public List<MovimientosDto> GetMovimientosSalida()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Movimiento, Nombre_Movimiento, Descripcion_Movimiento, Tipo FROM movimientos WHERE Tipo = 'Salida'";
                return connection.Query<MovimientosDto>(query).ToList();
            }
        }
        #endregion

        #region Agregar Movimientos
        public void AgregarNuevoMovimiento(MovimientosDatos movimiento)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Validación estricta
                if (movimiento.Tipo != "Entrada" && movimiento.Tipo != "Salida")
                {
                    throw new Exception("El tipo de movimiento debe ser exactamente 'Entrada' o 'Salida'.");
                }

                string query = @"INSERT INTO Movimientos 
                         (Nombre_Movimiento, Descripcion_Movimiento, Tipo)
                         VALUES (@Nombre_Movimiento, @Descripcion_Movimiento, @Tipo)";

                connection.Execute(query, new
                {
                    Nombre_Movimiento = movimiento.Nombre_Movimiento,
                    Descripcion_Movimiento = movimiento.Descripcion_Movimiento,
                    Tipo = movimiento.Tipo
                });
            }
        }

        #endregion

        #region Editar Movimiento
        public void EditarMovimiento(int id, MovimientosDatos movimiento)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Normalizar tipo de movimiento (si no es null o vacío)
                if (!string.IsNullOrWhiteSpace(movimiento.Tipo))
                {
                    var tipoNormalizado = char.ToUpper(movimiento.Tipo[0]) + movimiento.Tipo.Substring(1).ToLower();

                    if (tipoNormalizado != "Entrada" && tipoNormalizado != "Salida")
                        throw new Exception("El tipo de movimiento debe ser exactamente 'Entrada' o 'Salida'.");

                    movimiento.Tipo = tipoNormalizado;
                }

                // Verificar que el movimiento exista
                var movimientoActual = connection.QueryFirstOrDefault<MovimientosDatos>(
                    "SELECT ID_Movimiento, Nombre_Movimiento, Descripcion_Movimiento, Tipo FROM Movimientos WHERE ID_Movimiento = @ID",
                    new { ID = id });

                if (movimientoActual == null)
                    throw new Exception("El movimiento no existe.");

                // Asignar nuevos valores si se proporcionan
                var nuevoNombre = string.IsNullOrWhiteSpace(movimiento.Nombre_Movimiento)
                    ? movimientoActual.Nombre_Movimiento
                    : movimiento.Nombre_Movimiento;

                var nuevaDesc = string.IsNullOrWhiteSpace(movimiento.Descripcion_Movimiento)
                    ? movimientoActual.Descripcion_Movimiento
                    : movimiento.Descripcion_Movimiento;

                var nuevoTipo = string.IsNullOrWhiteSpace(movimiento.Tipo)
                    ? movimientoActual.Tipo
                    : movimiento.Tipo;

                // Actualizar
                string query = @"UPDATE Movimientos
                         SET Nombre_Movimiento = @Nombre_Movimiento,
                             Descripcion_Movimiento = @Descripcion_Movimiento,
                             Tipo = @Tipo
                         WHERE ID_Movimiento = @ID_Movimiento";

                connection.Execute(query, new
                {
                    Nombre_Movimiento = nuevoNombre,
                    Descripcion_Movimiento = nuevaDesc,
                    Tipo = nuevoTipo,
                    ID_Movimiento = id
                });
            }
        }
        #endregion

        #region Eliminar Movimiento
        public void EliminarMovimiento(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Verificar si está en uso en Entradas
                var enUsoEnEntradas = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Entradas WHERE ID_Movimiento = @ID_Movimiento",
                    new { ID_Movimiento = id });

                // Verificar si está en uso en Salidas
                var enUsoEnSalidas = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Salidas WHERE ID_Movimiento = @ID_Movimiento",
                    new { ID_Movimiento = id });

                if (enUsoEnEntradas > 0 || enUsoEnSalidas > 0)
                    throw new Exception("No se puede eliminar el movimiento porque está en uso en Entradas o Salidas.");

                // Verificar que exista
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Movimientos WHERE ID_Movimiento = @ID_Movimiento",
                    new { ID_Movimiento = id });

                if (existe == 0)
                    throw new Exception("El movimiento no existe.");

                // Eliminar
                string query = "DELETE FROM Movimientos WHERE ID_Movimiento = @ID_Movimiento";
                connection.Execute(query, new { ID_Movimiento = id });
            }
        }
        #endregion




    }
}