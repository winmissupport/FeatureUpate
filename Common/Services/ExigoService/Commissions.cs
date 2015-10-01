using Common;
using Common.Api.ExigoWebService;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        // Direct Deposit 
        public static BankAccount GetDirectDeposit(int customerID)
        {
            var account = new BankAccount();

            try
            {
                var context = Exigo.WebService();

                var result = context.GetAccountDirectDeposit(new GetAccountDirectDepositRequest
                {
                    CustomerID = customerID
                });

                if (result.Result.Status == ResultStatus.Success)
                {
                    account.AccountNumber = result.BankAccountNumberDisplay;
                    account.NameOnAccount = result.NameOnAccount;
                    account.BankName = result.BankName;
                    account.RoutingNumber = result.BankRoutingNumber;

                    account.BillingAddress.Address1 = result.BankAddress;
                    account.BillingAddress.City = result.BankCity;
                    account.BillingAddress.State = result.BankState;
                    account.BillingAddress.Country = result.BankCountry;
                    account.BillingAddress.Zip = result.BankZip;
                }
                // We are seperating out swiftCode/Iban so in order to keep everything else the same we are simply pulling back the account routing number, and account number and setting them to their proper fields.

                if (account.AccountNumber.IsNullOrEmpty())
                {
                    account.AccountNumber = result.Iban;
                    account.RoutingNumber = result.SwiftCode;
                }
            }
            catch
            {

            }

            return account;
        }
        public static bool SetDirectDeposit(int customerID, BankAccount account, MarketName CurrentMarket)
        {
            try
            {
                var context = Exigo.WebService();
                var request = new SetAccountDirectDepositRequest{
                       CustomerID = customerID,
                        NameOnAccount = account.NameOnAccount,
                        BankName = account.BankName,
                        
                        DepositAccountType = DepositAccountType.Checking,

                        BankAddress = account.BillingAddress.Address1,
                        BankCity = account.BillingAddress.City,
                        BankState = account.BillingAddress.State,
                        BankCountry = account.BillingAddress.Country,
                        BankZip = account.BillingAddress.Zip
                        
                };
                if (CurrentMarket == MarketName.UnitedStates)
                {
                    request.BankRoutingNumber = account.RoutingNumber;
                    request.BankAccountNumber = account.AccountNumber;
                }
                // We are seperating out swiftCode/Iban so in order to keep everything else the same we are simply pulling back the account routing number, and account number and setting them to their proper fields.
                else
                {
                    request.SwiftCode = account.RoutingNumber;
                    request.Iban = account.AccountNumber;
                }
                
                var result = context.SetAccountDirectDeposit(request);

                Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest 
                {
                    CustomerID = customerID,
                    PayableType = PayableType.DirectDeposit
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        // Commission Data
        public static IEnumerable<ICommission> GetCommissionList(int customerID)
        {
            // Historical Commissions
            var historicalCommissions = GetHistoricalCommissionList(customerID);
            foreach (var commission in historicalCommissions)
            {
                yield return commission;
            }
        }
        public static IEnumerable<ICommission> GetHistoricalCommissionList(int customerID)
        {
            // Historical Commissions
            var commissions = Exigo.OData().Commissions.Expand("CommissionRun/Period").Expand("PaidRank")
                .Where(c => c.CustomerID == customerID) 
                .OrderByDescending(c => c.CommissionRunID);
            if (commissions != null)
            {
                foreach (var commission in commissions)
                {
                    yield return (HistoricalCommission)commission;
                }
            }
        }

        public static IEnumerable<ICommission> GetCommissionPeriodList(int customerID)
        {
            // Historical Commissions
            var commissions = Exigo.OData().Commissions.Expand("CommissionRun/Period")
                .Where(c => c.CustomerID == customerID)
                .OrderByDescending(c => c.CommissionRunID);

            if (commissions != null)
            {
                foreach (var commission in commissions)
                {
                    yield return (HistoricalCommission)commission;
                }
            }
        }

        public static HistoricalCommission GetCustomerHistoricalCommission(int customerID, int commissionRunID)
        {
            // Get the commission record
            var commission = Exigo.OData().Commissions.Expand("CommissionRun/Period").Expand("PaidRank")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.CommissionRunID == commissionRunID)
                .FirstOrDefault();
            if (commission == null) return null;
            var result = (HistoricalCommission)commission;


            // Get the volumes
            result.Volumes = GetCustomerVolumes(new GetCustomerVolumesRequest
            {
                CustomerID = customerID,
                PeriodID = result.Period.PeriodID,
                PeriodTypeID = result.Period.PeriodTypeID,
                VolumeIDs = new int[] { 3, 6, 7, 9 }
            });

            return result;
        }
        public static IEnumerable<RealTimeCommission> GetCustomerRealTimeCommissions(GetCustomerRealTimeCommissionsRequest request)
        {
             
            var results = new List<RealTimeCommission>();


            // Get the commission record
            var realtimeresponse = Exigo.WebService().GetRealTimeCommissions(new Common.Api.ExigoWebService.GetRealTimeCommissionsRequest
            {
                CustomerID = request.CustomerID
            });
            if (realtimeresponse.Commissions.Length == 0) return results;


            // Get the unique periods for each of the commission results
            var periods = new List<Period>();
            var periodRequests = new List<GetPeriodsRequest>();
            foreach (var commissionResponse in realtimeresponse.Commissions)
            {
                var periodID = commissionResponse.PeriodID;
                var periodTypeID = commissionResponse.PeriodType;

                var req = periodRequests.Where(c => c.PeriodTypeID == periodTypeID).FirstOrDefault();
                if (req == null)
                {
                    periodRequests.Add(new GetPeriodsRequest()
                    {
                        PeriodTypeID = periodTypeID,
                        PeriodIDs = new int[] { periodID }
                    });
                }
                else
                {
                    var ids = req.PeriodIDs.ToList();
                    ids.Add(periodID);
                    req.PeriodIDs = ids.Distinct().ToArray();
                }
            }
            foreach (var req in periodRequests)
            {
                var responses = GetPeriods(req);
                foreach (var response in responses)
                {
                    periods.Add(response);
                }
            }


            // Get the volumes for each unique period
            var volumeCollections = new List<VolumeCollection>();
            foreach (var period in periods)
            {
                volumeCollections.Add(GetCustomerVolumes(new GetCustomerVolumesRequest
                {
                    CustomerID   = request.CustomerID,
                    PeriodID     = period.PeriodID,
                    PeriodTypeID = period.PeriodTypeID,
                    VolumeIDs    = request.VolumeIDs
                }));
            }

            // Process each commission response 
            try
            {
                foreach (var commission in realtimeresponse.Commissions)
                {
                    var typedCommission = (RealTimeCommission)commission;

                    typedCommission.Period = periods
                        .Where(c => c.PeriodTypeID == commission.PeriodType)
                        .Where(c => c.PeriodID == commission.PeriodID)
                        .FirstOrDefault();

                    typedCommission.Volumes = volumeCollections
                        .Where(c => c.Period.PeriodTypeID == typedCommission.Period.PeriodTypeID)
                        .Where(c => c.Period.PeriodID == typedCommission.Period.PeriodID)
                        .FirstOrDefault();

                    typedCommission.PaidRank = typedCommission.Volumes.PayableAsRank;

                    results.Add(typedCommission);
                }

                return results.OrderByDescending(c => c.Period.StartDate);
            }
            catch { return results; }
        }
    }
}