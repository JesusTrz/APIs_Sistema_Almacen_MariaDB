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
        void EliminarArticulodeInventario(int id);
        bool EliminarTodosArticulosInventario(int idSede);
        List<InventarioArticulos> GetInventarioPorSede(int idSede);
        List<InventarioArticulos> ObtenerInventarioPorSede(int idSede);
        bool ReiniciarCostoSaldo(int idSede);
        bool ReiniciarInventario(int idSede);
        bool ReiniciarInventarioPorSede(int idSede);
        List<InventarioArticulos> VerificarStockAlto(int idSede);
        List<InventarioArticulos> VerificarStockBajo(int idSede);
    }
}
