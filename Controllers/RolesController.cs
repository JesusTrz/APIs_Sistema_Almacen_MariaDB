using Sistema_Almacen_MariaDB.Infraestructure;
using Sistema_Almacen_MariaDB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sistema_Almacen_MariaDB.Controllers
{
    public class RolesController : ApiController
    {
        private readonly IRolesService _rolesService;

        public RolesController()
        {
            _rolesService = new RolesService();
        }

        [HttpGet]
        [Route("api/roles/all")]

        public IHttpActionResult GetAllRoles()
        {
            try
            {
                var roles = _rolesService.GetAllRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/roles/{id}")]

        public IHttpActionResult GetRolesById(int id)
        {
            try
            {
                var roles = _rolesService.GetRolesById(id);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
