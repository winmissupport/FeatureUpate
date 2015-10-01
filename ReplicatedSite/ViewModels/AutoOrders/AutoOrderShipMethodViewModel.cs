using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class AutoOrderShipMethodViewModel
    {
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public ShipMethod SelectedShipMethod { get; set; }
        public int AutoorderID { get; set; }

    }
}