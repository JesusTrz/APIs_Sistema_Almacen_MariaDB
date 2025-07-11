using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using static Mysqlx.Crud.Order.Types;

namespace Sistema_Almacen_MariaDB.Service
{
    public class ProveedoresService : IProveedoresService
    {
        private readonly string _connectionString;

        public ProveedoresService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }

        #region Obtener Proveedores
        public List<ProveedoresDto> GetAllProveedores()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Proveedores, RFC, Razon_Social, Direccion, Telefono, Email FROM Proveedores";
                return connection.Query<ProveedoresDto>(query).ToList();
            }
        }

        public List<ProveedoresDto> GetProveedoresById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT ID_Proveedores, RFC, Razon_Social, Direccion, Telefono, Email FROM Proveedores WHERE ID_Proveedores = @ID_Proveedores";
                return connection.Query<ProveedoresDto>(query, new { ID_Proveedores = id}).ToList();
            }
        }
        #endregion

        #region Agregar Nuevo Proveedor
        public void AgregarProveedor(ProveedoresDatos proveedores)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Validar si ya existe un proveedor con el mismo RFC
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Proveedores WHERE RFC = @RFC",
                    new { RFC = proveedores.RFC });

                if (existe > 0)
                    throw new Exception("Ya existe un proveedor con ese RFC.");

                // Insertar nuevo proveedor
                string query = @"INSERT INTO Proveedores (RFC, Razon_Social, Direccion, Telefono, Email) 
                         VALUES (@RFC, @Razon_Social, @Direccion, @Telefono, @Email)";

                connection.Execute(query, new
                {
                    RFC = proveedores.RFC,
                    Razon_Social = proveedores.Razon_Social,
                    Direccion = proveedores.Direccion,
                    Telefono = proveedores.Telefono,
                    Email = proveedores.Email
                });
            }
        }
        #endregion

        #region Editar Proveedor
        public void EditarProveedor(int id, ProveedoresDatos proveedores)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Obtener proveedor actual
                var proveedorActual = connection.QueryFirstOrDefault<ProveedoresDto>(
                    "SELECT ID_Proveedores, RFC, Razon_Social, Direccion, Telefono, Email FROM Proveedores WHERE ID_Proveedores = @ID",
                    new { ID = id });

                if (proveedorActual == null)
                    throw new Exception("El proveedor no existe.");

                // Tomar valores nuevos o conservar los actuales
                var nuevoRFC = string.IsNullOrWhiteSpace(proveedores.RFC) ? proveedorActual.RFC : proveedores.RFC;
                var nuevaRazonSocial = string.IsNullOrWhiteSpace(proveedores.Razon_Social) ? proveedorActual.Razon_Social : proveedores.Razon_Social;
                var nuevaDireccion = string.IsNullOrWhiteSpace(proveedores.Direccion) ? proveedorActual.Direccion : proveedores.Direccion;
                var nuevoTelefono = string.IsNullOrWhiteSpace(proveedores.Telefono) ? proveedorActual.Telefono : proveedores.Telefono;
                var nuevoEmail = string.IsNullOrWhiteSpace(proveedores.Email) ? proveedorActual.Email : proveedores.Email;

                // Actualizar datos
                string query = @"
            UPDATE Proveedores
            SET RFC = @RFC,
                Razon_Social = @Razon_Social,
                Direccion = @Direccion,
                Telefono = @Telefono,
                Email = @Email
            WHERE ID_Proveedores = @ID";

                connection.Execute(query, new
                {
                    RFC = nuevoRFC,
                    Razon_Social = nuevaRazonSocial,
                    Direccion = nuevaDireccion,
                    Telefono = nuevoTelefono,
                    Email = nuevoEmail,
                    ID = id
                });
            }
        }
        #endregion

        #region Eliminar Proveedor
        public void EliminarProveedor(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var enUso = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Entradas WHERE ID_Proveedores = @ID_Proveedores",
                    new { ID_Proveedores = id });
                if( enUso > 0)
                    throw new Exception("No se puede eliminar el Proveedor porque está en uso.");
                // Verificar existencia
                var existe = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Proveedores WHERE ID_Proveedores = @ID_Proveedores",
                    new { ID_Proveedores = id });

                if (existe == 0)
                    throw new Exception("El proveedor no existe.");

                // Aquí puedes agregar validación si está en uso en otra tabla, si aplica.
                // Ejemplo: Validar si tiene productos relacionados (dependiendo de tu modelo de datos).

                // Eliminar
                string query = "DELETE FROM Proveedores WHERE ID_Proveedores = @ID_Proveedores";
                connection.Execute(query, new { ID_Proveedores = id });
            }
        }
        #endregion

    }
}