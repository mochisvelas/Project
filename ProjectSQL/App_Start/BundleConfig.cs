using System.Web.Optimization;

namespace ProjectSQL {

    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {
            // CSS Bundle
            bundles.Add(new StyleBundle("~/bundles/style").Include(
                                        "~/Styles/bulma.min.css",
                                        "~/Styles/jquery.highlight-within-textarea.css"));
            // JS Bundle
            bundles.Add(new StyleBundle("~/bundles/script").Include(
                                        "~/Scripts/jquery-min.js",
                                        "~/Scripts/jquery.highlight-within-textarea.js",
                                        "~/Scripts/style.js"));
        }

    } 

}
