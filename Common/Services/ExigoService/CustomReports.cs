using Common;
using Common.Api.ExigoWebService;
using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static UnilevelPositionNode GetUniLevelPosition(int customerID)
        {
            // Make our request
            var request        = new Common.Api.ExigoWebService.GetCustomReportRequest();
            request.ReportID   = 8;
            request.Parameters = new List<ParameterRequest>
            {
                new ParameterRequest { ParameterName = "CustomerID", Value = customerID }
            }.ToArray();

            // Get the data
            var response = Exigo.WebService().GetCustomReport(request);

            // Populate our model
            var result = new UnilevelPositionNode();

            var row           = response.ReportData.Tables[0].Rows[0];
            result.CustomerID = Convert.ToInt32(row["CustomerID"]);
            result.Left       = Convert.ToInt32(row["Lft"]);
            result.Right      = Convert.ToInt32(row["Rgt"]);

            // Return the result
            return result;
        }
    }
}