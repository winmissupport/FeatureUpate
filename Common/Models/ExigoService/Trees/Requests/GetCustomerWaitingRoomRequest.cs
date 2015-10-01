using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExigoService
{
    public class GetCustomerWaitingRoomRequest
    {
        public int EnrollerID { get; set; }
        public int GracePeriod { get; set; }
    }
}