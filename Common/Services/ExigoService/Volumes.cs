using Common.Api.ExigoOData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static VolumeCollection GetCustomerVolumes(GetCustomerVolumesRequest request)
        {
        
            var requestedSpecificVolumes = (request.VolumeIDs != null && request.VolumeIDs.Length > 0);

            var baseQuery = Exigo.OData().PeriodVolumes.Expand("Period,Rank,PaidRank");
            var query = baseQuery
                .Where(c => c.CustomerID == request.CustomerID)
                .Where(c => c.PeriodTypeID == request.PeriodTypeID);

            // Determine which period ID to use
            if (request.PeriodID != null)
            {
                query = query.Where(c => c.PeriodID == (int)request.PeriodID);
            }
            else
            {
                query = query.Where(c => c.Period.IsCurrentPeriod);
            }

            var data = query.FirstOrDefault();

            //2015-07-25
            //Ivan S
            //#68437
            //I worked with Travis and I implemented a code he modified to fix the dashboard error changing
            //the way the dashboards are loading the volume data
            var result = (VolumeCollection)data;

            return result;
        
            
            
          
            
            //var requestedSpecificVolumes = (request.VolumeIDs != null && request.VolumeIDs.Length > 0);

            //var baseQuery = Exigo.OData().PeriodVolumes;
            //var query = baseQuery
            //    .Where(c => c.CustomerID == request.CustomerID)
            //    .Where(c => c.PeriodTypeID == request.PeriodTypeID);

            //// Determine which period ID to use
            //if (request.PeriodID != null)
            //{
            //    query = query.Where(c => c.PeriodID == (int)request.PeriodID);
            //}
            //else
            //{
            //    query = query.Where(c => c.Period.IsCurrentPeriod);
            //}

            //PeriodVolume data;

            //if (!requestedSpecificVolumes)
            //{
            //    data = query.Select(c => new Common.Api.ExigoOData.PeriodVolume()
            //    {
            //        CustomerID   = c.CustomerID,
            //        ModifiedDate = c.ModifiedDate,
            //        PaidRankID   = c.PaidRankID,
            //        PaidRank     = c.PaidRank,
            //        RankID       = c.RankID,
            //        Rank         = c.Rank,
            //        PeriodID     = c.PeriodID,
            //        Period       = c.Period,
            //        PeriodTypeID = c.PeriodTypeID                    
            //    }).FirstOrDefault();
            //}
            //else
            //{
            //    var volumes = new List<string>();
            //    foreach (var id in request.VolumeIDs)
            //    {
            //        volumes.Add("Volume" + id);
            //    }
            //    var select = string.Format("new({0},Period,Rank,PaidRank)", string.Join(",", volumes));
            //    var finalQuery = query.Select(select);

            //    var url = finalQuery.ToString();
            //    data = Exigo.OData().Execute<Common.Api.ExigoOData.PeriodVolume>(new Uri(url)).FirstOrDefault();
            //}

            //return (VolumeCollection)data;
        }
    }
}