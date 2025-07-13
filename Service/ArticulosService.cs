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
    public class ArticulosService : IArticulosService
    {
        private readonly string _connectionString;

        public ArticulosService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Articulos
        public List<ArticuloDto> GetAllArticulos()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        A.ID_Articulo,
                        A.Nombre_Articulo,
                        A.Descripcion_Articulo,
                        A.Numero_Parte,
                        A.ID_Linea,
                        L.Nombre_Linea,
                        A.ID_Medida,
                        U.Nombre_Unidad
                    FROM Articulo A
                    INNER JOIN Linea L ON A.ID_Linea = L.ID_Linea
                    INNER JOIN Unidades_Medida U ON A.ID_Medida = U.ID_Medida";
                return connection.Query<ArticuloDto>(query).ToList();
            }
        }

        public List<ArticuloDto> GetArticulosById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        A.ID_Articulo,
                        A.Nombre_Articulo,
                        A.Descripcion_Articulo,
                        A.Numero_Parte,
                        A.ID_Linea,
                        L.Nombre_Linea,
                        A.ID_Medida,
                        U.Nombre_Unidad
                    FROM Articulo A
                    INNER JOIN Linea L ON A.ID_Linea = L.ID_Linea
                    INNER JOIN Unidades_Medida U ON A.ID_Medida = U.ID_Medida
                    WHERE A.ID_Articulo = @ID_Articulo";
                return connection.Query<ArticuloDto>(query, new { ID_Articulo = id}).ToList();
            }
        }
        #endregion

        #region Agregar Articulos
        public void AgregarArticulos(ArticulosDatos articulo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                if(articulo.ID_Linea <= 0)
                    throw new Exception("Debe seleccionar una Linea válida.");

                var existeArticulo = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Articulo WHERE Nombre_Articulo = @Nombre_Articulo", 
                    new { articulo.Nombre_Articulo });
                if(existeArticulo > 0)
                    throw new Exception("Ya existe un Articulo con ese Nombre.");

                var lineaExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Linea WHERE ID_Linea = @ID_Linea", 
                    new { articulo.ID_Linea });
                if(lineaExistente == 0)
                    throw new Exception("La Linea agregada no Existe.");

                var unidadExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Unidades_Medida WHERE ID_Medida = @ID_Medida", 
                    new { articulo.ID_Medida });
                if(unidadExistente == 0)
                    throw new Exception("La Unidad agregada no Existe.");

                string query = @"INSERT INTO Articulo (Nombre_Articulo, Descripcion_Articulo, Numero_Parte, ID_Linea, ID_Medida) VALUES (@Nombre_Articulo, @Descripcion_Articulo, @Numero_Parte, @ID_Linea, @ID_Medida)";
                connection.Execute(query, articulo);
            }
        }
        #endregion

        #region Editar Articulo
        public void EditarArticulo(int id, ArticulosDatos articulo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var articuloActual = connection.QueryFirstOrDefault<ArticulosDatos>(
                    "SELECT ID_Articulo, Nombre_Articulo, Descripcion_Articulo, Numero_Parte, ID_Linea, ID_Medida FROM Articulo WHERE ID_Articulo = @ID_Articulo", 
                    new { ID_Articulo = id });

                if(articuloActual == null)
                    throw new Exception("El Articulo no existe.");

                int nuevaLinea = (int)(articulo.ID_Linea > 0 ? articulo.ID_Linea : articuloActual.ID_Linea);
                if(nuevaLinea != articuloActual.ID_Linea)
                {
                    var lineaExistente = connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Linea WHERE ID_Linea = @ID_Linea", 
                        new { ID_Linea = nuevaLinea });

                    if(lineaExistente == 0)
                        throw new Exception("La Linea Agregada no existe.");
                }

                int nuevaMedida = (int)(articulo.ID_Medida > 0 ? articulo.ID_Medida : articuloActual.ID_Medida);
                if(nuevaMedida != articuloActual.ID_Medida)
                {
                    var medidaExistente = connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Unidades_Medida WHERE ID_Medida = @ID_Medida", 
                        new { ID_Medida = nuevaMedida });

                    if(medidaExistente == 0)
                        throw new Exception("La Unidad de Medida Agregada no existe.");
                }

                var nuevoNombre = string.IsNullOrWhiteSpace(articulo.Nombre_Articulo) ? articuloActual.Nombre_Articulo : articulo.Nombre_Articulo;
                var nuevoDesc = string.IsNullOrWhiteSpace(articulo.Descripcion_Articulo) ? articuloActual.Descripcion_Articulo : articulo.Descripcion_Articulo;
                var nuevoNumPart = string.IsNullOrWhiteSpace(articulo.Numero_Parte) ? articuloActual.Numero_Parte : articulo.Numero_Parte;

                string query = @"UPDATE Articulo SET Nombre_Articulo = @Nombre_Articulo, Descripcion_Articulo = @Descripcion_Articulo, Numero_Parte = @Numero_Parte, ID_Linea = @ID_Linea, ID_Medida = @ID_Medida WHERE ID_Articulo = @ID_Articulo";
                connection.Execute(query, new
                {
                    Nombre_Articulo = nuevoNombre,
                    Descripcion_Articulo = nuevoDesc,
                    Numero_Parte = nuevoNumPart,
                    ID_Linea = nuevaLinea,
                    ID_Medida = nuevaMedida,
                    ID_Articulo = id
                });
            }
        }
        #endregion

        #region Eliminar Articulo
        public void EliminarArticulo (int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Inventario WHERE ID_Articulo = @ID_Articulo", 
                    new { ID_Articulo = id });
                if(enUso > 0)
                    throw new Exception("No se puede eliminar el Articulo porque está en uso.");

                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Articulo WHERE ID_Articulo = @ID_Articulo", 
                    new { ID_Articulo = id });
                if(existe == 0)
                    throw new Exception("El Articulo no Existe!");

                string query = "DELETE FROM Articulo WHERE ID_Articulo = @ID_Articulo";
                connection.Execute(query, new { ID_Articulo = id });
            }
        }
        #endregion

    }
}