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
    public class SalidaService : ISalidaService
    {
        private readonly string _connectionString;

        public SalidaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Agregar Salida
        public bool RegistrarSalidasyDetalles(SalidaDto salidaDto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Verificar stock disponible por artículo
                        foreach (var detalle in salidaDto.Detalles)
                        {
                            var inventario = connection.QueryFirstOrDefault<dynamic>(
                                @"SELECT Stock_Actual FROM Inventario 
                          WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede",
                                new { detalle.ID_Articulo, salidaDto.ID_Sede },
                                transaction);

                            if (inventario == null)
                                throw new Exception($"No se encontró inventario para el artículo {detalle.ID_Articulo}");

                            if ((int)inventario.Stock_Actual < detalle.Cantidad)
                                throw new Exception($"No hay suficiente stock para el artículo {detalle.ID_Articulo}. Stock actual: {inventario.Stock_Actual}, solicitado: {detalle.Cantidad}");
                        }

                        // Insertar salida
                        var sqlSalida = @"
                    INSERT INTO Salidas (Fecha, Hora, ID_CenCost, ID_Unidad, ID_Personal, ID_Movimiento, Comentarios, ID_Sede)
                    VALUES (@Fecha, @Hora, @ID_CenCost, @ID_Unidad, @ID_Personal, @ID_Movimiento, @Comentarios, @ID_Sede);
                    SELECT LAST_INSERT_ID();";

                        var idSalida = connection.ExecuteScalar<int>(sqlSalida, new
                        {
                            Fecha = DateTime.Now.Date,
                            Hora = DateTime.Now.TimeOfDay,
                            salidaDto.ID_CenCost,
                            salidaDto.ID_Unidad,
                            salidaDto.ID_Personal,
                            salidaDto.ID_Movimiento,
                            salidaDto.Comentarios,
                            salidaDto.ID_Sede
                        }, transaction);

                        // Insertar detalles de salida y actualizar inventario
                        foreach (var detalle in salidaDto.Detalles)
                        {
                            var total = detalle.Cantidad * detalle.Precio_Unitario;

                            var sqlDetalle = @"
                        INSERT INTO Detalle_Salida (ID_Salida, ID_Articulo, Cantidad, Precio_Unitario, Total)
                        VALUES (@ID_Salida, @ID_Articulo, @Cantidad, @Precio_Unitario, @Total);";

                            connection.Execute(sqlDetalle, new
                            {
                                ID_Salida = idSalida,
                                detalle.ID_Articulo,
                                detalle.Cantidad,
                                detalle.Precio_Unitario,
                                Total = total
                            }, transaction);

                            // Actualizar inventario
                            var sqlUpdateInventario = @"
                        UPDATE Inventario 
                        SET 
                            Stock_Actual = Stock_Actual - @Cantidad,
                            Ultimo_Costo = @Precio_Unitario,
                            Ultima_Compra = @Ultima_Compra,
                            Saldo = (Stock_Actual - @Cantidad) * Costo_Promedio
                        WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede;";

                            connection.Execute(sqlUpdateInventario, new
                            {
                                detalle.Cantidad,
                                detalle.Precio_Unitario,
                                Ultima_Compra = DateTime.Now,
                                detalle.ID_Articulo,
                                salidaDto.ID_Sede
                            }, transaction);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al registrar la salida: " + ex.Message);
                    }
                }
            }
        }

        #endregion

        #region Obtener Salidas y Detalles
        public List<GetSalidasDto> ObtenerSalidasporSede(int idSede)
        {
            var salidasDict = new Dictionary<int, GetSalidasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
            SELECT 
                s.ID_Salida, s.Fecha, s.Hora, s.ID_Movimiento, s.ID_CenCost, s.ID_Unidad, 
                s.ID_Personal, s.Comentarios, s.ID_Sede,
                m.Nombre_Movimiento, m.Descripcion_Movimiento,
                cc.Nombre_CenCost,
                u.Numero_Placa,
                p.Nombre,
                ds.ID_Articulo, ds.Cantidad, ds.Precio_Unitario, ds.Total,
                a.Nombre_Articulo,
                um.Nombre_Unidad
            FROM Salidas s
            INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
            INNER JOIN Centro_Costo cc ON s.ID_CenCost = cc.ID_CenCost
            INNER JOIN Unidades u ON s.ID_Unidad = u.ID_Unidad
            INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
            INNER JOIN Detalle_Salida ds ON s.ID_Salida = ds.ID_Salida
            INNER JOIN Articulo a ON ds.ID_Articulo = a.ID_Articulo
            INNER JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
            WHERE s.ID_Sede = @ID_Sede
            ORDER BY s.ID_Salida DESC;
        ";

                var salidas = connection.Query<GetSalidasDto, GetDetalleSalidasDto, GetSalidasDto>(
                    sql,
                    (salida, detalle) =>
                    {
                        if (!salidasDict.TryGetValue(salida.ID_Salida, out var salidaExistente))
                        {
                            salidaExistente = salida;
                            salidaExistente.Detalles = new List<GetDetalleSalidasDto>();
                            salidasDict.Add(salida.ID_Salida, salidaExistente);
                        }

                        salidaExistente.Detalles.Add(detalle);
                        return salidaExistente;
                    },
                    new { ID_Sede = idSede },
                    splitOn: "ID_Articulo"
                ).Distinct().ToList();
            }

            return salidasDict.Values.ToList();
        }

        #endregion

        #region Eliminar Salidas
        public void EliminarSalidas(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Salidas WHERE ID_Salida = @ID_Salida", 
                    new { ID_Salida = id });

                if(existe == 0)
                    throw new Exception("La Salida no Existe!");

                string query = "DELETE FROM Salidas WHERE ID_Salida = @ID_Salida";
                connection.Execute(query, new { ID_Salida = id });
            }
        }

        public void EliminarArticulosSalidas(int idSalida, int idArticulo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM Detalle_Salida WHERE ID_Salida = @ID_Salida AND ID_Articulo = @ID_Articulo", 
                    new { ID_Salida = idSalida, ID_Articulo = idArticulo });

                if (existe == 0)
                    throw new Exception("El Articulo no fue encontrado en la Salida.");

                string query = @"DELETE FROM Detalle_Salida WHERE ID_Salida = @ID_Salida AND ID_Articulo = @ID_Articulo";

                connection.Execute(query, new { ID_Salida = idSalida, ID_Articulo = idArticulo });
            }
        }
        #endregion

        #region Modificar Salida y Detalles
        public bool ActualizarSalidasyDetalles(int idSalida, GetSalidasDto dto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Verificar que la salida exista
                        var salidaExistente = connection.QueryFirstOrDefault<GetSalidasDto>(
                            "SELECT * FROM Salidas WHERE ID_Salida = @ID_Salida",
                            new { ID_Salida = idSalida }, transaction);

                        if (salidaExistente == null)
                            return false;

                        // 2. Actualizar cabecera
                        string updateSalidaSql = @"
                    UPDATE Salidas SET
                        ID_CenCost = @ID_CenCost,
                        ID_Unidad = @ID_Unidad,
                        ID_Personal = @ID_Personal,
                        Comentarios = @Comentarios,
                        ID_Sede = @ID_Sede,
                        Fecha = @Fecha,
                        Hora = @Hora
                    WHERE ID_Salida = @ID_Salida";

                        connection.Execute(updateSalidaSql, new
                        {
                            ID_CenCost = dto.ID_CenCost,
                            ID_Unidad = dto.ID_Unidad,
                            ID_Personal = dto.ID_Personal,
                            Comentarios = dto.Comentarios,
                            ID_Sede = dto.ID_Sede,
                            Fecha = DateTime.Now.Date,
                            Hora = DateTime.Now.TimeOfDay,
                            ID_Salida = idSalida
                        }, transaction);

                        // 3. Obtener detalles anteriores
                        var detallesAntiguos = connection.Query<GetDetalleSalidasDto>(
                            "SELECT * FROM Detalle_Salida WHERE ID_Salida = @ID_Salida",
                            new { ID_Salida = idSalida }, transaction).ToList();

                        foreach (var antiguo in detallesAntiguos)
                        {
                            connection.Execute(@"
                        UPDATE Inventario
                        SET Stock_Actual = Stock_Actual + @Cantidad,
                            Saldo = (Stock_Actual + @Cantidad) * Costo_Promedio
                        WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede",
                                new
                                {
                                    Cantidad = antiguo.Cantidad,
                                    ID_Articulo = antiguo.ID_Articulo,
                                    ID_Sede = dto.ID_Sede
                                }, transaction);
                        }

                        // 4. Eliminar detalles anteriores
                        connection.Execute(
                            "DELETE FROM Detalle_Salida WHERE ID_Salida = @ID_Salida",
                            new { ID_Salida = idSalida }, transaction);

                        // 5. Insertar nuevos detalles y actualizar inventario
                        foreach (var detalle in dto.Detalles)
                        {
                            // Verificar existencia en inventario
                            var inventario = connection.QueryFirstOrDefault<InventarioDto>(
                                "SELECT * FROM Inventario WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede",
                                new { detalle.ID_Articulo, dto.ID_Sede }, transaction);

                            if (inventario == null)
                                throw new Exception("Inventario no encontrado para el artículo " + detalle.ID_Articulo);

                            // Insertar nuevo detalle
                            connection.Execute(@"
                        INSERT INTO Detalle_Salida
                            (ID_Salida, ID_Articulo, Cantidad, Precio_Unitario, Total)
                        VALUES
                            (@ID_Salida, @ID_Articulo, @Cantidad, @Precio_Unitario, @Total)",
                                new
                                {
                                    ID_Salida = idSalida,
                                    ID_Articulo = detalle.ID_Articulo,
                                    Cantidad = detalle.Cantidad,
                                    Precio_Unitario = detalle.Precio_Unitario,
                                    Total = detalle.Cantidad * detalle.Precio_Unitario
                                }, transaction);

                            // Actualizar inventario
                            connection.Execute(@"
                        UPDATE Inventario
                        SET 
                            Stock_Actual = Stock_Actual - @Cantidad,
                            Ultimo_Costo = @Precio_Unitario,
                            Ultima_Compra = @FechaActual,
                            Costo_Promedio = (Costo_Promedio + @Precio_Unitario) / 2,
                            Saldo = (Stock_Actual - @Cantidad) * ((Costo_Promedio + @Precio_Unitario) / 2)
                        WHERE ID_Articulo = @ID_Articulo AND ID_Sede = @ID_Sede",
                                new
                                {
                                    Cantidad = detalle.Cantidad,
                                    Precio_Unitario = detalle.Precio_Unitario,
                                    FechaActual = DateTime.Now,
                                    ID_Articulo = detalle.ID_Articulo,
                                    ID_Sede = dto.ID_Sede
                                }, transaction);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al actualizar la salida: " + ex.Message);
                    }
                }
            }
        }

        #endregion

        #region OBTENER SALIDA #1 
        public List<GetSalidasDto> ObtenerSalidas(
     int? idSalida,
     DateTime? fechaInicio,
     DateTime? fechaFin,
     int? idSede = null) // 👈 Nuevo parámetro
        {
            var sql = @"
SELECT  
    s.ID_Salida, s.Fecha, s.Hora, s.ID_Movimiento, s.ID_CenCost, s.ID_Unidad, s.ID_Personal, s.Comentarios, s.ID_Sede,
    m.Nombre_Movimiento, m.Descripcion_Movimiento,
    cc.Nombre_CenCost,
    u.Numero_Placa,
    p.Nombre,
    d.ID_Articulo, d.Cantidad, d.Precio_Unitario, d.Total,
    a.Nombre_Articulo, um.Nombre_Unidad
FROM Salidas s
INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
INNER JOIN centro_costo cc ON s.ID_CenCost = cc.ID_CenCost
INNER JOIN Unidades u ON s.ID_Unidad = u.ID_Unidad
INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
INNER JOIN Detalle_Salida d ON s.ID_Salida = d.ID_Salida
INNER JOIN Articulo a ON d.ID_Articulo = a.ID_Articulo
INNER JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
WHERE (@IdSalida IS NULL OR s.ID_Salida = @IdSalida)
  AND (@FechaInicio IS NULL OR s.Fecha >= @FechaInicio)
  AND (@FechaFin IS NULL OR s.Fecha <= @FechaFin)
  AND (@IdSede IS NULL OR s.ID_Sede = @IdSede) -- 👈 Filtro por sede
ORDER BY s.Fecha DESC;";

            var parametros = new DynamicParameters();
            parametros.Add("@IdSalida", idSalida);
            parametros.Add("@FechaInicio", fechaInicio);
            parametros.Add("@FechaFin", fechaFin);
            parametros.Add("@IdSede", idSede);

            var salidasDict = new Dictionary<int, GetSalidasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Query<GetSalidasDto, GetDetalleSalidasDto, GetSalidasDto>(
                    sql,
                    (salida, detalle) =>
                    {
                        if (!salidasDict.TryGetValue(salida.ID_Salida, out var salidaExistente))
                        {
                            salida.Detalles = new List<GetDetalleSalidasDto>();
                            salidasDict.Add(salida.ID_Salida, salida);
                            salidaExistente = salida;
                        }

                        salidaExistente.Detalles.Add(detalle);
                        return salidaExistente;
                    },
                    parametros,
                    splitOn: "ID_Articulo"
                );
            }

            return salidasDict.Values.ToList();
        }


        #endregion

        #region SALIDAS FILTRADAS #2
        public List<GetSalidasDto> ObtenerSalidasFiltradas(
     DateTime? fechaInicio,
     DateTime? fechaFin,
     int? folioInicio,
     int? folioFin,
     int? idCentroCosto,
     int? idUnidad,
     int? idArticulo,
     int? idSede // Nuevo parámetro
 )
        {
            var sql = @"
    SELECT s.ID_Salida, s.Fecha, s.Hora, s.ID_Movimiento, m.Nombre_Movimiento, m.Descripcion_Movimiento,
           s.ID_CenCost, cc.Nombre_CenCost, s.ID_Unidad, u.Numero_Placa,
           s.ID_Personal, p.Nombre, s.Comentarios,
           s.ID_Sede,
           ds.ID_Articulo, a.Nombre_Articulo, um.Nombre_Unidad,
           ds.Cantidad, ds.Precio_Unitario, ds.Total
    FROM Salidas s
    INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
    INNER JOIN centro_costo cc ON s.ID_CenCost = cc.ID_CenCost
    INNER JOIN Unidades u ON s.ID_Unidad = u.ID_Unidad
    INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
    INNER JOIN Detalle_Salida ds ON s.ID_Salida = ds.ID_Salida
    INNER JOIN Articulo a ON ds.ID_Articulo = a.ID_Articulo
    INNER JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
    WHERE (@fechaInicio IS NULL OR s.Fecha >= @fechaInicio)
      AND (@fechaFin IS NULL OR s.Fecha <= @fechaFin)
      AND (@folioInicio IS NULL OR s.ID_Salida >= @folioInicio)
      AND (@folioFin IS NULL OR s.ID_Salida <= @folioFin)
      AND (@idCentroCosto IS NULL OR s.ID_CenCost = @idCentroCosto)
      AND (@idUnidad IS NULL OR s.ID_Unidad = @idUnidad)
      AND (@idArticulo IS NULL OR ds.ID_Articulo = @idArticulo)
      AND (@idSede IS NULL OR s.ID_Sede = @idSede) -- Filtro por sede
    ORDER BY s.ID_Salida ASC";

            var parametros = new DynamicParameters();
            parametros.Add("@fechaInicio", fechaInicio);
            parametros.Add("@fechaFin", fechaFin);
            parametros.Add("@folioInicio", folioInicio);
            parametros.Add("@folioFin", folioFin);
            parametros.Add("@idCentroCosto", idCentroCosto);
            parametros.Add("@idUnidad", idUnidad);
            parametros.Add("@idArticulo", idArticulo);
            parametros.Add("@idSede", idSede); // Pasar parámetro

            var salidasDict = new Dictionary<int, GetSalidasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Query<GetSalidasDto, GetDetalleSalidasDto, GetSalidasDto>(
                    sql,
                    (salida, detalle) =>
                    {
                        if (!salidasDict.TryGetValue(salida.ID_Salida, out var salidaExistente))
                        {
                            salida.Detalles = new List<GetDetalleSalidasDto>();
                            salidasDict.Add(salida.ID_Salida, salida);
                            salidaExistente = salida;
                        }
                        salidaExistente.Detalles.Add(detalle);
                        return salidaExistente;
                    },
                    parametros,
                    splitOn: "ID_Articulo"
                );
            }

            return salidasDict.Values.ToList();
        }

        #endregion

        #region SALIDAS POR ARTICULO #3
        public List<GetSalidasDto> ObtenerSalidasPorArticulo(
      DateTime? fechaInicio,
      DateTime? fechaFin,
      int? idArticulo,
      int? idSede) // 👈 nuevo parámetro
        {
            var sql = @"
        SELECT 
            s.ID_Salida, s.Fecha, s.Hora, s.ID_Movimiento, 
            m.Nombre_Movimiento, m.Descripcion_Movimiento,
            s.ID_CenCost, cc.Nombre_CenCost, 
            s.ID_Unidad, u.Numero_Placa,
            s.ID_Personal, p.Nombre, 
            s.Comentarios, s.ID_Sede,
            ds.ID_Articulo, a.Nombre_Articulo, um.Nombre_Unidad,
            ds.Cantidad, ds.Precio_Unitario, ds.Total
        FROM Salidas s
        INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
        INNER JOIN centro_costo cc ON s.ID_CenCost = cc.ID_CenCost
        INNER JOIN Unidades u ON s.ID_Unidad = u.ID_Unidad
        INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
        INNER JOIN Detalle_Salida ds ON s.ID_Salida = ds.ID_Salida
        INNER JOIN Articulo a ON ds.ID_Articulo = a.ID_Articulo
        INNER JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
        WHERE (@fechaInicio IS NULL OR s.Fecha >= @fechaInicio)
          AND (@fechaFin IS NULL OR s.Fecha <= @fechaFin)
          AND (@idArticulo IS NULL OR ds.ID_Articulo = @idArticulo)
          AND (@idSede IS NULL OR s.ID_Sede = @idSede) -- 👈 filtro por sede
        ORDER BY s.ID_Salida ASC;";

            var parametros = new DynamicParameters();
            parametros.Add("@fechaInicio", fechaInicio);
            parametros.Add("@fechaFin", fechaFin);
            parametros.Add("@idArticulo", idArticulo);
            parametros.Add("@idSede", idSede);

            var salidasDict = new Dictionary<int, GetSalidasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Query<GetSalidasDto, GetDetalleSalidasDto, GetSalidasDto>(
                    sql,
                    (salida, detalle) =>
                    {
                        if (!salidasDict.TryGetValue(salida.ID_Salida, out var salidaExistente))
                        {
                            salida.Detalles = new List<GetDetalleSalidasDto>();
                            salidasDict.Add(salida.ID_Salida, salida);
                            salidaExistente = salida;
                        }

                        salidaExistente.Detalles.Add(detalle);
                        return salidaExistente;
                    },
                    parametros,
                    splitOn: "ID_Articulo"
                );
            }

            return salidasDict.Values.ToList();
        }

        #endregion

        #region SALIDAS POR MOVIMIENTO #4
        public List<GetSalidasDto> ObtenerSalidasPorMovimiento(
     DateTime? fechaInicio,
     DateTime? fechaFin,
     int? idMovimiento,
     int? idSede) // 👈 nuevo parámetro
        {
            var sql = @"
        SELECT 
            s.ID_Salida, s.Fecha, s.Hora, s.ID_Movimiento, 
            m.Nombre_Movimiento, m.Descripcion_Movimiento,
            s.ID_CenCost, cc.Nombre_CenCost, 
            s.ID_Unidad, u.Numero_Placa,
            s.ID_Personal, p.Nombre, 
            s.Comentarios,
            s.ID_Sede,
            ds.ID_Articulo, a.Nombre_Articulo, um.Nombre_Unidad,
            ds.Cantidad, ds.Precio_Unitario, ds.Total
        FROM Salidas s
        INNER JOIN Movimientos m ON s.ID_Movimiento = m.ID_Movimiento
        INNER JOIN centro_costo cc ON s.ID_CenCost = cc.ID_CenCost
        INNER JOIN Unidades u ON s.ID_Unidad = u.ID_Unidad
        INNER JOIN Personal p ON s.ID_Personal = p.ID_Personal
        INNER JOIN Detalle_Salida ds ON s.ID_Salida = ds.ID_Salida
        INNER JOIN Articulo a ON ds.ID_Articulo = a.ID_Articulo
        INNER JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
        WHERE (@fechaInicio IS NULL OR s.Fecha >= @fechaInicio)
          AND (@fechaFin IS NULL OR s.Fecha <= @fechaFin)
          AND (@idMovimiento IS NULL OR s.ID_Movimiento = @idMovimiento)
          AND (@idSede IS NULL OR s.ID_Sede = @idSede) -- 👈 filtro por sede
        ORDER BY m.Nombre_Movimiento, s.ID_Salida ASC;";

            var parametros = new DynamicParameters();
            parametros.Add("@fechaInicio", fechaInicio);
            parametros.Add("@fechaFin", fechaFin);
            parametros.Add("@idMovimiento", idMovimiento);
            parametros.Add("@idSede", idSede);

            var salidasDict = new Dictionary<int, GetSalidasDto>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Query<GetSalidasDto, GetDetalleSalidasDto, GetSalidasDto>(
                    sql,
                    (salida, detalle) =>
                    {
                        if (!salidasDict.TryGetValue(salida.ID_Salida, out var salidaExistente))
                        {
                            salida.Detalles = new List<GetDetalleSalidasDto>();
                            salidasDict.Add(salida.ID_Salida, salida);
                            salidaExistente = salida;
                        }
                        salidaExistente.Detalles.Add(detalle);
                        return salidaExistente;
                    },
                    parametros,
                    splitOn: "ID_Articulo"
                );
            }

            return salidasDict.Values.ToList();
        }

        #endregion
    }
}