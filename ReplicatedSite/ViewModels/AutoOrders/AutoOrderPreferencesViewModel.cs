using ExigoService;
using System.Collections.Generic;


namespace ReplicatedSite.ViewModels
{
    public class AutoOrderPreferencesViewModel
    {
        public AutoOrderPreferencesViewModel()
        {
            this.AutoOrders = new List<AutoOrder>();
        }

        public List<AutoOrder> AutoOrders { get; set; }
    }
}