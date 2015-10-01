using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReplicatedSite.Controllers
{
    public class ProductInfoController : Controller
    {
        // GET: ProductInfo
        public ActionResult Index()
        {
            return View();
        }
        [Route("weight")]
        public ActionResult WeightManagements()
        {
            return View();
        }
        [Route("health")]
        public ActionResult OptimumHealth()
        {
            return View();
        }
        [Route("science")]
        public ActionResult TheScience()
        {
            return View();
        }
    }
}