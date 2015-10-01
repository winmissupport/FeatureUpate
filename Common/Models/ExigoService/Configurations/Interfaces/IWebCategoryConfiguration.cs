using Common;
using Common.Api.ExigoWebService;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExigoService
{
    public interface IWebCategoryConfiguration
    {
        IEnumerable<ItemCategory> Categories { get; }

        ItemCategory FeaturedProducts             { get; }

    }                                       
}