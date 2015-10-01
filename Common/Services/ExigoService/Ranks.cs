using System;
using Common.Kendo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dapper;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Rank> GetRanks()
        {
            var context = Exigo.OData();
            var apiRanks = context.Ranks
                .OrderBy(c => c.RankID);

            var ranks = new List<Rank>();
            foreach (var apiRank in apiRanks)
            {
                ranks.Add((Rank)apiRank);
            }
            return ranks;
        }
        public async static Task<IEnumerable<Rank>> GetRanksAsync()
        {
            return await Task.Run(() => GetRanks());
        }

        public static Rank GetRank(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID == rankID)
                .FirstOrDefault();
        }

        public static IEnumerable<Rank> GetNextRanks(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID > rankID)
                .OrderBy(c => c.RankID)
                .ToList();
        }
        public static Rank GetNextRank(int rankID)
        {
            return GetNextRanks(rankID).FirstOrDefault();
        }

        public static IEnumerable<Rank> GetPreviousRanks(int rankID)
        {
            return GetRanks()
                .Where(c => c.RankID < rankID)
                .OrderByDescending(c => c.RankID)
                .ToList();
        }
        public static Rank GetPreviousRank(int rankID)
        {
            return GetPreviousRanks(rankID).FirstOrDefault();
        }

        public static CustomerRankCollection GetCustomerRanks(GetCustomerRanksRequest request)
        {
            var result = new CustomerRankCollection();
            var context = Exigo.OData();

            // Get the highest rank achieved in the customer table
            var highestRankAchieved = context.Customers
                .Where(c => c.CustomerID == request.CustomerID)
                .Select(c => new
                {
                    c.Rank
                }).FirstOrDefault();
            if (highestRankAchieved != null)
            {
                result.HighestPaidRankInAnyPeriod = (Rank)highestRankAchieved.Rank;
            }

            // Get the period ranks
            var query = context.PeriodVolumes
                .Where(c => c.CustomerID == request.CustomerID)
                .Where(c => c.PeriodTypeID == request.PeriodTypeID);

            if (request.PeriodID != null)
            {
                query = query.Where(c => c.PeriodID == (int)request.PeriodID);
            }
            else
            {
                query = query.Where(c => c.Period.IsCurrentPeriod);
            }

            var periodRanks = query.Select(c => new
            {
                c.Rank,
                c.PaidRank
            }).FirstOrDefault();

            if (periodRanks != null)
            {
                if (periodRanks.PaidRank != null)
                {
                    result.CurrentPeriodRank = (Rank)periodRanks.PaidRank;
                }
                if (periodRanks.Rank != null)
                {
                    result.HighestPaidRankUpToPeriod = (Rank)periodRanks.Rank;
                }
            }

            return result;
        }

        public static IEnumerable<CustomerRankScore> GetDownlineUpcomingPromotions(GetDownlineUpcomingPromotionsRequest request)
        {
            var results = new List<CustomerRankScore>();

            var context = Exigo.Sql();
            context.Open();

            results = context.Query<CustomerRankScore, Rank, CustomerRankScore>(@"
                SELECT c.CustomerID, 
                        c.FirstName, 
                        c.LastName, 
                        c.Company,
                        RankScore = Score, 
                        TotalScore = Score * prso.PaidRankID,
                        RankID = prso.PaidRankID,
                        r.RankDescription
                FROM   (SELECT prs.PeriodTypeID, 
                                prs.PeriodID, 
                                uld.CustomerID, 
                                PaidRankID=Min(PaidRankID) 
                        FROM   UniLevelDownline uld 
                                INNER JOIN PeriodRankScores prs 
                                        ON prs.PeriodTypeID = @periodtypeid
                                            AND prs.PeriodID = (SELECT periodid 
                                                                FROM   periods 
                                                                WHERE  PeriodTypeID = @periodtypeid
                                                                        AND Getdate() >= StartDate 
                                                                        AND Getdate() < EndDate + 1 
                                                                ) 
                                            AND prs.CustomerID = uld.CustomerID 
                        WHERE  uld.DownlineCustomerID = @downlinecustomerid
                                AND prs.Score < 100 
                                " + ((request.RankID != null) ? "AND PaidRankID = @rankid" : "") + @"
                        GROUP  BY prs.PeriodTypeID, 
                                    prs.PeriodID, 
                                    uld.CustomerID) i 
                        INNER JOIN Customers c 
                                ON i.CustomerID = c.CustomerID 
                        INNER JOIN PeriodRankScores prso 
                                ON i.PeriodTypeID = prso.PeriodTypeID 
                                    AND i.PeriodID = prso.PeriodID 
                                    AND i.CustomerID = prso.CustomerID 
                                    AND i.PaidRankID = prso.PaidRankID 
                        INNER JOIN Ranks r 
                                ON prso.PaidRankID = r.RankID 
                ORDER BY 
                    Score * prso.PaidRankID DESC,
                    c.CreatedDate ASC
            
                OFFSET @skip ROWS
                FETCH NEXT @take ROWS ONLY
            
            ", (customer, rank) => {
                    customer.Rank = rank;
                    return customer;
                }, 
                new
                {
                    downlinecustomerid = request.DownlineCustomerID,
                    periodtypeid       = request.PeriodTypeID,
                    rankid             = request.RankID,
                    skip               = request.Skip,
                    take               = request.Take
                }, splitOn: "RankID").ToList();

            context.Close();

            foreach (var result in results)
            {
                yield return result;
            }
        }
    }
}