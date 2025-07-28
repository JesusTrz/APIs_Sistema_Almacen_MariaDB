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
    public class UnidadesMedidaService : IUnidadesMedidaService
    {
        private readonly string _connectionString;

        public UnidadesMedidaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Unidades de Medida
        public List<UnidadesMedidaDto> GetAllUMedida()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Medida, Nombre_Unidad, Descripcion_Unidad FROM Unidades_Medida";
                return connection.Query<UnidadesMedidaDto>(query).ToList();
            }
        }

        public List<UnidadesMedidaDto> GetMedidaById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_Medida, Nombre_Unidad, Descripcion_Unidad FROM Unidades_Medida WHERE ID_Medida = @ID_Medida";
                return connection.Query<UnidadesMedidaDto>(query, new { ID_Medida = id }).ToList();
            }
        }
        #endregion

        #region Crear nueva Unidad de Medida
        public void AgregarUnidadMedida(UnidadesMedidaNombre medida)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Unidades_Medida (Nombre_Unidad, Descripcion_Unidad) VALUES (@Nombre_Unidad, @Descripcion_Unidad)";
                connection.Execute(query, new { Nombre_Unidad = medida.Nombre_Unidad, Descripcion_Unidad = medida.Descripcion_Unidad });
            }
        }
        #endregion

        #region Editar Unidad de Medida
        public void EditarUnidadMedida(int id, string nuevoNombre, string nuevaDesc)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Verificar si la unidad existe
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Unidades_Medida WHERE ID_Medida = @ID",
                    new { ID = id });

                if (existe == 0)
                    throw new Exception("La unidad de medida no existe.");

                // Verificar si el nuevo nombre ya existe (opcional)
                var duplicado = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Unidades_Medida WHERE Nombre_Unidad = @Nombre AND ID_Medida != @ID",
                    new { Nombre = nuevoNombre, ID = id });

                if (duplicado > 0)
                    throw new Exception("Ya existe otra unidad de medida con ese nombre.");

                // Realizar la actualización
                string query = @"UPDATE Unidades_Medida 
                         SET Nombre_Unidad = @Nombre, 
                             Descripcion_Unidad = @Descripcion_Unidad 
                         WHERE ID_Medida = @ID";

                connection.Execute(query, new
                {
                    Nombre = nuevoNombre,
                    Descripcion_Unidad = nuevaDesc,
                    ID = id
                });
            }
        }
        #endregion

        #region Eliminar Unidad de Medida
        public void EliminarUnidadMedida(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Validar si la unidad está en uso en la tabla ARTICULO
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM ARTICULO WHERE ID_Medida = @ID",
                    new { ID = id });

                if (enUso > 0)
                    throw new Exception("No se puede eliminar la unidad porque está siendo utilizada por uno o más artículos.");

                // Ejecutar la eliminación
                string query = "DELETE FROM Unidades_Medida WHERE ID_Medida = @ID_Medida";
                connection.Execute(query, new { ID_Medida = id });
            }
        }

        #endregion
    }
}