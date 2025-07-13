using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IArticulosService
    {
        void AgregarArticulos(ArticulosDatos articulo);
        void EditarArticulo(int id, ArticulosDatos articulo);
        void EliminarArticulo(int id);
        List<ArticuloDto> GetAllArticulos();
        List<ArticuloDto> GetArticulosById(int id);
    }
}
