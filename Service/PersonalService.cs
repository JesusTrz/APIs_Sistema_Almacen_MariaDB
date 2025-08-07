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
    public class PersonalService : IPersonalService
    {
        private readonly string _connectionString;

        public PersonalService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Personal
        public List<PersonalDto> GetAllPersonal()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Personal, Nombre, ID_Sede FROM Personal";
                return connection.Query<PersonalDto>(query).ToList();
            }
        }

        public List<PersonalDto> GetPersonalById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Personal, Nombre, ID_Sede FROM Personal WHERE ID_Personal = @ID_Personal";
                return connection.Query<PersonalDto>(query, new { ID_Personal = id }).ToList();
            }
        }

        public List<PersonalDto> GetUsuariosByIdSede(int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Personal, Nombre, ID_Sede FROM Personal WHERE ID_Sede = @ID_Sede";
                return connection.Query<PersonalDto>(query, new { ID_Sede = idSede }).ToList();
            }
        }

        public List<PersonalDto> BuscarPersonalPorNombreYPorSede(string inicioNombre, int idSede)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = @"
            SELECT ID_Personal, Nombre, ID_Sede 
            FROM Personal 
            WHERE Nombre LIKE @NombreInicio AND ID_Sede = @ID_Sede
            ORDER BY Nombre ASC";

                return connection.Query<PersonalDto>(query, new
                {
                    NombreInicio = inicioNombre + "%",
                    ID_Sede = idSede
                }).ToList();
            }
        }


        #endregion

        #region Agregar Personal
        public void AgregarPersona(PersonalDatos persona)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                if(persona.ID_Sede <= 0 )
                    throw new Exception("Debe seleccionar una sede válida.");

                var personaExistente = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Personal WHERE Nombre = @Nombre", 
                    new { persona.Nombre});

                if(personaExistente > 0 )
                    throw new Exception("Ya existe una persona con ese nombre y apellidos.");

                var sedeExiste = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede",
                    new { persona.ID_Sede });

                if (sedeExiste == 0)
                    throw new Exception("La sede especificada no existe.");

                string query = @"INSERT INTO Personal (Nombre, ID_Sede) VALUES (@Nombre, @ID_Sede)";
                connection.Execute(query, persona);
            }
        }
        #endregion

        #region Editar Personal
        public void EditarPersona(int id, PersonalDatos persona)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var personaActual = connection.QueryFirstOrDefault<PersonalDatos>(
                    @"SELECT ID_Personal, Nombre, ID_Sede FROM Personal WHERE ID_Personal = @ID_Personal", 
                    new { ID_Personal = id });

                if(personaActual == null)
                    throw new Exception("La Persona Ingresada no Existe.");

                int nuevaSede = (int)(persona.ID_Sede > 0 ? persona.ID_Sede : personaActual.ID_Sede);

                if (nuevaSede != personaActual.ID_Sede)
                {
                    var sedeExiste = connection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Sedes WHERE ID_Sede = @ID_Sede",
                        new { ID_Sede = nuevaSede });

                    if (sedeExiste == 0)
                        throw new Exception("La sede especificada no existe.");
                }

                string nuevoNombre = string.IsNullOrWhiteSpace(persona.Nombre) ? personaActual.Nombre : persona.Nombre;

                string query = @"UPDATE Personal SET Nombre = @Nombre, ID_Sede = @ID_Sede WHERE ID_Personal = @ID_Personal";

                connection.Execute(query, new
                {
                    Nombre = nuevoNombre,
                    ID_Sede = nuevaSede,
                    ID_Personal = id
                });
            }
        }
        #endregion

        #region Eliminar Personal
        public void EliminarPersonal(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Personal WHERE ID_Personal = @ID_Personal", 
                    new { ID_Personal = id});

                if (existe == 0) throw new Exception("La Persona no Existe.");

                string query = "DELETE FROM Personal WHERE ID_Personal = @ID_Personal";
                connection.Execute(query, new { ID_Personal = id });
            }
        }
        #endregion
    }
}