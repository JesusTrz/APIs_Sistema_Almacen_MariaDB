using MySql.Data.MySqlClient;
using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Models;
using Sistema_Almacen_MariaDB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers
{
    public class UsuarioController : ApiController
    {
        private readonly IUsuariosService _usuariosService;

        public UsuarioController()
        {
            _usuariosService = new UsuariosService();
        }

        #region Obtener Sedes
        [HttpGet]
        [Route("api/usuarios/all")]
        public IHttpActionResult GetAllSedes()
        {
            try
            {
                var users = _usuariosService.GetAllUsuarios();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }
        #endregion

        #region Obtener Usuarios por Id de Sede
        [HttpGet]
        [Route("api/usuarios")]
        public IHttpActionResult GetUsuarioBySedeId(int idSede)
        {
            try
            {
                var users = _usuariosService.GetUsuarioBySedeId(idSede);
                if (users == null)
                    return NotFound();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Crear Nuevos Usuarios
        [HttpPost]
        [Route("api/crate/users")]

        public IHttpActionResult CrearUsuarios(UsuariosDatos users)
        {
            if (users == null || string.IsNullOrWhiteSpace(users.Nombre_Usuario) || string.IsNullOrWhiteSpace(users.Contrasenia))
                return BadRequest("El Nombre de Usuario y Contraseña es Obligatorio.");

            try
            {
                _usuariosService.CrearUsuarios(users);
                return Ok("Usuario Creado Correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Login de Usuarios

        [HttpPost]
        [Route("api/usuarios/login")]

        public IHttpActionResult Login(LoginUser login)
        {
            try
            {
                var usuario = _usuariosService.LoginUsuarios(login.Nombre_Usuario, login.Contrasenia, login.ID_Sede);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Cambiar Contraseña
        [HttpPut]
        [Route("api/usuarios/cambiar/contrasenia")]

        public IHttpActionResult CambiarContrasenia(int idUsuario, CambioContrasenia dto)
        {
            try
            {
                _usuariosService.CambiarContrasenia(idUsuario, dto.ViejaContrasenia, dto.NuevaContrasenia);
                return Ok("Contraseña actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
