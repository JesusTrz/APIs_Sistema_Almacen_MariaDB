using Sistema_Almacen_MariaDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Almacen_MariaDB.Infraestructure
{
    public interface ICentroCostoService
    {
        void AgregarCentro(CenCostoDatos centro);
        void EditarCentrodeCosto(int id, CenCostoDatos centro);
        void EliminarCentro(int id);
        List<CentroCostoDto> GetAllCenCost();
        List<CentroCostoDto> GetCenCostById(int id);
    }
}
