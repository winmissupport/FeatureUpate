using System.Collections.Generic;
using System.Globalization;

namespace ExigoService
{
    
    
    public static partial class Exigo
    {
        public static string culturename = CultureInfo.CurrentCulture.Name;

        private static readonly IEnumerable<IRankRequirementDefinition> RankQualificationDefinitions = new List<IRankRequirementDefinition>
        {           
          
            Boolean("Status",  
                Expression: @"^MUST HAVE A VALID STATUS - ACTIVE$",
                Description: "Your account must be active.",
                Qualified: "",
                NotQualified: "You're account is not active."
            ),

            Boolean("Customer Type", 
                Expression: @"^MUST BE A VALID CUSTOMER TYPE - DISTRIBUTOR$", 
                Description: "You must be a Distributor.",
                Qualified: "",
                NotQualified: "You are not a Distributor."
            ),

            Boolean("Personally Active", 
                Expression: @"^1 PERSONALLY ACTIVE$",
                Description: "You must be Personally Active.",
                Qualified: "",
                NotQualified: "You are not personally active."
            ),

            Decimal("Active Legs", 
                Expression: @"^\d+ ACTIVE LEGS$",
                Description: "You must have at least {{RequiredValueAsDecimal:N0}} Active Legs.",
                Qualified: "",
                NotQualified: "You must have at least <strong>{{AmountNeededToQualify:N0}}</strong> more Active Leg."
            ),

             Decimal("2000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 1200 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} TGQV capped at " + (1200.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),

             Decimal("6000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 3600 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} TGQV capped at " + (3600.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),

             Decimal("12000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 7200 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} TGQV capped at " + (7200.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),

             Decimal("36000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 14400 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} capped at " + (14400.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),

             Decimal("75000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 30000 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} TGQV capped at " + (30000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),

             Decimal("150000 TGQV", 
                Expression: @"^\d+ TGQV CAPPED AT 60000 PER LEG WITH A MAX PQV OF 1000$",
                Description: "You must must have at least {{RequiredValueAsDecimal:N0}} TGQV capped at " + (60000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))) + " per leg with a max PQV of " + (1000.ToString("##,#", CultureInfo.GetCultureInfo(culturename))),
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more TGQV."
            ),
            
        };
    }
}