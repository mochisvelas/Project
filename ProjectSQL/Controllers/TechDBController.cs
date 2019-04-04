using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectSQL.Controllers
{
    public class TechDBController : Controller
    {
        [HttpGet]
        public ActionResult SQLCode() {

            return View();
        }

        [HttpGet]
        public ActionResult SQLDisplay() {

            return View();
        }

        [HttpGet]
        public ActionResult TableInfo() {

            return View();
        }
    }
}