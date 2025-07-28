using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface IInventarioService
    {
        bool ActualizarStockArticulo(StockEntrada entrada);
        void AgregarArticuloaInventario(AgregarArticuloaInventario invArt);
        void EditarArticuloInventario(int idInv, AgregarArticuloaInventario invArt);
        List<InventarioArticulos> GetInventarioPorSede(int idSede);
        List<ExpandoObject> ObtenerInventarioFiltrado(InventarioFiltro filtros);
    }
}
