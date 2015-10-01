using Common.Bundles;
using System.Web.Optimization;

namespace ReplicatedSite
{
    public class BundleConfig
    {

        public static void RegisterBundles(BundleCollection bundles)
        {
            // Enable bundling optimizations, even when the site is in debug mode or local.
            BundleTable.EnableOptimizations = true;


            // Create the bundles
            var styles = new StyleBundle("~/bundles/styles");
            var icons = new StyleBundle("~/bundles/icons");
            var fonts = new StyleBundle("~/bundles/fonts");
            var scripts = new ScriptBundle("~/bundles/scripts");


            // jQuery
            scripts.Include(
                "~/Content/scripts/vendor/jquery.js",
                "~/Content/scripts/vendor/jquery.extended.js",
                "~/Content/scripts/vendor/jquery.validate.js",
                "~/Content/scripts/vendor/jquery.validate.config.js",
                "~/Content/scripts/vendor/jquery.validate.unobtrusive.js",
                "~/Content/scripts/vendor/jquery.validate.unobtrusive.custom.js",
                "~/Content/scripts/vendor/jquery.unobtrusive-ajax.js");


            // Common styles
            styles.Include("~/Content/styles/common.min.css", new CssRewriteUrlTransformer());


            // Bootstrap
            scripts.Include("~/Content/scripts/vendor/bootstrap.js");


            // Icons
            icons.Include("~/Content/styles/icons.min.css");

            // Fonts
            fonts.Include("~/Content/styles/fonts.min.css");


            // Toastr
            styles.Include("~/Content/scripts/vendor/toastr/toastr.css");
            scripts.Include("~/Content/scripts/vendor/toastr/toastr.js");


            // Antiscroll
            styles.Include("~/Content/scripts/vendor/antiscroll/antiscroll.css");
            scripts.Include("~/Content/scripts/vendor/antiscroll/antiscroll.js");


            // Mousewheel
            scripts.Include("~/Content/scripts/vendor/jquery.mousewheel.js");


            // Loading Buttons plugin styles
            styles.Include("~/Content/scripts/vendor/loadingbuttons/loadingbuttons.css", new CssRewriteUrlTransformer());
            scripts.Include(
                "~/Content/scripts/modules/widgets.js",
                "~/Content/scripts/vendor/loadingbuttons/loadingbuttons.js");


            // Kendo
            styles.Include("~/Content/scripts/vendor/kendo/styles/kendo.common-bootstrap.min.css", new CssRewriteUrlTransformer());
            styles.Include("~/Content/scripts/vendor/kendo/styles/kendo.bootstrap.min.css", new CssRewriteUrlTransformer());
            scripts.Include("~/Content/scripts/vendor/kendo/scripts/kendo.web.min.js");


            // jCrop styles
            styles.Include("~/Content/scripts/vendor/jcrop/jcrop.css", new CssRewriteUrlTransformer());



            // App-specific styles and scripts
            styles.Include("~/Content/styles/site.min.css", new CssRewriteUrlTransformer());
            scripts.Include("~/Content/scripts/modules/extensions.js");

            // Client Styles
            styles.Include("~/Content/styles/client/style.css", new CssRewriteUrlTransformer());
            styles.Include("~/Content/styles/client/responsive.css");
            styles.Include("~/Content/styles/client/base.css");

            // Client Scripts
            scripts.Include(
                "~/Content/scripts/template/jquery.jpanelmenu.js",
                "~/Content/scripts/template/jquery.themepunch.plugins.min.js",
                "~/Content/scripts/template/jquery.themepunch.revolution.min.js",
                "~/Content/scripts/template/jquery.themepunch.showbizpro.min.js",
                "~/Content/scripts/template/jquery.magnific-popup.min.js",
                "~/Content/scripts/template/hoverIntent.js",
                "~/Content/scripts/template/superfish.js",
                "~/Content/scripts/template/jquery.pureparallax.js",
                "~/Content/scripts/template/jquery.pricefilter.js",
                "~/Content/scripts/template/jquery.selectric.min.js",
                "~/Content/scripts/template/jquery.royalslider.min.js",
                "~/Content/scripts/template/SelectBox.js",
                "~/Content/scripts/template/modernizr.custom.js",
                "~/Content/scripts/template/waypoints.min.js",
                "~/Content/scripts/template/jquery.flexslider-min.js",
                "~/Content/scripts/template/jquery.counterup.min.js",
                "~/Content/scripts/template/jquery.tooltips.min.js",
                "~/Content/scripts/template/jquery.isotope.min.js",
                "~/Content/scripts/template/puregrid.js",
                "~/Content/scripts/template/stacktable.js",
                "~/Content/scripts/template/custom.js");


            bundles.Add(styles);
            bundles.Add(icons);
            bundles.Add(fonts);
            bundles.Add(scripts);
        }
    }

}