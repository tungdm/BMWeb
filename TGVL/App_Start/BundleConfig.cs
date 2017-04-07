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
                        "~/Scripts/jquery.validate*"
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/css/bootstrap.min.css",
                      "~/Content/site.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/wysiwyg.css",
                      "~/Content/storelocator.css",
                      "~/Content/Search-form.css",
                      "~/Content/header-cart.css",
                      "~/Content/nprogress.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/notification").Include(
                      "~/Content/notification.css"));


            //TungDM

            bundles.Add(new StyleBundle("~/bundles/SummernoteCSS").Include(
                "~/Scripts/summernote/summernote.css"

                ));

            bundles.Add(new ScriptBundle("~/bundles/SummernoteJS").Include(
               "~/Scripts/summernote/summernote.js"
               ));

            //ajax
            bundles.Add(new ScriptBundle("~/bundles/otf").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/otf.js",
                "~/Scripts/custom.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/otf2").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/MvcFoolproofJQueryValidation.js",
                "~/Client_Scripts/mvcfoolproof.unobtrusive.js",
                "~/Scripts/otf-1.2.js",
                //"~/Scripts/custom.min.js",
                "~/Scripts/dateformat.js",
                "~/Scripts/moment.js",
                "~/Scripts/nprogress.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/signalR").Include(
                "~/Scripts/jquery.signalR-2.2.1.min.js",
                "~/Scripts/notification.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/request").Include(
                "~/Scripts/request.js"
                ));


            // VyCMT
            bundles.Add(new StyleBundle("~/Content/alertcss").Include(

                "~/Content/sweetalert.css"
              ));

            bundles.Add(new StyleBundle("~/Content/mainpagecss").Include(
                //"~/Content/css/bootstrap.min.css",
                "~/Content/css/slider.css",
                "~/Content/css/owl.carousel.css",
                "~/Content/css/owl.theme.css",
                //"~/Content/font-awesome.min.css",
                //"~/Content/bootstrap.css",
                //"~/Content/AdminLTE.min.css",
                "~/Content/css/style.css"
             ));

            bundles.Add(new StyleBundle("~/Content/slidercss").Include(
                //"~/Content/test/bootstrap.min.css",
                //"~/Content/test/font-awesome.min.css",
                //"~/Content/test/owl.carousel.css",
                "~/Content/test/responsive.css",
                "~/Content/test/style.css"
             ));

            bundles.Add(new StyleBundle("~/Content/viewdetailcss").Include(
                "~/Content/css/bootstrap.min.css",
                "~/Content/css/owl.carousel.css",
                "~/Content/css/owl.theme.css",
                "~/Content/css/flexslider.css",
                "~/Content/css/fancybox.css",
                //"~/Content/font-awesome.min.css",
                "~/Content/css/style.css"
             ));

            bundles.Add(new StyleBundle("~/Content/searchproductcss").Include(
                //"~/Content/css/bootstrap.min.css",
                //"~/Content/css/owl.carousel.css",
                //"~/Content/css/owl.theme.css",
                //"~/Content/font-awesome.min.css",
                "~/Content/css/flexslider.css",
                "~/Content/css/fancybox.css",
                "~/Content/css/blogmate.css",
                "~/Content/timeTo.css",
                "~/Content/AdminLTE.min.css"
             //"~/Content/css/style.css"

             ));


            // jquery
            bundles.Add(new ScriptBundle("~/bundles/alertjquery").Include(
                                    "~/Scripts/jquery-2.2.3.min.js",
                                    "~/Scripts/sweetalert.js"
                                    ));

            bundles.Add(new ScriptBundle("~/bundles/clockjquery").Include(

                                    "~/Scripts/jquery.time-to.js"
                                    ));

            bundles.Add(new ScriptBundle("~/bundles/mainpagejquery").Include(
                                    "~/Scripts/js/jquery.min.js",
                                    "~/Scripts/js/bootstrap.min.js",
                                    //"~/Scripts/js/parallax.js",
                                    //"~/Scripts/js/common.js",
                                    "~/Scripts/js/slider.js"
                                    //"~/Scripts/js/owl.carousel.min.js",
                                    //"~/Scripts/js/flipclock.js"
                                    ));

            bundles.Add(new ScriptBundle("~/bundles/sliderjquery").Include(
                                    "~/Scripts/js/jquery.min.js",
                                    "~/Scripts/js/bootstrap.min.js",
                                    "~/Scripts/test/bxslider.min.js",
                                    "~/Scripts/test/jquery.easing.1.3.min.js",
                                    //"~/Scripts/test/main.js",
                                    "~/Scripts/test/owl.carousel.min.js",
                                    "~/Scripts/test/script.slider.js"
                                    ));

            bundles.Add(new StyleBundle("~/Content/textareascss").Include(
                                  "~/Content/custom.min.css",
                                  "~/Content/prettify.min.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/textareasjquery").Include(
                                    "~/Scripts/bootstrap-wysiwyg.js",
                                    "~/Scripts/prettify.js",
                                    "~/Scripts/jquery.hotkeys.js",
                                    "~/Scripts/customnguyen.js",
                                    "~/Scripts/textarea-hack.js"
                                    ));
        }
    }
}
