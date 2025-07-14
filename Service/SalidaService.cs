using Sistema_Almacen_MariaDB.Infraestructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sistema_Almacen_MariaDB.Service
{
    public class SalidaService : ISalidaService
    {
        private readonly string _connectionString;

        public SalidaService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MariaDbConnection"].ConnectionString;
        }
    }
}