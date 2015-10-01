using Common;
using Common.Api.ExigoWebService;
using Common.Helpers;
using Common.Services;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Payments
{
    #region Documentation and Notes

    // KEY TERMS AND DEFINITIONS:
    //  Payment Merchant - Exigo has established Merchants to whom we can directly submit payment data from a form without redirecting the customer from our site to a 2rd Party site
    //  3rd Party Payment Provider - a Payment Merchant where a customer is redirected to an external site to complete their payment and then returned to our site when their transaction is complete
    //  Referrer URL / Referring URL: The page from which the form is submitted to the 3rd Party Payment Provider and from which the customer is redirected to the external site. The domain for these pages must be added to the list of trusted sites in the Payment Provider Account Settings. Referring pages must utilize SSL protocols or they will not be recognized by the Payment Provider - See: http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/security-pre-payment-check#referrer
    //  Return URL: Page(s) to which response will be returned depending on the transaction feedback that can be included in the Payment Response. See: http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/transaction-feedback#basicredirection
    //  SHA-IN and SHA-OUT phrases: Signatures for encrypting / decrypting request and for encrypting / decrypting response on both sides. See: http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/security-pre-payment-check#shainsignature
    // - Alan C, 25 June 2015

    // NOTE:
    //  For this site we only have one third-party Payment Provider, Ingenico, which was formerly known as Ogone
    //  Each Payment Provider Model inherits the base properties of IPaymentProvider interface to ensure we have the base structure and properties of the Payment Request, Payment Response, and POST Payment Response, which is how we submit the data from the form securely to the third party site
    // - Alan C, 25 June 2015

    // REFERENCE:
    //  For help, See E-Commerce Integration guide at: http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/introduction
    //  E-Commerce Integration for Ingenico means that we are posting data to the Payment Provider Redirect Site where the customer will complete their payment and then be returned to our site,
    //      rather than gathering all payment data on our site, submitting to the Payment Provider, and receiving transaction feedback without the customer leaving our site
    // - Alan C, 25 June 2015

    // Payment redirect cannot be tested locally due to Payment Provider requirement that the 

    #endregion

    #region External Configurations

    // Settings for the Test Account and Production Account may be configured acccording to Documentation at http://payment-services.ingenico.com/int/en/ogone/support/login
    // Since we have multiple Carts and Account pages that use the Payment redirect for iDEAL, account settings must be configured for 'Redirection Based on Transaction Feedback' to determine the ReturnURL
    // See: http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/transaction-feedback#SC_7_2
    // - Alan C, 25 June 2015

    #endregion

    #region Outside Dependencies

    //   Common/Settings/Settings.cs
    //      - GlobalSettings.Merchants.Ingenico.Address - Update this reference to the Test URL for publishing to UAT, but ensure it is switched back to Production URL before publishing Live
    //   Common/Payments/Providers/Interfaces
    //      - IPaymentProvider.cs - Interface for all PaymentProvider Models
    //   Common/Payments/Models/
    //      - POSTPaymentRequest.cs
    //      - PaymentRequest.cs
    //      - PaymentResponse.cs
    // - Alan C, 25 June 2015

    #endregion

    #region Sample Form

    // Demo Form from http://payment-services.ingenico.com/int/en/ogone/support/guides/integration%20guides/e-commerce/link-your-website-to-the-payment-page#formparameters - Alan C - June 25 2015

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

    // Establish the Model for the Payment Redirect Provider and Payment Redirect Form 

    public class IngenicoPaymentProvider : IPaymentProvider
    {
        #region Constructors

        // We use an Instance Constructor here to initialize our Payment Provider model and provide access to our Controllers that will execute our Redirect / submit values assigned
        // We do not require any Static Constructors or Private Constructors at this time since we are dealing with dynamic data and will submit data securely to the 3rd Party Payment Provider
        // - Alan C, 25 June 2015
        public IngenicoPaymentProvider()
        {

        }

        #endregion

        #region Properties

        public string ID { get { return "1"; } } // Identifies Payment Provider for Controller - Alan C 25 June 2015
        public PaymentHandlerType HandlerType { get { return PaymentHandlerType.Remote; } } // Tells Controller during 'Submit' Action to expect a Redirect and Use POST - Alan C, 25 June 2015      
        public Order Order { get; set; } // Provides Strongly Typed Model for Order Details Data to pass to Payment Provider - Alan C, 25 June 2015
        public IOrderConfiguration OrderConfiguration { get; set; } // Provides the Payment Provider Model with the Model Property into which we will populate Order Configurtation data, such as Warehouse, Market, Currency, etc, inehrits from IOrderConfiguration - Alan C 25 June 2015

        // Test Credentials - SHA-In Pass Phrase
        //public string HashSalt = "testSalt123@temp"; // Switch to this for using Test Account before Publishing to UAT Sites - Alan C, 25 June 2015

        // Live Creds - SHA-In Pass Phrase
        public string HashSalt = "liveWINSalt123@Oats"; // Switch to this for using Production Account before Publishing to Live Sites - Alan C, 25 June 2015

        #endregion

        #region Methods

        // Prepares Form Data for Posting to Payment Provider - Alan C, 25 June 2015
        public IPaymentRequest GetPaymentRequest(PaymentRequestArgs args)
        {

            // We will populate form data from the View and from static settings into this Strongly Typed Model
            // Which will collect our Variables and then assign them to a Dictionary of Form Properties expected by our Payment Providers
            // - Alan C 25 June 2015
            var postRequest = new POSTPaymentRequest(); // New instance of our Model for HTTP POST Form Data to 3rd Party Payment Providers - Location: 

            #region Variables for Request Data

            var Request = HttpContext.Current.Request; // Access current URI information for Referring Page
            var customer = Exigo.OData().Customers.Where(c => c.CustomerID == Order.CustomerID).FirstOrDefault(); // Retrieve data for Customer placing the order
            var urlhelper = new UrlHelper(Request.RequestContext); // Access helper classes and methods for constructing dynamic URLs into the Absolute paths required by the Payment Provider
            //var encryptor = new MD5CryptoServiceProvider(); - Not being used at the moment - Alan C, 25 June 2015

            var url = GlobalSettings.Merchants.Ingenico.Address; // Switch to Test URL for UAT Sites and Production URL for Live Sites
            var referenceCode = Order.OrderID.ToString() + "-" + args.Attempt;
            var orderToken = Security.Encrypt(new { OrderID = Order.OrderID, CustomerID = Order.CustomerID });
            var email = customer.Email;
            var tax = Order.TaxTotal;
            var language = GlobalUtilities.GetSelectedLanguage().Replace("-", "_");
            var recipient = Order.Recipient;
            var addressDisplay = (recipient.Address2.IsEmpty()) ? recipient.Address1 : recipient.Address1 + " " + recipient.Address2;
            var phone = customer.Phone;

            // Set default payment method to iDeal to bypass payment method list screen on Ingenico
            var paymentMethod = "iDEAL";


            // Format and set up our parameters for the Return URL to which Payment Provider will return their Response
            var returnUrl = args.ReturnUrl;
            returnUrl += (returnUrl.Contains("?") ? "&" : "?") + "_p=" + ID; // Adds Payment Provider identifier to the Return URL
            returnUrl += "&" + "_cid=" + Order.CustomerID;// Adds CustomerID to the Return URL
            returnUrl += "&" + "_oID=" + Order.OrderID;
            returnUrl += "&" + "token=" + orderToken; // Adds tokenized status / response data from Payment Provider to the Return URL

            // We run our web alias logic below. If we are in a replicated site setting, we need to ensure the web alias is inserted correctly in the return urls
            var appPath = (args.RequiresWebAlias) ? "/{0}/".FormatWith(args.WebAlias) : "/";
            var fullReturnUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, appPath, returnUrl);

            var successUrl = fullReturnUrl + "&ps=success";
            var errorUrl = fullReturnUrl + "&ps=error";
            var declineUrl = fullReturnUrl + "&ps=decline";
            var cancelUrl = fullReturnUrl + "&ps=cancel";
            var homeView = (args.RequiresWebAlias) ? urlhelper.Action("index", "home", new { webalias = args.WebAlias }) : urlhelper.Action("index", "dashboard");
            var homeUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, Request.ApplicationPath, homeView.TrimStart('/'));
            var shopView = (args.RequiresWebAlias) ? urlhelper.Action("itemlist", "shopping", new { webalias = args.WebAlias }) : urlhelper.Action("itemlist", "shopping");
            var shopUrl = FormatReturnURL(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, Request.ApplicationPath, shopView.TrimStart('/'));

            var shaPassPhrase = HashSalt;

            var address = (args.BillingAddress != null) ? args.BillingAddress : new Address();
            //var address = (args.BillingAddress != null) ? args.BillingAddress : recipient; 

            // Establish a variable for UTF8 encoding so we can properly encode necessary fields - Alan C, 25 June 2015
            var utf8 = Encoding.UTF8;

            // Encode the Customer's name to allow for the special characters and accents of Unicode that appear in European Names - Alan C, 25 June 2015
            byte[] utfBillingName = utf8.GetBytes(args.BillingName);

            //string stringName = utf8.GetString(utfBillingName); - Not being used - Alan C, 25 June 2015

            #endregion

            #region Populate Data into Form Properties

            // Establish Collection of Payment Provider Form Properties to which we Assign Values from our Variables - Alan C, 25 June 2015
            var data = new Dictionary<string, object>()
            {
                {"PSPID", "WINBV" },
                //{"ORDERID", "WinExigoOrder" + Order.OrderID.ToString() },
                {"ORDERID", Order.OrderID.ToString() }, // Changed to remove 'WinExigoOrder' per Ticket 67703 on 29 June 2015 - Alan C
                {"AMOUNT", (Order.Total * 100).ToString("0") },
                {"CURRENCY", CurrencyCodes.Euro },
                {"LANGUAGE", language },
                // optional customer details, highly recommended for fraud prevention
                {"CN", args.BillingName},
                {"EMAIL", recipient.Email },
                {"OWNERADDRESS", address.AddressDisplay },
                {"OWNERCTY", address.City },
                {"OWNERTELNO", recipient.Phone },
                {"OWNERZIP", address.Zip },
                {"COM", "Exigo Order {0}".FormatWith(Order.OrderID) },               
                //<!-- link to your website: see Default reaction -->
                {"HOMEURL", homeUrl }, // Page to return customer to if they click "cancel" - constructed dynamically based onw whether Replciated Site or BackOffice - Alan C, 29 June 2015
                {"CATALOGURL", shopUrl }, // Shopping landing page if needed - constructed dynamically based onw whether Replciated Site or BackOffice - Alan C, 29 June 2015
                //<!-- post payment redirection: see Redirection depending on the payment result -->
                {"ACCEPTURL", successUrl },
                {"DECLINEURL", declineUrl },
                {"EXCEPTIONURL", errorUrl },
                {"CANCELURL", cancelUrl },
                //<!-- Tell Ingenico to skip the Payment Method List page and go directly to iDEAL - Alan C 29 June 2015 -->
                {"PM",  paymentMethod}, 
                

            };

            #endregion

            var hashedData = GetHashedData(data, shaPassPhrase); // Encrypt our Data Properties using our SHA-In Phrase - Alan C, 25 June 2015

            data.Add("SHASIGN", hashedData); // Add our SHA-In Signature and hashed (encrypted into string format) Form Data to Dictionary of Form Properties - Alan C, 25 June 2015

            postRequest.Method = PaymentRequestMethod.Post; // Define the HTTP Method we will use to gather and submit the data to the Payment Provider - Alan C, 25 June 2015
            postRequest.RequestUrl = url; // Add the URL to which we are submitting to our request - Alan C, 25 June 2015
            postRequest.RequestForm = FormHelper.GetSelfPostingFormHtml(url, data); // Call the helper classes and methods to format the data and prepare for submission - Alan C, 25 June 2015

            try
            {
                var context = Exigo.ODataLogging();
                context.AddToLogs(new Common.Api.ExigoOData.LoggingContext.Log { OrderID = Order.OrderID, Request = postRequest.RequestUrl + "  |  " + postRequest.RequestForm, RequestDate = DateTime.Now });
                context.SaveChanges();
            }
            catch (Exception exception) { var error = exception.Message; }

            return postRequest;
        }

        // Allows Controller to retrieve the Encrypted Form Data from the GetPaymentRequest Method - Alan C, 25 June 2015
        public string GetHashedData(Dictionary<string, object> data, string shaPassPhrase)
        {
            SHA1 sha1 = SHA1.Create();
            // Build up each line one-by-one and then trim the end
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object> pair in data.OrderBy(d => d.Key).Where(d => d.Value != null && !d.Value.ToString().IsEmpty()))
            {
                //var value = (pair.Key == "CN") ? Encoding.UTF8.GetString((byte[])pair.Value) : pair.Value;
                builder.Append(pair.Key).Append("=").Append(pair.Value).Append(shaPassPhrase);
            }
            string input = builder.ToString();

            byte[] bytes = new UTF8Encoding().GetBytes(input);
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

        // Get Customer Data for Deferred Response if it does not contain the "_cid" paramater in its response
        public Order GetOrderInformation(int OrderID)
        {
            var _order = new Order();

            var request = Exigo.OData().Orders.Where(o => o.OrderID == OrderID).FirstOrDefault();

            _order = (ExigoService.Order)request;

            return _order;
        }

        // Gets the Response from the Payment Provider to determine what View Actions we will execute
        public PaymentResult GetResponse(Uri uri, NameValueCollection form)
        {


            // Ingenico Account is configured to return to the following Action: Replicated Site - AppController - PaymentRedirectLanding

            // Based on the type of Shopping Cart identified in the Properties Region of the Shopping Controllers, Enrollment Controller, and Account Controller, the Action, Replicated Site - App Controller - PaymentRedirectLandingComplete determines to which URL / page to redirect the customer when they are being returned to our site from the 3rd Party site - Alan C - 25 June 2015

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
            var updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Pending; // Set our order status to Pending by Default - Meaning it is Pending Payment when we do our redirect - Alan C 30, June 2015
            var statusCode = "";

            if (querystring.ContainsKey("ps")) // Look for url paramater "ps" to tell us what the outcome of the redirect was - Alan C, 29 June 2015
            {
                statusCode = querystring["ps"];
            }
            else if (querystring.ContainsKey("STATUS"))
            {
                if (querystring["STATUS"] == "9")
                {
                    statusCode = "success"; // Look for form paramater "STATUS" to tell us what the outcome of the redirect was - Alan C, 29 June 2015
                }
                else
                {
                    statusCode = "fail";
                }

            }
            else if (querystring.ContainsKey("ACCEPTANCE"))
            {
                if (querystring["ACCEPTANCE"].IsNotNullOrEmpty())
                {
                    statusCode = "success"; // Look for the string "Acceptance" to tell us the outcome - Alan C, 29 June 2015
                }
                else
                {
                    statusCode = "fail";
                }
            }
            else statusCode = "fail";

            result.Token = (querystring.ContainsKey("token")) ? querystring["token"] : "";

            if (querystring.ContainsKey("PM"))
            {
                result.PaymentType = querystring["PM"];
            }
            else if (querystring.ContainsKey("BRAND"))
            {
                result.PaymentType = querystring["BRAND"];
            }
            else result.PaymentType = "iDEAL";

            result.OrderID = (querystring.ContainsKey("orderID")) ? Convert.ToInt32(querystring["orderID"]) : Convert.ToInt32(querystring["_oID"]);

            var orderInfo = GetOrderInformation(result.OrderID);

            result.CustomerID = (querystring.ContainsKey("_cid")) ? Convert.ToInt32(querystring["_cid"]) : orderInfo.CustomerID;
            if (querystring["_p"] == "1")
            result.ProviderType = "Ingenico";
            //result.QueryString = querystring;
            result.Status = (statusCode == "success") ? PaymentStatus.Success : PaymentStatus.Fail;

            //switch (statusCode)
            //{
            //    case "success":
            //        updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Accepted;
            //        break;
            //    case "decline":
            //        updatedStatus = Common.Api.ExigoWebService.OrderStatusType.CCDeclined;
            //        break;
            //    case "cancel":
            //        updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Canceled;
            //        break;
            //    default:
            //        updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Pending; // Default - leave order status as Pending per James H at WIN - Alan C 30 June 2015
            //        break;
            //}

            // For displaying feedback data to customer if we are not leaving the sit eto enter payment data - not needed - Alan C, 25 June 2015
            //var transactionState = querystring["lapResponseCode"];
            //result.Attempt = Convert.ToInt32(querystring["referenceCode"].Split('-')[1]);
            //result.TransactionRecord = querystring["reference_pol"];
            //result.ErrorMessage = GetResponseMessage(Convert.ToInt32(statusCode), transactionState);
            //result.ErrorMessage = GetResponseMessage(statusCode, transactionState);

            // Get the Status Code and Determine Which Exigo Order Status Type we will use to update the Order Status in the Admin Portal - Do not try to update an Order Status as "Accepted" 
            // per Client - they get that information from deferred data from Ingenico so that customers cannot hack the URL feedback; 
            //The deferred response is pointed to the Action "PaymentRedirectLanding" in the Replicated Site App Controller - 
            //responses for both the Backoffice and the Replicated Site return here - Alan C, 29 June 2015

            switch (querystring["STATUS"])
            {
                case "0":
                    updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Pending; // Means an error has occurred - We change the Status to Pending so the customer will know to follow up on the Payment due - Alan C 30 June 2015
                    break;
                case "1":
                    updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Canceled; // This code means that the "Cancel" button was clicked on Ingenico - we change the order status to Canceled - Alan C 30 June 2015
                    break;
                //case "5":
                //    updatedStatus = Common.Api.ExigoWebService.OrderStatusType.ACHPending;
                //    break;
                case "7":
                    updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Canceled; // Means the payment has been deleted - so we cancel the order - Alan C 30 June 2015
                    break;
                default:
                    updatedStatus = Common.Api.ExigoWebService.OrderStatusType.Pending; // Default - leave order status as Pending per James H at WIN - Alan C 30 June 2015
                    break;
            }

            // Update the Order Status before we return our result
            var contextcall = Exigo.WebService();
            var request = new ChangeOrderStatusRequest()
            {
                OrderID = result.OrderID,
                OrderStatus = updatedStatus
            };
            var response = contextcall.ChangeOrderStatus(request);

            //var context = Exigo.ODataLogging();
            //var logging = context.Logs.Where(l => l.OrderID == result.OrderID).FirstOrDefault();
            //logging.Response = result.Uri.ToString();
            //logging.ResponseDate = DateTime.Now;
            //context.UpdateObject(logging);
            //context.SaveChanges();


            return result; // Return our result and response data to the Controller to execute the final actions we need to occur in the Views and Controller
        }

        #endregion

        #region Helpers

        // Build the Return Url to provide to the Payment Provider - Alan C, 25 June 2015
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

        // Not currently being used - Alan C, 25 June 2015
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

        #endregion
    }
}