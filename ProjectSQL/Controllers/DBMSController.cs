using System.Web.Mvc;

namespace ProjectSQL.Controllers {

    public class DBMSController : Controller {
        
        // Return the view to load the reserved words
        [HttpGet]
        public ActionResult LoadReservedWords() {
            return View();
        }

    }

}
