using Common;
using Common.Helpers;
using Common.Services;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Payments
{
    #region Sample Form
    //<form method="post" action="https://secure.ogone.com/ncol/test/orderstandard.asp" id=form1 name=form1>
    //    <!-- general parameters: see General Payment Parameters -->
    //    <input type="hidden" name="PSPID" value="">
    //    <input type="hidden" name="ORDERID" value="">
    //    <input type="hidden" name="AMOUNT" value="">
    //    <input type="hidden" name="CURRENCY" value="">
    //    <input type="hidden" name="LANGUAGE" value="">
    //    <!-- optional customer details, highly recommended for fraud prevention: see General
    //    parameters and optional customer details -->
    //    <input type="hidden" name="CN" value="">
    //    <input type="hidden" name="EMAIL" value="">
    //    <input type="hidden" name="OWNERZIP" value="">
    //    <input type="hidden" name="OWNERADDRESS" value="">
    //    <input type="hidden" name="OWNERCTY" value="">
    //    <input type="hidden" name="OWNERTOWN" value="">
    //    <input type="hidden" name="OWNERTELNO" value="">
    //    <input type="hidden" name="COM" value="">
    //    <!-- check before the payment: see SHA-IN signature -->
    //    <input type="hidden" name="SHASIGN" value="">
    //    <!-- layout information: see Look & Feel of the Payment Page -->
    //    <input type="hidden" name="TITLE" value="">
    //    <input type="hidden" name="BGCOLOR" value="">
    //    <input type="hidden" name="TXTCOLOR" value="">
    //    <input type="hidden" name="TBLBGCOLOR" value="">
    //    <input type="hidden" name="TBLTXTCOLOR" value="">
    //    <input type="hidden" name="BUTTONBGCOLOR" value="">
    //        <input type="hidden" name="BUTTONTXTCOLOR" value="">
    //    <input type="hidden" name="LOGO" value="">
    //    <input type="hidden" name="FONTTYPE" value="">
    //    <!-- dynamic template page: see Look & Feel of the Payment Page -->
    //    <input type="hidden" name="TP" value="">
    //    <!-- payment methods/page specifics: see Payment method and payment page
    //    specifics -->
    //    <input type="hidden" name="PM" value="">
    //    <input type="hidden" name="BRAND" value="">
    //    <input type="hidden" name="WIN3DS" value="">
    //    <input type="hidden" name="PMLIST" value="">
    //    <input type="hidden" name="PMLISTTYPE" value="">
    //    <!-- link to your website: see Default reaction -->
    //    <input type="hidden" name="HOMEURL" value="">
    //    <input type="hidden" name="CATALOGURL" value="">
    //    <!-- post payment parameters: see Redirection depending on the payment result -->
    //    <input type="hidden" name="COMPLUS" value="">
    //    <input type="hidden" name="PARAMPLUS" value="">
    //    <!-- post payment parameters: see Direct feedback requests (Post-payment) -->
    //    <input type="hidden" name="PARAMVAR" value="">
    //    <!-- post payment redirection: see Redirection depending on the payment result -->
    //    <input type="hidden" name="ACCEPTURL" value="">
    //    <input type="hidden" name="DECLINEURL" value="">
    //    <input type="hidden" name="EXCEPTIONURL" value="">
    //    <input type="hidden" name="CANCELURL" value="">
    //    <!-- optional operation field: see Operation -->
    //    <input type="hidden" name="OPERATION" value="">
    //    <!-- optional extra login detail field: see User field -->
    //    <input type="hidden" name="USERID" value="">
    //    <!-- Alias details: see Alias Management documentation -->
    //    <input type="hidden" name="ALIAS" value="">
    //    <input type="hidden" name="ALIASUSAGE" value="">
    //    <input type="hidden" name="ALIASOPERATION" value="">
    //    <input type="submit" value="" id="submit2" name="SUBMIT2">
    //</form>
    #endregion

    public class IngenicoPaymentProvider : IPaymentProvider
    {
        // Constructors
        public IngenicoPaymentProvider()
        {
        }

        // Properties
        public string ID { get { return "Ingenico"; } }
        public PaymentHandlerType HandlerType { get { return PaymentHandlerType.Remote; } }
        public Order Order { get; set; }
        public IOrderConfiguration OrderConfiguration { get; set; }
        // Test Creds
        //public string HashSalt = "testSalt123@temp"; 

        // Live Creds
        public string HashSalt = "liveWINSalt123@Oats";


        // Methods
        public IPaymentRequest GetPaymentRequest(PaymentRequestArgs args)
        {
            var request = new POSTPaymentRequest();

            var Request = HttpContext.Current.Request;
            var customer = Exigo.OData().Customers.Where(c => c.CustomerID == Order.CustomerID).FirstOrDefault();
            var urlhelper = new UrlHelper(Request.RequestContext);
            //var encryptor = new MD5CryptoServiceProvider();

            var url = GlobalSettings.Merchants.Ingenico.Address;
            var referenceCode = Order.OrderID.ToString() + "-" + args.Attempt;
            var orderToken = Security.Encrypt(new { OrderID = Order.OrderID, CustomerID = Order.CustomerID });
            var email = customer.Email;
            var tax = Order.TaxTotal;
            var language = GlobalUtilities.GetSelectedLanguage().Replace("-", "_");
            var recipient = Order.Recipient;
            var addressDisplay = (recipient.Address2.IsEmpty()) ? recipient.Address1 : recipient.Address1 + " " + recipient.Address2;
                 

            // Format and set up our URL parameters
            var returnUrl = args.ReturnUrl;
            returnUrl += (returnUrl.Contains("?") ? "&" : "?") + "_p=" + ID;
            returnUrl += "&" + "_cid=" + Order.CustomerID;
            returnUrl += "&" + "token=" + orderToken;

            // We run our web alias logic below. If we are in a replicated site setting, we need to ensure the web alias is inserted correctly in the return urls
            var appPath = (args.RequiresWebAlias) ?  "/{0}/".FormatWith(args.WebAlias) : "/";
            var fullReturnUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, appPath, returnUrl);

            var successUrl = fullReturnUrl + "&paymentstatus=success";
            var errorUrl = fullReturnUrl + "&paymentstatus=error";
            var declineUrl = fullReturnUrl + "&paymentstatus=decline";
            var cancelUrl = fullReturnUrl + "&paymentstatus=cancel";
            var homeView = (args.RequiresWebAlias) ? urlhelper.Action("index", "home", new { webalias = args.WebAlias }) : urlhelper.Action("index", "dashboard");
            var homeUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, Request.ApplicationPath, homeView.TrimStart('/'));
            var shopView = (args.RequiresWebAlias) ? urlhelper.Action("itemlist", "shopping", new { webalias = args.WebAlias }) : urlhelper.Action("itemlist", "shopping");
            var shopUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, Request.ApplicationPath, shopView.TrimStart('/'));
            
            var shaPassPhrase = HashSalt;

            var data = new Dictionary<string, object>()
            {
                {"PSPID", "WINBV" },
                {"ORDERID", "WinExigoOrder" + Order.OrderID.ToString() },
                {"AMOUNT", (Order.Total * 100).ToString("0") },
                {"CURRENCY", CurrencyCodes.Euro },
                {"LANGUAGE", language },
                // optional customer details, highly recommended for fraud prevention
                {"CN", recipient.FirstName + " " + recipient.LastName },
                {"EMAIL", recipient.Email },
                {"OWNERADDRESS", addressDisplay },
                {"OWNERCTY", recipient.City },
                {"OWNERTELNO", recipient.Phone },
                {"OWNERZIP", recipient.Zip },
                {"COM", "Exigo Order {0}".FormatWith(Order.OrderID) },               
                //<!-- link to your website: see Default reaction -->
                {"HOMEURL", homeUrl },
                {"CATALOGURL", shopUrl },
                //<!-- post payment redirection: see Redirection depending on the payment result -->
                {"ACCEPTURL", successUrl },
                {"DECLINEURL", declineUrl },
                {"EXCEPTIONURL", errorUrl },
                {"CANCELURL", cancelUrl }
            };

            var hashedData = GetHashedData(data, shaPassPhrase);

            data.Add("SHASIGN", hashedData);

            request.Method = PaymentRequestMethod.Post;
            request.RequestUrl = url;
            request.RequestForm = FormHelper.GetSelfPostingFormHtml(url, data);

            return request;
        }

        public string GetHashedData(Dictionary<string, object> data, string shaPassPhrase)
        {
            SHA1 sha1 = SHA1.Create();
            // Build up each line one-by-one and then trim the end
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object> pair in data.OrderBy(d => d.Key).Where(d => d.Value != null && !d.Value.ToString().IsEmpty()))
            {
                builder.Append(pair.Key).Append("=").Append(pair.Value).Append(shaPassPhrase);
            }
            string input = builder.ToString();
            
            byte[] bytes = new ASCIIEncoding().GetBytes(input);
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 20; i++)
            {
                string temp = hash[i].ToString("X2");
                if (temp.Length == 1)
                {
                    temp = "0" + temp;
                }
                result.Append(temp);
            }

            return result.ToString();
        }

        public IPaymentResponse EndTransaction(object args)
        {
            // Parse the information based on how this bank wants you to handle their response

            return new PaymentResponse()
            {
                Status = PaymentResultStatus.Success,
                AuthorizationCode = "123456"
            };
        }
        public PaymentResult GetResponse(Uri uri, NameValueCollection form)
        {
            var result = new PaymentResult()
            {
                Uri = uri,
                Form = form
            };

            var querystring = new Dictionary<string, string>();
            foreach (var query in uri.Query.TrimStart('?').Split('&'))
            {
                var queryPart = query.Split('=');
                querystring.Add(queryPart[0], queryPart[1]);
            }

            //var statusCode = querystring["paymentstatus"];
            var statusCode = querystring["status"];
            result.Token = (querystring.ContainsKey("token")) ? querystring["token"] : "";
            //var transactionState = querystring["lapResponseCode"];
            //result.Attempt = Convert.ToInt32(querystring["referenceCode"].Split('-')[1]);
            //result.TransactionRecord = querystring["reference_pol"];
            result.PaymentType = querystring["PM"];
            result.OrderID = Convert.ToInt32(querystring["orderID"].Replace("WinExigoOrder",""));
            //result.CustomerID = Convert.ToInt32(querystring["_cid"]);
            result.ProviderType = querystring["_p"];
            //result.QueryString = querystring;
            //result.ErrorMessage = GetResponseMessage(statusCode, transactionState);
            result.Status = (statusCode == "success") ? PaymentStatus.Success : PaymentStatus.Fail;

            return result;
        }

        // Helpers
        public string FormatReturnURL(string scheme, string host, int port, string appPath, string returnUrl)
        {
            StringBuilder returnURL = new StringBuilder();
            returnURL.Append(scheme);
            returnURL.Append("://");
            returnURL.Append(host);
            returnURL.Append(":");
            returnURL.Append(port);
            returnURL.Append(appPath);
            //returnURL.Append("/");
            returnURL.Append(returnUrl);
            return returnURL.ToString();
        }
        private string GetResponseMessage(int statusCode, string transactionState)
        {
            switch (statusCode)
            {
                case 1:
                    return "Response Unknown";
                case 4:
                    return "Transaction rejected by payment network";
                case 5:
                    return "Transaction has been declined by the bank";
                case 6:
                    return "Insufficient funds";
                case 7:
                    return "Invalid Card";
                case 8:
                    switch (transactionState)
                    {
                        case "CONTACT_THE_ENTITY":
                            return "Please contact your financial entity";
                        case "BANK_ACCOUNT_ACTIVATION_ERROR":
                            return "Automatic debit not allowed";
                        case "BANK_ACCOUNT_NOT_AUTHORIZED_FOR_AUTOMATIC_DEBIT":
                            return "Automatic debit not allowed";
                        case "INVALID_AGENCY_BANK_ACCOUNT":
                            return "Automatic debit not allowed";
                        case "INVALID_BANK_ACCOUNT":
                            return "Automatic debit not allowed";
                        case "INVALID_BANK":
                            return "Automatic debit not allowed";
                        default:
                            return "Response Code Unknown";
                    }
                case 9:
                    return "Expired card";
                case 10:
                    return "Restricted card";
                case 12:
                    return "Date of expiration or security code is invalid";
                case 13:
                    return "Retry the transaction";
                case 14:
                    return "Transaction invalid";
                case 15:
                    return "Transaction is pending approval";
                case 17:
                    return "Value exceeds maximum allowed by this entity";
                case 19:
                    return "Transaction abandoned by the payer";
                case 20:
                    return "Transaction expired";
                case 22:
                    return "Card is not authorized for internet purchases";
                case 23:
                    return "Transaction has been rejected by the anti-fraud module";
                case 25:
                    switch (transactionState)
                    {
                        case "PENDING_TRANSACTION_CONFIRMATION":
                            return "Receipt of payment generated. Pending payment";
                        case "PENDING_PAYMENT_IN_ENTITY":
                            return "Receipt of payment generated. Pending payment";
                        case "PENDING_NOTIFYING_ENTITY":
                            return "Receipt of payment generated. Pending payment";
                        default:
                            return "Response Code Unknown";
                    }
                case 26:
                    return "Receipt of payment generated. Pending payment";
                case 29:
                    return "Pending being sent to finacial entity";
                case 9994:
                    return "Pending confirmation from PSE";
                case 9995:
                    return "Digital certificate not found";
                case 9996:
                    switch (transactionState)
                    {
                        case "BANK_UNREACHABLE":
                            return "Error trying to communicate with the bank";
                        case "PAYMENT_NETWORK_NO_CONNECTION":
                            return "Unable to communicate with the financial institution";
                        case "PAYMENT_NETWORK_NO_RESPONSE":
                            return "No response from the financial institution";
                        default:
                            return "Response Code Unknown";
                    }
                case 9997:
                    return "Error communicating with the financial institution";
                case 9998:
                    switch (transactionState)
                    {
                        case "NOT_ACCEPTED_TRANSACTION":
                            return "Transaction not permitted to cardholder";
                        case "PENDING_TRANSACTION_TRANSMISSION":
                            return "Not permitted transaction";
                        default:
                            return "Response Code Unknown";
                    }
                case 9999:
                    return "Internal error";
                default:
                    return "Response Code Unknown";
            }
        }
    }
}