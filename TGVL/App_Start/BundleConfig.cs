using System.Web;
using System.Web.Optimization;

namespace TGVL
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // TungDM - Gentelella Template
            bundles.Add(new StyleBundle("~/Content/customcss").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/nprogress.css",
                      "~/Content/custom.min.css",
                      "~/Content/daterangepicker.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/loginformcss").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/nprogress.css",
                      "~/Content/animate.min.css",
                      "~/Content/custom.min.css",
                      "~/Content/bootstrap-social.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/customjquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/fastclick.js",
                        "~/Scripts/nprogress.js",
                        "~/Scripts/raphael.min.js",
                        "~/Scripts/morris.min.js",
                        "~/Scripts/bootstrap-progressbar.min.js",
                        "~/Scripts/moment.js",
                        "~/Scripts/daterangepicker.js",
                        "~/Scripts/custom.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/manageshop").Include(
                         "~/Scripts/jquery-{version}.js",
                         "~/Scripts/bootstrap.js",
                         "~/Scripts/fastclick.js",
                         "~/Scripts/nprogress.js",
                         "~/Scripts/Chart.js",
                         "~/Scripts/jquery.sparkline.js",
                         "~/Scripts/raphael.min.js",
                         "~/Scripts/morris.min.js",
                         "~/Scripts/gauge.js",
                         "~/Scripts/bootstrap-progressbar.min.js",

                         "~/Scripts/skycons.js",

                         "~/Scripts/canvasjs.min.js",

                         "~/Scripts/jquery.flot.js",
                         "~/Scripts/jquery.flot.pie.js",
                         "~/Scripts/jquery.flot.time.js",
                         "~/Scripts/jquery.flot.stack.js",
                         "~/Scripts/jquery.flot.resize.js",

                         "~/Scripts/jquery.flot.orderBars.js",
                         "~/Scripts/jquery.flot.spline.min.js",
                         "~/Scripts/curvedLines.js",

                         "~/Scripts/date.js",
                         "~/Scripts/moment.js",
                         "~/Scripts/daterangepicker.js",
                         "~/Scripts/custom.js"
                         ));
        }
    }
}
