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

           
            //TungDM
            //ajax
            bundles.Add(new ScriptBundle("~/bundles/otf").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/otf.js"
                ));
           
            // TungDM - Gentelella Template
            bundles.Add(new StyleBundle("~/Content/customcss").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/nprogress.css",
                      "~/Content/custom.min.css",
                      "~/Content/daterangepicker.css",
                      "~/Content/jquery-ui.css"
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

            // VyCMT
            bundles.Add(new StyleBundle("~/Content/mainpagecss").Include(
                "~/Content/css/bootstrap.min.css",
                "~/Content/css/slider.css",
                "~/Content/css/owl.carousel.css",
                "~/Content/css/owl.theme.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap.css",
                "~/Content/AdminLTE.min.css",
                "~/Content/css/style.css"
             ));

            bundles.Add(new StyleBundle("~/Content/viewdetailcss").Include(
                "~/Content/css/bootstrap.min.css",
                "~/Content/css/owl.carousel.css",
                "~/Content/css/owl.theme.css",
                "~/Content/css/flexslider.css",
                "~/Content/css/fancybox.css.css",
                "~/Content/font-awesome.min.css",
                "~/Content/css/style.css"
             ));


            bundles.Add(new StyleBundle("~/Content/createrequestcss").Include(
                "~/Content/css/air.components.2.3.1.min.css",
                "~/Content/css/air.global.2.3.1.min.css",
                "~/Content/css/homepage.css",
                "~/Content/css/components.css"
             ));

            
            bundles.Add(new StyleBundle("~/Content/viewcategorycss").Include(
                "~/Content/css/style.css",
                "~/Content/css/easy-responsive-tabs.css",
                "~/Content/css/slider.css",
                "~/Content/css/global.css"
             ));

            bundles.Add(new StyleBundle("~/Content/bidrequestcss").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/nprogress.css",
                "~/Content/daterangepicker.css",
                "~/Content/ion.rangeSlider.css",
                "~/Content/ion.rangeSlider.skinFlat.css",
                "~/Content/bootstrap-colorpicker.min.css",
                "~/Content/green.css",
                "~/Content/prettify.min.css",
                "~/Content/select2.min.css",
                "~/Content/switchery.min.css",
                "~/Content/starrr.css",
                
                "~/Content/custom.min.css"
                ));
            bundles.Add(new StyleBundle("~/Content/productcss").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/nprogress.css",
                "~/Content/daterangepicker.css",
                "~/Content/green.css",
                "~/Content/dataTables.bootstrap.min.css",
                "~/Content/buttons.bootstrap.min.css",
                "~/Content/fixedHeader.bootstrap.min.css",
                "~/Content/responsive.bootstrap.min.css",
                "~/Content/scroller.bootstrap.min.css",
                "~/Content/custom.min.css"
                ));

            // jquery
            bundles.Add(new ScriptBundle("~/bundles/mainpagejquery").Include(
                                    "~/Scripts/js/jquery.min.js",
                                    "~/Scripts/js/bootstrap.min.js",
                                    "~/Scripts/js/parallax.js",
                                    "~/Scripts/js/common.js",
                                    "~/Scripts/js/slider.js",
                                    "~/Scripts/js/owl.carousel.min.js"
                                    ));

            bundles.Add(new ScriptBundle("~/bundles/viewdetailjquery").Include(
                                    "~/Scripts/js/prototype.js",
                                    "~/Scripts/js/jquery.min.js",
                                    "~/Scripts/js/bootstrap.min.js",
                                    "~/Scripts/js/common.js",
                                    "~/Scripts/js/owl.carousel.min.js",
                                    "~/Scripts/js/toggle.js",
                                    "~/Scripts/js/pro-img-slider.js",
                                    "~/Scripts/js/jquery.flexslider.js",
                                    "~/Scripts/js/cloud-zoom.js"
                                    ));
            
            bundles.Add(new ScriptBundle("~/bundles/viewcategoryjquery").Include(
                                    "~/Scripts/js/jquery-1.11.0.min.js",
                                    "~/Scripts/js/move-top.js",
                                    "~/Scripts/js/easing.js",
                                    "~/Scripts/js/easyResponsiveTabs.js",
                                    "~/Scripts/js/slides.min.jquery.js"
                                    
                                    ));

            bundles.Add(new ScriptBundle("~/bundles/bidrequestjquery").Include(
                                   "~/Scripts/jquery.min.js",
                                    "~/Scripts/bootstrap.min.js",
                                    "~/Scripts/fastclick.js",
                                    "~/Scripts/nprogress.js",
                                    "~/Scripts/moment.min.js",
                                    "~/Scripts/daterangepicker.js",
                                    "~/Scripts/bootstrap-colorpicker.min.js",
                                    "~/Scripts/bootstrap-progressbar.min.js",
                                    "~/Scripts/icheck.min.js",
                                    "~/Scripts/bootstrap-wysiwyg.min.js",
                                    "~/Scripts/jquery.hotkeys.js",
                                    "~/Scripts/prettify.js",
                                    "~/Scripts/jquery.tagsinput.js",
                                    "~/Scripts/switchery.min.js",
                                    "~/Scripts/select2.full.min.js",
                                    "~/Scripts/autosize.min.js",
                                    "~/Scripts/jquery.autocomplete.min.js",
                                    "~/Scripts/starrr.js",
                                    "~/Scripts/dropzone.min.js",
                                    "~/Scripts/custom.min.js"
                                    
                                    ));
            
            bundles.Add(new ScriptBundle("~/bundles/productjquery").Include(
                                    "~/Scripts/jquery.min.js",
                                    "~/Scripts/bootstrap.min.js",
                                    "~/Scripts/fastclick.js",
                                    "~/Scripts/nprogress.js",
                                    "~/Scripts/moment.min.js",
                                    "~/Scripts/daterangepicker.js",
                                    "~/Scripts/icheck.min.js",
                                    "~/Scripts/jquery.dataTables.min.js",
                                    "~/Scripts/dataTables.bootstrap.min.js",
                                    "~/Scripts/dataTables.buttons.min.js",
                                    "~/Scripts/buttons.bootstrap.min.js",
                                    "~/Scripts/buttons.flash.min.js",
                                    "~/Scripts/buttons.html5.min.js",
                                    "~/Scripts/buttons.print.min.js",
                                    "~/Scripts/dataTables.fixedHeader.min.js",
                                    "~/Scripts/dataTables.keyTable.min.js",
                                    "~/Scripts/dataTables.responsive.min.js",
                                    "~/Scripts/responsive.bootstrap.js",
                                    "~/Scripts/dataTables.scroller.min.js",
                                    "~/Scripts/jszip.min.js",
                                    "~/Scripts/pdfmake.min.js",
                                    "~/Scripts/vfs_fonts.js",
                                    "~/Scripts/jquery.smartWizard.js",
                                    "~/Scripts/custom.min.js"
                                    ));
            
        }
    }
}
