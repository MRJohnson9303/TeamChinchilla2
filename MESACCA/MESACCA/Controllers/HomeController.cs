using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MESACCA.Models;
using MESACCA.DataBaseManagers;

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
            List<Models.Center> centerList = SQLManager.sqlConnectionForCentersList();
            Models.Center center = SQLManager.sqlConnectionForCenter(1);
            return View(centerList);
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
            return View(SQLManager.getNewsPosts());
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

        public ActionResult Collaboration()
        {
            return View();
        }
        
        
    }
}

