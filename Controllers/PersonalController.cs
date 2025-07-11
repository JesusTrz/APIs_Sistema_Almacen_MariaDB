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
    public class PersonalController : ApiController
    {
        private readonly IPersonalService _personalService;

        public PersonalController()
        {
            _personalService = new PersonalService();
        }

        #region Obtener Personal
        [HttpGet]
        [Route("api/personal/all")]

        public IHttpActionResult GetAllPersonal()
        {
            try
            {
                var personal = _personalService.GetAllPersonal();
                return Ok(personal);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/personal/id")]

        public IHttpActionResult GetPersonalById(int id)
        {
            try
            {
                var persona = _personalService.GetPersonalById(id);
                if(persona == null)
                    return NotFound();
                return Ok(persona);
            }
            catch (Exception ex) 
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/personal/sede/id")]

        public IHttpActionResult GetPersonalByIdSede(int idSede)
        {
            try
            {
                var persona = _personalService.GetUsuariosByIdSede(idSede);
                if (persona == null)
                    return NotFound();
                return Ok(persona);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/personal/search/by_name")]
        public IHttpActionResult BuscarPersonal(string inicioNombre, int idSede)
        {
            try
            {
                var resultado = _personalService.BuscarPersonalPorNombreYPorSede(inicioNombre, idSede);
                return Ok(resultado); // 👈 Esto es una lista
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        #endregion

        #region Agregar Personal
        [HttpPost]
        [Route("api/personal/add")]

        public IHttpActionResult AgregarPersona(PersonalDatos persona)
        {
            if(persona == null || string.IsNullOrWhiteSpace(persona.Nombre)
                               || string.IsNullOrWhiteSpace(persona.Apellidos)
                               || persona.ID_Sede <= 0)
                return BadRequest("La Persona no debe contener datos Vacios!");
            try
            {
                _personalService.AgregarPersona(persona);
                return Ok("Persona Agregada con Exito!");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Editar Personal
        [HttpPut]
        [Route("api/personal/{id}")]

        public IHttpActionResult EditarPersona(int id, PersonalDatos persona)
        {
            try
            {
                _personalService.EditarPersona(id, persona);
                return Ok("Persona Actualizada Correctamente!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Eliminar Personal
        [HttpDelete]
        [Route("api/personal/{id}")]

        public IHttpActionResult EliminarPersonal(int id)
        {
            try
            {
                _personalService.EliminarPersonal(id);
                return Ok("Persona Eliminada con Exito!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
