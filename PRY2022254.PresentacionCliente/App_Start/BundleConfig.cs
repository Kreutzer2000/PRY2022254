using System.Web;
using System.Web.Optimization;

namespace PRY2022254.PresentacionCliente
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/complementos").Include(
                        "~/Scripts/wow/wow.min.js",
                        "~/Scripts/easing/easing.min.js",
                        "~/Scripts/waypoints/waypoints.min.js",
                        "~/Scripts/counterup/counterup.min.js",
                        "~/Scripts/owlcarousel/owl.carousel.min.js",
                        "~/Scripts/isotope/isotope.pkgd.min.js",
                        "~/Scripts/lightbox/js/lightbox.min.js",
                        "~/Scripts/main.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            //// Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios.  De esta manera estará
            //// para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap/bootstrap.min.css",
                      "~/Content/Principal/animate/animate.min.css",
                      "~/Content/Principal/carousel/owl.carousel.min.css",
                      "~/Content/Principal/lightbox/css/lightbox.min.css",
                      "~/Content/site.css"));
        }
    }
}
