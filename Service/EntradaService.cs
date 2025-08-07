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
    public class EntradaService : IEntradaService
    {
        private readonly string _connectionString;

        public EntradaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Registrar Entradas y Detalles
        public bool RegistrarEntradayDetalles(EntradasDto entradasdto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insertar entrada
                        string insertEntrada = @"
                    INSERT INTO Entradas (Fecha, Hora, ID_Proveedores, ID_Movimiento, Comentarios, ID_Sede)
                    VALUES (@Fecha, @Hora, @ID_Proveedores, @ID_Movimiento, @Comentarios, @ID_Sede);
                    SELECT LAST_INSERT_ID();";

                        int idEntrada = connection.ExecuteScalar<int>(insertEntrada, new
                        {
                            Fecha = DateTime.Now.Date,
                            Hora = DateTime.Now.TimeOfDay,
                            entradasdto.ID_Proveedores,
                            entradasdto.ID_Movimiento,
                            entradasdto.Comentarios,
                            entradasdto.ID_Sede
                        }, transaction);

                        foreach (var detalle in entradasdto.Detalles)
                        {
                            // Insertar detalle
                            string insertDetalle = @"
                        INSERT INTO Detalle_Entrada (ID_Entradas, ID_Articulo, Cantidad, Precio_Unitario, Total)
                        VALUES (@ID_Entradas, @ID_Articulo, @Cantidad, @Precio_Unitario, @Total);";

                            connection.Execute(insertDetalle, new
                            {
                                ID_Entradas = idEntrada,
                                detalle.ID_Articulo,
                                detalle.Cantidad,
                                detalle.Precio_Unitario,
                                Total = detalle.Cantidad * detalle.Precio_Unitario
                            }, transaction);

                            // Actualizar inventario
                            var inventario = connection.QueryFirstOrDefault<dynamic>(
                                "SELECT * FROM Inventario WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede",
                                new { detalle.ID_Articulo, entradasdto.ID_Sede }, transaction);

                            if (inventario != null)
                            {
                                decimal nuevoCostoPromedio = (Convert.ToDecimal(inventario.Costo_Promedio) + detalle.Precio_Unitario) / 2;
                                int nuevoStock = Convert.ToInt32(inventario.Stock_Actual) + detalle.Cantidad;
                                decimal nuevoSaldo = nuevoStock * nuevoCostoPromedio;

                                string updateInventario = @"
                            UPDATE Inventario SET 
                                Stock_Actual = @Stock_Actual,
                                Ultimo_Costo = @Ultimo_Costo,
                                Ultima_Compra = @Ultima_Compra,
                                Costo_Promedio = @Costo_Promedio,
                                Saldo = @Saldo
                            WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede";

                                connection.Execute(updateInventario, new
                                {
                                    Stock_Actual = nuevoStock,
                                    Ultimo_Costo = detalle.Precio_Unitario,
                                    Ultima_Compra = DateTime.Now,
                                    Costo_Promedio = nuevoCostoPromedio,
                                    Saldo = nuevoSaldo,
                                    detalle.ID_Articulo,
                                    entradasdto.ID_Sede
                                }, transaction);
                            }
                            else
                            {
                                throw new Exception($"No se encontró el inventario para el artículo con ID {detalle.ID_Articulo} en la sede {entradasdto.ID_Sede}.");
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al registrar la entrada: " + ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Obtener entradas por sede
        public List<GetEntradasDto> ObtenerEntradasPorSede(int idSede)
        {
            var sql = @"
        SELECT 
            e.ID_Entradas, e.ID_Sede, e.Fecha, e.Hora, e.ID_Movimiento, e.ID_Proveedores, e.Comentarios,
            m.Nombre_Movimiento, m.Descripcion_Movimiento,
            p.Razon_Social,
            d.ID_Articulo, d.Cantidad, d.Precio_Unitario, d.Total,
            a.Nombre_Articulo, u.Nombre_Unidad
        FROM Entradas e
            INNER JOIN Movimientos m ON e.ID_Movimiento = m.ID_Movimiento
            INNER JOIN Proveedores p ON e.ID_Proveedores = p.ID_Proveedores
            INNER JOIN Detalle_Entrada d ON e.ID_Entradas = d.ID_Entradas
            INNER JOIN Articulo a ON d.ID_Articulo = a.ID_Articulo
            INNER JOIN Unidades_Medida u ON a.ID_Medida = u.ID_Medida
            WHERE e.ID_Sede = @ID_Sede
            ORDER BY e.Fecha DESC, e.Hora DESC";

            var entradasDict = new Dictionary<int, GetEntradasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var lista = connection.Query<GetEntradasDto, GetDetallesEntradasDto, GetEntradasDto>(
                    sql,
                    (entrada, detalle) =>
                    {
                        if (!entradasDict.TryGetValue(entrada.ID_Entradas, out var entradaExistente))
                        {
                            entrada.Detalles = new List<GetDetallesEntradasDto>();
                            entradasDict.Add(entrada.ID_Entradas, entrada);
                            entradaExistente = entrada;
                        }

                        entradaExistente.Detalles.Add(detalle);
                        return entradaExistente;
                    },
                    new { ID_Sede = idSede },
                    splitOn: "ID_Articulo"
                );
            }

            return entradasDict.Values.ToList();
        }
        #endregion

        #region Eliminar Entrada
        public void EliminarEntrada(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Entradas WHERE ID_Entradas = @ID_Entradas", 
                    new { ID_Entradas = id});

                if (existe == 0)
                    throw new Exception("La Entrada no Existe!");

                string query = "DELETE FROM Entradas WHERE ID_Entradas = @ID_Entradas";
                connection.Execute(query, new { ID_Entradas = id });
            }
        }

        public void EliminarArticuloEntrada(int idEntrada, int idArticulo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM detalle_entrada 
              WHERE ID_Entradas = @ID_Entradas AND ID_Articulo = @ID_Articulo",
                    new { ID_Entradas = idEntrada, ID_Articulo = idArticulo });

                if (existe == 0)
                    throw new Exception("El artículo no fue encontrado en la entrada.");

                string query = @"DELETE FROM detalle_entrada 
                         WHERE ID_Entradas = @ID_Entradas AND ID_Articulo = @ID_Articulo";

                connection.Execute(query, new { ID_Entradas = idEntrada, ID_Articulo = idArticulo });
            }
        }

        #endregion

        #region Modificar Entradas con detalles
        public bool ActualizarEntradasyDetalles(int idEntrada, GetEntradasDto dto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var entrada = connection.QueryFirstOrDefault(
                            "SELECT * FROM Entradas WHERE ID_Entradas = @ID_Entradas", 
                            new { ID_Entradas = idEntrada }, transaction);

                        if (entrada == null) 
                            return false;

                        var updateEntradaQuery = @"
                            UPDATE Entradas SET 
                                ID_Proveedores = @ID_Proveedores,
                                ID_Movimiento = @ID_Movimiento,
                                Fecha = @Fecha,
                                Hora = @Hora,
                                Comentarios = @Comentarios,
                                ID_Sede = @ID_Sede
                            WHERE ID_Entradas = @ID_Entradas";
                        connection.Execute(updateEntradaQuery, new
                        {
                            dto.ID_Proveedores,
                            dto.ID_Movimiento,
                            dto.Fecha,
                            dto.Hora,
                            dto.Comentarios,
                            dto.ID_Sede,
                            ID_Entradas = idEntrada
                        }, transaction);

                        var detallesAntiguos = connection.Query<GetDetallesEntradasDto>(
                            "SELECT * FROM Detalle_Entrada WHERE ID_Articulo = @ID_Articulo", 
                            new { ID_Articulo  = idEntrada}, transaction).ToList();

                        foreach(var antiguo in detallesAntiguos)
                        {
                            var inventario = connection.QueryFirstOrDefault<InventarioDto>(
                                "SELECT * FROM Inventario WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede", 
                                new { ID_Articulo = antiguo.ID_Articulo, ID_Sede = dto.ID_Sede }, transaction);

                            if(inventario != null)
                            {
                                var nuevoStock = inventario.Stock_Actual - antiguo.Cantidad;
                                var nuevoSaldo = nuevoStock * (inventario.Costo_Promedio ?? 0);
                                connection.Execute(
                                    "UPDATE Inventario SET Stock_Actual = @Stock_Actual, Saldo = @Saldo WHERE ID_Inventario = @ID_Inventario", 
                                    new { Stock_Actual = nuevoStock, Saldo = nuevoSaldo, ID_Inventario = inventario.ID_Inventario }, transaction);
                            }
                        }
                        connection.Execute(
                            "DELETE FROM Detalle_Entrada WHERE ID_Entradas = @ID_Entradas", 
                            new { ID_Entradas = idEntrada }, transaction);

                        foreach(var detalle in dto.Detalles)
                        {
                            var total = detalle.Cantidad * detalle.Precio_Unitario;

                            connection.Execute(
                                @"INSERT INTO Detalle_Entrada 
                                (ID_Entradas, ID_Articulo, Cantidad, Precio_Unitario, Total)
                                VALUES (@ID_Entradas, @ID_Articulo, @Cantidad, @Precio_Unitario, @Total)", 
                                new {
                                    ID_Entradas = idEntrada,
                                    detalle.ID_Articulo,
                                    detalle.Cantidad,
                                    detalle.Precio_Unitario,
                                    Total = total
                                }, transaction);

                            var inventario = connection.QueryFirstOrDefault<InventarioDto>(
                                "SELECT * FROM Inventario WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede", 
                                new { detalle.ID_Articulo, dto.ID_Sede }, transaction);
                            if(inventario == null)
                                throw new Exception($"Inventario no encontrado para el artículo {detalle.ID_Articulo}");

                            decimal costoPromedio = (decimal)((inventario.Costo_Promedio ?? 0 + detalle.Precio_Unitario) / 2);
                            int nuevoStock = (int)(inventario.Stock_Actual + detalle.Cantidad);
                            decimal nuevoSaldo = nuevoStock * costoPromedio;

                            connection.Execute(
                                @"UPDATE Inventario SET 
                                    Stock_Actual = @Stock,
                                    Ultimo_Costo = @Ultimo_Costo,
                                    Ultima_Compra = CURDATE(),
                                    Costo_Promedio = @Costo_Promedio,
                                    Saldo = @Saldo
                                WHERE ID_Inventario = @ID_Inventario",
                                new 
                                {
                                    Stock = nuevoStock,
                                    Ultimo_Costo = detalle.Precio_Unitario,
                                    Costo_Promedio = costoPromedio,
                                    Saldo = nuevoSaldo,
                                    ID_Inventario = inventario.ID_Inventario
                                }, transaction);
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al actualizar la entrada: " + ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Ajustar Stock
        private void AjustarStock(MySqlConnection connection, MySqlTransaction transaction, int idSede, int idArticulo, int cantidadDelta)
        {
            string updateStock = @"
                UPDATE Inventario 
                SET Stock_Actual = Stock_Actual + @Cantidad,
                    Saldo = (Stock_Actual + @Cantidad) * Costo_Promedio
                WHERE ID_Sede = @ID_Sede AND ID_Articulo = @ID_Articulo";

            connection.Execute(updateStock, new
            {
                Cantidad = cantidadDelta,
                ID_Sede = idSede,
                ID_Articulo = idArticulo
            }, transaction);
        }
        #endregion


    }
}