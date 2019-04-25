using System.Web.Optimization;

namespace ProjectSQL {

    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {
            // CSS Bundle
            bundles.Add(new StyleBundle("~/bundles/style").Include(
                                        "~/Styles/bulma.min.css"));
            // JS Bundle
            bundles.Add(new StyleBundle("~/bundles/script").Include(
                                        "~/Scripts/jquery-min.js",
                                        "~/Scripts/style.js"));
        }

    } 

}
