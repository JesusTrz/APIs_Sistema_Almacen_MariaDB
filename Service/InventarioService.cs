using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service
{
    public class InventarioService : IInventarioService
    {
        private readonly string _connectionString;

        public InventarioService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Agregar Articulo a Inventario
        public void AgregarArticuloaInventario(AgregarArticuloaInventario invArt)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                if (invArt.ID_Sede <= 0 || invArt.ID_Articulo <= 0)
                    throw new Exception("Debe seleccionar una sede y un artículo válidos.");

                var sedeExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede",
                    new { invArt.ID_Sede });

                if (sedeExistente == 0)
                    throw new Exception("La sede especificada no existe.");

                var articuloExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Articulo WHERE ID_Articulo = @ID_Articulo",
                    new { invArt.ID_Articulo });

                if (articuloExistente == 0)
                    throw new Exception("El artículo ingresado no existe.");

                var yaExiste = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Inventario WHERE ID_Sede = @ID_Sede AND ID_Articulo = @ID_Articulo",
                    new { invArt.ID_Sede, invArt.ID_Articulo });

                if (yaExiste > 0)
                    throw new Exception("El artículo ya existe en el inventario de esta sede.");

                // Validar y sanitizar ubicación
                string ubicacion = string.IsNullOrWhiteSpace(invArt.Ubicacion)
                    ? "S/U"
                    : invArt.Ubicacion.Trim();

                if (!Regex.IsMatch(ubicacion, @"^[a-zA-Z0-9\s\-/]+$"))
                    throw new Exception("La ubicación solo debe contener letras, números y guiones.");

                // Valores por defecto
                int stock = invArt.Stock_Actual ?? 0;
                decimal costoPromedio = invArt.Costo_Promedio ?? 0;
                decimal saldo = stock * costoPromedio;

                string query = @"
                    INSERT INTO Inventario 
                        (ID_Sede, ID_Articulo, Stock_Actual, Stock_Minimo, Stock_Maximo, Ubicacion, Costo_Promedio, Saldo, Ultimo_Costo, Ultima_Compra)
                    VALUES
                        (@ID_Sede, @ID_Articulo, @Stock_Actual, @Stock_Minimo, @Stock_Maximo, @Ubicacion, @Costo_Promedio, @Saldo, @Ultimo_Costo, @Ultima_Compra)";

                connection.Execute(query, new
                {
                    invArt.ID_Sede,
                    invArt.ID_Articulo,
                    Stock_Actual = stock,
                    Stock_Minimo = invArt.Stock_Minimo ?? 0,
                    Stock_Maximo = invArt.Stock_Maximo ?? 0,
                    Ubicacion = ubicacion,
                    Costo_Promedio = costoPromedio,
                    Saldo = saldo,
                    Ultimo_Costo = invArt.Ultimo_Costo ?? 0,
                    Ultima_Compra = invArt.Ultima_Compra ?? DateTime.Now
                });
            }
        }

        #endregion

        #region Obtener articulos de inventario a sede
        public List<InventarioArticulos> GetInventarioPorSede(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                i.ID_Inventario,
                a.ID_Articulo,
                a.Nombre_Articulo,
                a.Descripcion_Articulo,
                a.Numero_Parte,
                a.ID_Linea,
                l.Nombre_Linea,
                a.ID_Medida,
                um.Nombre_Unidad,
                i.Ubicacion,
                i.Stock_Actual,
                i.Stock_Minimo,
                i.Stock_Maximo,
                i.Costo_Promedio,
                i.Saldo,
                i.Ultimo_Costo,
                i.Ultima_Compra
            FROM Inventario i
            INNER JOIN Articulo a ON i.ID_Articulo = a.ID_Articulo
            LEFT JOIN Linea l ON a.ID_Linea = l.ID_Linea
            LEFT JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
            WHERE i.ID_Sede = @ID_Sede
            ORDER BY a.Nombre_Articulo ASC";

                return connection.Query<InventarioArticulos>(query, new { ID_Sede = idSede }).ToList();
            }
        }

        #endregion

        #region Base de Datos
        public bool ReiniciarInventarioPorSede(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    UPDATE Inventario
                    SET 
                        Stock_Actual = 0,
                        Stock_Minimo = 0,
                        Stock_Maximo = 0
                    WHERE ID_Sede = @ID_Sede";

                int filasAfectadas = connection.Execute(query, new { ID_Sede = idSede });

                return filasAfectadas > 0;
            }
        }

        public bool ReiniciarCostoSaldo(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    UPDATE Inventario
                    SET 
                        Costo_Promedio = 0,
                        Saldo = 0
                    WHERE ID_Sede = @ID_Sede";

                int filasAfectadas = connection.Execute(query, new { ID_Sede = idSede });

                return filasAfectadas > 0;
            }
        }

        public bool ReiniciarInventario(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    UPDATE Inventario
                    SET 
                        Stock_Actual = 0,
                        Stock_Minimo = 0,
                        Stock_Maximo = 0,
                        Costo_Promedio = 0,
                        Saldo = 0
                    WHERE ID_Sede = @ID_Sede";

                int filasAfectadas = connection.Execute(query, new { ID_Sede = idSede });

                return filasAfectadas > 0;
            }
        }

        public bool EliminarTodosArticulosInventario(int idSede)
        {
            if (idSede <= 0)
                throw new ArgumentException("ID de sede inválido.", nameof(idSede));

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "DELETE FROM Inventario WHERE ID_Sede = @ID_Sede";

                int filasAfectadas = connection.Execute(query, new { ID_Sede = idSede });

                return filasAfectadas > 0;
            }
        }
        #endregion

        #region Editar Inventario
        public void EditarArticuloInventario(int idInv, AgregarArticuloaInventario invArt)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var inventarioActual = connection.QueryFirstOrDefault<AgregarArticuloaInventario>(
                    "SELECT * FROM Inventario WHERE ID_Inventario = @ID_Inventario",
                    new { ID_Inventario = idInv });

                if (inventarioActual == null)
                    throw new Exception("El inventario no existe.");

                // Validar y sanitizar ubicación
                string ubicacion = string.IsNullOrWhiteSpace(invArt.Ubicacion)
                    ? "S/U"
                    : invArt.Ubicacion.Trim();

                if (!Regex.IsMatch(ubicacion, @"^[a-zA-Z0-9\s\-/]+$"))
                    throw new Exception("La ubicación solo debe contener letras, números y guiones.");

                // Calcular valores
                int stock = invArt.Stock_Actual ?? inventarioActual.Stock_Actual ?? 0;
                decimal costoPromedio = invArt.Costo_Promedio ?? inventarioActual.Costo_Promedio ?? 0;
                decimal saldo = stock * costoPromedio;

                string query = @"
            UPDATE Inventario SET 
                Stock_Actual = @Stock_Actual,
                Stock_Minimo = @Stock_Minimo,
                Stock_Maximo = @Stock_Maximo,
                Ubicacion = @Ubicacion,
                Costo_Promedio = @Costo_Promedio,
                Saldo = @Saldo,
                Ultimo_Costo = @Ultimo_Costo,
                Ultima_Compra = @Ultima_Compra
            WHERE ID_Inventario = @ID_Inventario";

                connection.Execute(query, new
                {
                    Stock_Actual = stock,
                    Stock_Minimo = invArt.Stock_Minimo ?? inventarioActual.Stock_Minimo,
                    Stock_Maximo = invArt.Stock_Maximo ?? inventarioActual.Stock_Maximo,
                    Ubicacion = ubicacion,
                    Costo_Promedio = costoPromedio,
                    Saldo = saldo,
                    Ultimo_Costo = invArt.Ultimo_Costo ?? inventarioActual.Ultimo_Costo,
                    Ultima_Compra = invArt.Ultima_Compra ?? inventarioActual.Ultima_Compra,
                    ID_Inventario = idInv
                });
            }
        }

        #endregion

        #region Eliminar Inventario
        public void EliminarArticulodeInventario(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Inventario WHERE ID_Inventario = @ID_Inventario", new { ID_Inventario = id });
                if (existe == 0)
                    throw new Exception("El articulo no existe en el inventario!");

                string query = "DELETE FROM Inventario WHERE ID_Inventario = @ID_Inventario";
                connection.Execute(query, new { ID_Inventario = id });
            }
        }
        #endregion

        #region Actualizar Stock
        public bool ActualizarStockArticulo(StockEntrada entrada)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                if(entrada.ID_Sede <= 0)
                    throw new Exception("Debe seleccionar una sede válida.");

                var sedeExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede", 
                    new { entrada.ID_Sede });
                if(sedeExistente == 0)
                    throw new Exception("La sede especificada no existe.");
                // Verificar existencia del artículo en la sede
                var inventario = connection.QueryFirstOrDefault<InventarioDto>(
                    @"SELECT ID_Inventario, Stock_Actual, Costo_Promedio 
                      FROM Inventario 
                      WHERE ID_Sede = @ID_Sede AND ID_Articulo = @ID_Articulo",
                    new { entrada.ID_Sede, entrada.ID_Articulo });

                if (inventario == null)
                    throw new Exception("El artículo no existe en el inventario de la sede especificada.");

                // Validar cantidad
                if (entrada.Cantidad <= 0)
                    throw new Exception("La cantidad debe ser mayor a 0.");

                // Calcular nuevo stock y saldo
                int nuevoStock = (int)(inventario.Stock_Actual + entrada.Cantidad);
                decimal nuevoSaldo = (inventario.Costo_Promedio ?? 0) * nuevoStock;

                // Actualizar stock y saldo
                string updateQuery = @"UPDATE Inventario 
                               SET Stock_Actual = @Stock_Actual, Saldo = @Saldo 
                               WHERE ID_Sede = @ID_Sede AND ID_Articulo = @ID_Articulo";

                int filas = connection.Execute(updateQuery, new
                {
                    Stock_Actual = nuevoStock,
                    Saldo = nuevoSaldo,
                    entrada.ID_Sede,
                    entrada.ID_Articulo
                });

                return filas > 0;
            }
        }
        #endregion

        #region Verificar Stock Bajo
        public List<InventarioArticulos> VerificarStockBajo(int idSede)
        {
            if (idSede <= 0)
                throw new ArgumentException("ID de sede inválido.", nameof(idSede));

            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                i.ID_Inventario,
                a.ID_Articulo,
                a.Nombre_Articulo,
                a.Descripcion_Articulo,
                a.Numero_Parte,
                a.ID_Linea,
                l.Nombre_Linea,
                a.ID_Medida,
                um.Nombre_Unidad,
                i.Ubicacion,
                i.Stock_Actual,
                i.Stock_Minimo,
                i.Stock_Maximo,
                i.Costo_Promedio,
                i.Saldo,
                i.Ultimo_Costo,
                i.Ultima_Compra
            FROM Inventario i
            INNER JOIN Articulo a ON i.ID_Articulo = a.ID_Articulo
            LEFT JOIN Linea l ON a.ID_Linea = l.ID_Linea
            LEFT JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
            WHERE i.ID_Sede = @ID_Sede
              AND i.Stock_Actual <= i.Stock_Minimo
            ORDER BY a.Nombre_Articulo ASC";

                return connection.Query<InventarioArticulos>(query, new { ID_Sede = idSede }).ToList();
            }
        }
        #endregion

        #region Stock Alto
        public List<InventarioArticulos> VerificarStockAlto(int idSede)
        {
            if (idSede <= 0)
                throw new ArgumentException("ID de sede inválido.", nameof(idSede));

            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                i.ID_Inventario,
                a.ID_Articulo,
                a.Nombre_Articulo,
                a.Descripcion_Articulo,
                a.Numero_Parte,
                a.ID_Linea,
                l.Nombre_Linea,
                a.ID_Medida,
                um.Nombre_Unidad,
                i.Ubicacion,
                i.Stock_Actual,
                i.Stock_Minimo,
                i.Stock_Maximo,
                i.Costo_Promedio,
                i.Saldo,
                i.Ultimo_Costo,
                i.Ultima_Compra
            FROM Inventario i
            INNER JOIN Articulo a ON i.ID_Articulo = a.ID_Articulo
            LEFT JOIN Linea l ON a.ID_Linea = l.ID_Linea
            LEFT JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
            WHERE i.ID_Sede = @ID_Sede
              AND i.Stock_Actual >= i.Stock_Maximo
            ORDER BY a.Nombre_Articulo DESC";

                return connection.Query<InventarioArticulos>(query, new { ID_Sede = idSede }).ToList();
            }
        }
        #endregion

    }
}