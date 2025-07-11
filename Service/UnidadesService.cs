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
    public class UnidadesService : IUnidadesService
    {
        private readonly string _connectionString;

        public UnidadesService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Unidades
        public List<UnidadesDto> GetAllUnidades()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Unidad, Numero_Placa, Descripcion_Unidad, ID_Sede FROM Unidades";
                return connection.Query<UnidadesDto>(query).ToList();
            }
        }

        public List<UnidadesDto> GetUnidadesById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_Unidad, Numero_Placa, Descripcion_Unidad, ID_Sede 
                                 FROM Unidades 
                                 WHERE ID_Unidad = @ID_Unidad";
                return connection.Query<UnidadesDto>(query, new { ID_Unidad = id }).ToList();
            }
        }

        public List<UnidadesDto> GetUnidadesByIdSede(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"SELECT ID_Unidad, Numero_Placa, Descripcion_Unidad, ID_Sede 
                                 FROM Unidades 
                                 WHERE ID_Sede = @ID_Sede";
                return connection.Query<UnidadesDto>(query, new { ID_Sede =  idSede }).ToList();
            }
        }
        #endregion

        #region Agregar nuevas Unidades
        public void AgregarUnidad(UnidadesDatos unidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                if (unidad.ID_Sede <= 0)
                    throw new Exception("Debe seleccionar una sede válida.");

                var existeUnidad = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Unidades WHERE Numero_Placa = @Numero_Placa",
                    new { unidad.Numero_Placa });

                if (existeUnidad > 0)
                    throw new Exception("Ya existe una unidad con ese número de placa.");

                var sedeExiste = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede",
                    new { unidad.ID_Sede });

                if (sedeExiste == 0)
                    throw new Exception("La sede especificada no existe.");

                string query = @"INSERT INTO Unidades (Numero_Placa, Descripcion_Unidad, ID_Sede) 
                                 VALUES (@Numero_Placa, @Descripcion_Unidad, @ID_Sede)";
                connection.Execute(query, unidad);
            }
        }
        #endregion

        #region Editar Unidades
        public void EditarUnidades(int id, UnidadesDatos unidad)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var unidadActual = connection.QueryFirstOrDefault<UnidadesDatos>(
                    @"SELECT ID_Unidad, Numero_Placa, Descripcion_Unidad, ID_Sede 
                      FROM Unidades 
                      WHERE ID_Unidad = @ID_Unidad",
                    new { ID_Unidad = id });

                if (unidadActual == null)
                    throw new Exception("La unidad no existe.");

                // Validar si ID_Sede se proporcionó y es diferente
                int nuevaSede = (int)(unidad.ID_Sede > 0 ? unidad.ID_Sede : unidadActual.ID_Sede);

                if (nuevaSede != unidadActual.ID_Sede)
                {
                    var sedeExiste = connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede",
                        new { ID_Sede = nuevaSede });

                    if (sedeExiste == 0)
                        throw new Exception("La sede especificada no existe.");
                }

                string nuevaPlaca = string.IsNullOrWhiteSpace(unidad.Numero_Placa) ? unidadActual.Numero_Placa : unidad.Numero_Placa;
                string nuevaDescripcion = string.IsNullOrWhiteSpace(unidad.Descripcion_Unidad) ? unidadActual.Descripcion_Unidad : unidad.Descripcion_Unidad;

                string query = @"UPDATE Unidades 
                                 SET Numero_Placa = @Numero_Placa, 
                                     Descripcion_Unidad = @Descripcion_Unidad, 
                                     ID_Sede = @ID_Sede 
                                 WHERE ID_Unidad = @ID_Unidad";

                connection.Execute(query, new
                {
                    Numero_Placa = nuevaPlaca,
                    Descripcion_Unidad = nuevaDescripcion,
                    ID_Sede = nuevaSede,
                    ID_Unidad = id
                });
            }
        }
        #endregion

        #region Eliminar Unidades
        public void EliminarUnidad(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Salidas WHERE ID_Unidad = @ID_Unidad",
                    new { ID_Unidad = id });

                if (enUso > 0)
                    throw new Exception("No se puede eliminar la unidad porque está en uso.");

                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Unidades WHERE ID_Unidad = @ID_Unidad",
                    new { ID_Unidad = id });

                if (existe == 0)
                    throw new Exception("La unidad no existe.");

                string query = "DELETE FROM Unidades WHERE ID_Unidad = @ID_Unidad";
                connection.Execute(query, new { ID_Unidad = id });
            }
        }
        #endregion
    }
}