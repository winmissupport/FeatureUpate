using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Web;

namespace Common.Services
{
    public class ThirdPartyPaymentService
    {
        public void ProcessAlignNetPayment()
        {
            var form = new StringBuilder();

            form.AppendFormat(@"<html>
                                    <body onload='document.forms[""paymentform""].submit()'>
                                        <form name='paymentform' action='{0}' method='post'>
                                            <input type='hidden' name='test' value='{1}' />
                                        </form>
                                    </body>    
                                </html>"
                               , 0
                               , "test");

            HttpContext.Current.Response.Write(form.ToString());
            HttpContext.Current.Response.End();
        }
    }
}
