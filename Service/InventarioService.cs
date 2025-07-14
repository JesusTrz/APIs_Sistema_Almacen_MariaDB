using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
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
                if(invArt.ID_Sede <= 0)
                    throw new Exception("Debe seleccionar una sede válida.");

                var sedeExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede", 
                    new { invArt.ID_Sede});
                if(sedeExistente == 0)
                    throw new Exception("La sede especificada no existe.");

                var articuloExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Articulo WHERE ID_Articulo = @ID_Articulo", 
                    new { invArt.ID_Articulo });
                if(articuloExistente == 0)
                    throw new Exception("El Articulo ingresado no existe.");

                var articuloInvExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Inventario WHERE ID_Articulo = @ID_Articulo", 
                    new { invArt.ID_Articulo });
                if(articuloExistente == 0)
                    throw new Exception("El Articulo ingresado ya existe en este Inventario!");

                string query = "INSERT INTO Inventario (ID_Articulo, ID_Sede) Values (@ID_Articulo, @ID_Sede)";
                connection.Execute(query, invArt);
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

        #region Editar Inventario
        public void EditarArticuloInventario(int idInv, InventarioDatos inventario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var inventarioActual = connection.QueryFirstOrDefault<InventarioDatos>(
                         "SELECT ID_Inventario, ID_Sede, ID_Articulo, Stock_Actual, Stock_Minimo, Stock_Maximo, Ubicacion, Costo_Promedio, Saldo, Ultimo_Costo, Ultima_Compra FROM Inventario WHERE ID_Inventario = @ID_Inventario",
                         new { ID_Inventario = idInv });

                if (inventarioActual == null)
                    throw new Exception("El inventario no existe.");

                string ubicacion = string.IsNullOrWhiteSpace(inventario.Ubicacion)
                    ? "Sin Ubicación"
                    : inventario.Ubicacion.Trim();

                if(!System.Text.RegularExpressions.Regex.IsMatch(ubicacion, @"^[a-zA-Z0-9-]+$"))
                  throw new Exception("La Ubicación solo debe contener letras y números.");

                #region Calcular Saldo
                decimal costoPromedio = inventario.Costo_Promedio ?? inventarioActual.Costo_Promedio ?? 0;
                int stockActual = inventario.Stock_Actual ?? inventarioActual.Stock_Actual ?? 0;
                decimal saldo = costoPromedio * stockActual;
                #endregion

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
                    Stock_Actual = stockActual,
                    Stock_Minimo = inventario.Stock_Minimo ?? inventarioActual.Stock_Minimo,
                    Stock_Maximo = inventario.Stock_Maximo ?? inventarioActual.Stock_Maximo,
                    Ubicacion = ubicacion,
                    Costo_Promedio = costoPromedio,
                    Saldo = saldo,
                    Ultimo_Costo = inventario.Ultimo_Costo ?? inventarioActual.Ultimo_Costo,
                    Ultima_Compra = inventario.Ultima_Compra ?? inventarioActual.Ultima_Compra,
                    ID_Inventario = idInv
                });


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

        #region Filtro
        public List<ExpandoObject> ObtenerInventarioFiltrado(InventarioFiltro filtros)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                i.ID_Inventario,
                i.Stock_Actual,
                i.Stock_Minimo,
                i.Stock_Maximo,
                i.Costo_Promedio,
                i.Saldo,
                i.Ultimo_Costo,
                i.Ultima_Compra,
                i.Ubicacion,
                a.ID_Articulo,
                a.Nombre_Articulo,
                a.Descripcion_Articulo,
                a.Numero_Parte,
                um.Nombre_Unidad,
                l.Nombre_Linea
            FROM Inventario i
            INNER JOIN Articulo a ON a.ID_Articulo = i.ID_Articulo
            LEFT JOIN Unidades_Medida um ON a.ID_Medida = um.ID_Medida
            LEFT JOIN Linea l ON a.ID_Linea = l.ID_Linea
            WHERE i.ID_Sede = @ID_Sede";

                var datos = connection.Query<dynamic>(query, new { ID_Sede = filtros.ID_Sede });

                var resultado = new List<ExpandoObject>();

                foreach (var item in datos)
                {
                    IDictionary<string, object> obj = new ExpandoObject();

                    foreach (var campo in filtros.Campos)
                    {
                        if (((IDictionary<string, object>)item).ContainsKey(campo))
                        {
                            obj[campo] = ((IDictionary<string, object>)item)[campo];
                        }
                        else
                        {
                            // Convertir nombres según sea necesario
                            switch (campo)
                            {
                                case "Nombre_Unidad": obj[campo] = item.Nombre_Unidad; break;
                                case "Nombre_Linea": obj[campo] = item.Nombre_Linea; break;
                                case "Nombre_Articulo": obj[campo] = item.Nombre_Articulo; break;
                                case "Descripcion_Articulo": obj[campo] = item.Descripcion_Articulo; break;
                                case "Numero_Parte": obj[campo] = item.Numero_Parte; break;
                                case "Ubicacion": obj[campo] = item.Ubicacion; break;
                                case "Stock_Actual": obj[campo] = item.Stock_Actual; break;
                                case "Stock_Minimo": obj[campo] = item.Stock_Minimo; break;
                                case "Stock_Maximo": obj[campo] = item.Stock_Maximo; break;
                                case "Costo_Promedio": obj[campo] = item.Costo_Promedio; break;
                                case "Saldo": obj[campo] = item.Saldo; break;
                                case "Ultimo_Costo": obj[campo] = item.Ultimo_Costo; break;
                                case "Ultima_Compra": obj[campo] = item.Ultima_Compra; break;
                                default: break;
                            }
                        }
                    }

                    resultado.Add((ExpandoObject)obj);
                }

                return resultado;
            }
        }

        #endregion

    }
}