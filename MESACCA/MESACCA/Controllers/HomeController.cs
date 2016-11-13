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
        public ActionResult MCCAMembers()
        {
            List<Models.Center> centerList = SQLManager.sqlConnectionForCentersList();
            centerList.Sort(delegate (Models.Center x, Models.Center y)
            {
                return x.Name.CompareTo(y.Name);
            });
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
            return View(SQLManager.sqlConnectionGetDonation());
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