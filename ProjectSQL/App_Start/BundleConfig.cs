using System.Web.Optimization;

namespace ProjectSQL {

    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {
            // JS
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/site.js"));

            //CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                 "~/Content/menu.css",
                 "~/Content/site.css"));
        }

    }

}