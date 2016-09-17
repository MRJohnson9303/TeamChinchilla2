using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MESACCA.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public ActionResult Centers()
        {
            ViewData["Message"] = "Centers Page";

            return View();
        }
        public ActionResult VisionMissionValues()
        {
            ViewData["Message"] = "VisionMissionValues Page";

            return View();
        }
        public ActionResult MESASchoolsProgram()
        {
            ViewData["Message"] = "MESA Schools Program Page";

            return View();
        }
        public ActionResult MESACommunityCollegeProgram()
        {
            ViewData["Message"] = "MESA Community College Program Page";

            return View();
        }
        public ActionResult MESAEngineeringProgram()
        {
            ViewData["Message"] = "MESA Engineering Program Page";

            return View();
        }
        public ActionResult News()
        {
            ViewData["Message"] = "News Page";

            return View();
        }
        public ActionResult Donate()
        {
            ViewData["Message"] = "Donate Page";

            return View();
        }
        public ActionResult Error()
        {
            return View();
        }
    }
}