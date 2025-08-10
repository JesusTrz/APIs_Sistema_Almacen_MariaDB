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
    public class KardexService : IKardexService
    {
        private readonly string _connectionString;

        public KardexService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        public List<KardexDto> ObtenerKardexPorArticulo(int idSede, int idArticulo)
        {
            var kardex = new List<KardexDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                // 1. Obtener información básica del artículo
                var articuloInfo = connection.QueryFirstOrDefault<dynamic>(
                    @"SELECT a.Nombre_Articulo, um.Nombre_Unidad 
              FROM Articulo a
              LEFT JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
              WHERE a.ID_Articulo = @ID_Articulo",
                    new { ID_Articulo = idArticulo });

                if (articuloInfo == null)
                    throw new Exception("El artículo no existe");

                // 2. Obtener todas las entradas del artículo en la sede
                var entradas = connection.Query<dynamic>(
                    @"SELECT 
                e.ID_Entradas AS IdMovimiento,
                'Entrada' AS TipoMovimiento,
                e.Fecha,
                e.Hora,
                p.Razon_Social AS Proveedor,
                de.Cantidad,
                de.Precio_Unitario,
                de.Total,
                m.Nombre_Movimiento AS Movimiento,
                e.Comentarios
              FROM Detalle_Entrada de
              INNER JOIN Entradas e ON de.ID_Entradas = e.ID_Entradas
              INNER JOIN Movimientos m ON e.ID_Movimiento = m.ID_Movimiento
              INNER JOIN Proveedores p ON e.ID_Proveedores = p.ID_Proveedores
              WHERE e.ID_Sede = @ID_Sede AND de.ID_Articulo = @ID_Articulo
              ORDER BY e.Fecha DESC, e.Hora DESC",
                    new { ID_Sede = idSede, ID_Articulo = idArticulo });

                // 3. Obtener todas las salidas del artículo en la sede
                var salidas = connection.Query<dynamic>(
                    @"SELECT 
                s.ID_Salida AS IdMovimiento,
                'Salida' AS TipoMovimiento,
                s.Fecha,
                s.Hora,
                CONCAT(p.Nombre, ' (', cc.Nombre_CenCost, ')') AS Destino,
                ds.Cantidad,
                ds.Precio_Unitario,
                ds.Total,
                m.Nombre_Movimiento AS Movimiento,
                s.Comentarios
              FROM Detalle_Salida ds
              INNER JOIN Salidas s ON ds.ID_Salida = s.ID_Salida
              INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
              INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
              INNER JOIN Centro_Costo cc ON s.ID_CenCost = cc.ID_CenCost
              WHERE s.ID_Sede = @ID_Sede AND ds.ID_Articulo = @ID_Articulo
              ORDER BY s.Fecha DESC, s.Hora DESC",
                    new { ID_Sede = idSede, ID_Articulo = idArticulo });

                // 4. Combinar y ordenar todos los movimientos
                var movimientosCombinados = entradas.Select(e => new KardexDto
                {
                    IdMovimiento = e.IdMovimiento,
                    TipoMovimiento = e.TipoMovimiento,
                    Fecha = e.Fecha,
                    Hora = e.Hora,
                    OrigenDestino = e.Proveedor,
                    Cantidad = e.Cantidad,
                    PrecioUnitario = e.Precio_Unitario,
                    Total = e.Total,
                    TipoDocumento = e.Movimiento,
                    Comentarios = e.Comentarios,
                    NombreArticulo = articuloInfo.Nombre_Articulo,
                    UnidadMedida = articuloInfo.Nombre_Unidad
                })
                    .Concat(salidas.Select(s => new KardexDto
                    {
                        IdMovimiento = s.IdMovimiento,
                        TipoMovimiento = s.TipoMovimiento,
                        Fecha = s.Fecha,
                        Hora = s.Hora,
                        OrigenDestino = s.Destino,
                        Cantidad = -s.Cantidad, // Las salidas son negativas
                        PrecioUnitario = s.Precio_Unitario,
                        Total = -s.Total, // Los totales de salida son negativos
                        TipoDocumento = s.Movimiento,
                        Comentarios = s.Comentarios,
                        NombreArticulo = articuloInfo.Nombre_Articulo,
                        UnidadMedida = articuloInfo.Nombre_Unidad
                    }))
                    .OrderByDescending(m => m.Fecha)
                    .ThenByDescending(m => m.Hora)
                    .ToList();

                // 5. Calcular saldos acumulados
                int saldoCantidad = 0;
                decimal saldoValor = 0;

                // Primero obtener el inventario inicial
                var inventario = connection.QueryFirstOrDefault<dynamic>(
                    @"SELECT Stock_Actual, Costo_Promedio 
              FROM Inventario 
              WHERE ID_Sede = @ID_Sede AND ID_Articulo = @ID_Articulo",
                    new { ID_Sede = idSede, ID_Articulo = idArticulo });

                if (inventario != null)
                {
                    saldoCantidad = inventario.Stock_Actual;
                    saldoValor = inventario.Stock_Actual * (inventario.Costo_Promedio ?? 0);
                }

                // Procesar movimientos en orden cronológico inverso para calcular saldos
                foreach (var movimiento in movimientosCombinados.OrderBy(m => m.Fecha).ThenBy(m => m.Hora))
                {
                    movimiento.SaldoCantidad = saldoCantidad;
                    movimiento.SaldoValor = saldoValor;

                    // Ajustar saldos
                    saldoCantidad -= movimiento.Cantidad; // Restamos porque las salidas ya son negativas
                    saldoValor -= movimiento.Total; // Restamos porque los totales de salida ya son negativos
                }

                // Ordenar nuevamente por fecha descendente para presentación
                kardex = movimientosCombinados
                    .OrderByDescending(m => m.Fecha)
                    .ThenByDescending(m => m.Hora)
                    .ToList();

                return kardex;
            }
        }
    }
}