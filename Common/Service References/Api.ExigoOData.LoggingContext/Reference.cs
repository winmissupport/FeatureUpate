//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Original file name:
// Generation date: 4/9/2015 10:44:35 AM
namespace Common.Api.ExigoOData.LoggingContext
{
    
    /// <summary>
    /// There are no comments for LoggingContext in the schema.
    /// </summary>
    public partial class LoggingContext : global::System.Data.Services.Client.DataServiceContext
    {
        /// <summary>
        /// Initialize a new LoggingContext object.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public LoggingContext(global::System.Uri serviceRoot) : 
                base(serviceRoot)
        {
            this.ResolveName = new global::System.Func<global::System.Type, string>(this.ResolveNameFromType);
            this.ResolveType = new global::System.Func<string, global::System.Type>(this.ResolveTypeFromName);
            this.OnContextCreated();
        }
        partial void OnContextCreated();
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        protected global::System.Type ResolveTypeFromName(string typeName)
        {
            global::System.Type resolvedType = this.DefaultResolveType(typeName, "CodeFirstNamespace", "Common.Api.ExigoOData.LoggingContext");
            if ((resolvedType != null))
            {
                return resolvedType;
            }
            return null;
        }
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        protected string ResolveNameFromType(global::System.Type clientType)
        {
            if (clientType.Namespace.Equals("Common.Api.ExigoOData.LoggingContext", global::System.StringComparison.Ordinal))
            {
                return string.Concat("CodeFirstNamespace.", clientType.Name);
            }
            return null;
        }
        /// <summary>
        /// There are no comments for Logs in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public global::System.Data.Services.Client.DataServiceQuery<Log> Logs
        {
            get
            {
                if ((this._Logs == null))
                {
                    this._Logs = base.CreateQuery<Log>("Logs");
                }
                return this._Logs;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private global::System.Data.Services.Client.DataServiceQuery<Log> _Logs;
        /// <summary>
        /// There are no comments for Logs in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public void AddToLogs(Log log)
        {
            base.AddObject("Logs", log);
        }
    }
    /// <summary>
    /// There are no comments for CodeFirstNamespace.Log in the schema.
    /// </summary>
    /// <KeyProperties>
    /// LogID
    /// </KeyProperties>
    [global::System.Data.Services.Common.DataServiceKeyAttribute("LogID")]
    public partial class Log
    {
        /// <summary>
        /// Create a new Log object.
        /// </summary>
        /// <param name="logID">Initial value of LogID.</param>
        /// <param name="orderID">Initial value of OrderID.</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public static Log CreateLog(int logID, int orderID)
        {
            Log log = new Log();
            log.LogID = logID;
            log.OrderID = orderID;
            return log;
        }
        /// <summary>
        /// There are no comments for Property LogID in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public int LogID
        {
            get
            {
                return this._LogID;
            }
            set
            {
                this.OnLogIDChanging(value);
                this._LogID = value;
                this.OnLogIDChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private int _LogID;
        partial void OnLogIDChanging(int value);
        partial void OnLogIDChanged();
        /// <summary>
        /// There are no comments for Property OrderID in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public int OrderID
        {
            get
            {
                return this._OrderID;
            }
            set
            {
                this.OnOrderIDChanging(value);
                this._OrderID = value;
                this.OnOrderIDChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private int _OrderID;
        partial void OnOrderIDChanging(int value);
        partial void OnOrderIDChanged();
        /// <summary>
        /// There are no comments for Property Request in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public string Request
        {
            get
            {
                return this._Request;
            }
            set
            {
                this.OnRequestChanging(value);
                this._Request = value;
                this.OnRequestChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private string _Request;
        partial void OnRequestChanging(string value);
        partial void OnRequestChanged();
        /// <summary>
        /// There are no comments for Property Response in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public string Response
        {
            get
            {
                return this._Response;
            }
            set
            {
                this.OnResponseChanging(value);
                this._Response = value;
                this.OnResponseChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private string _Response;
        partial void OnResponseChanging(string value);
        partial void OnResponseChanged();
        /// <summary>
        /// There are no comments for Property RequestDate in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public global::System.Nullable<global::System.DateTime> RequestDate
        {
            get
            {
                return this._RequestDate;
            }
            set
            {
                this.OnRequestDateChanging(value);
                this._RequestDate = value;
                this.OnRequestDateChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private global::System.Nullable<global::System.DateTime> _RequestDate;
        partial void OnRequestDateChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnRequestDateChanged();
        /// <summary>
        /// There are no comments for Property ResponseDate in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public global::System.Nullable<global::System.DateTime> ResponseDate
        {
            get
            {
                return this._ResponseDate;
            }
            set
            {
                this.OnResponseDateChanging(value);
                this._ResponseDate = value;
                this.OnResponseDateChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private global::System.Nullable<global::System.DateTime> _ResponseDate;
        partial void OnResponseDateChanging(global::System.Nullable<global::System.DateTime> value);
        partial void OnResponseDateChanged();
        /// <summary>
        /// There are no comments for Property LogTypeID in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        public global::System.Nullable<int> LogTypeID
        {
            get
            {
                return this._LogTypeID;
            }
            set
            {
                this.OnLogTypeIDChanging(value);
                this._LogTypeID = value;
                this.OnLogTypeIDChanged();
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Services.Design", "1.0.0")]
        private global::System.Nullable<int> _LogTypeID;
        partial void OnLogTypeIDChanging(global::System.Nullable<int> value);
        partial void OnLogTypeIDChanged();
    }
}
