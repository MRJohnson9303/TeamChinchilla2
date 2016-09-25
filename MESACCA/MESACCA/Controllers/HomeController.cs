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
            ViewData["Posts"] = getNewsPosts();

            return View(getNewsPosts());
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



        private List<String> getNewsPosts()
        {
            List<String> returnValue = new List<String>();

            //get a certain number of posts from back end

            for (int i = 1; i < 5; i++)
            {
                returnValue.Add(" have panel which I colored blue if this panel is being selected (clicked on it). Additionally, I add a small sign (.png image) to that panel, which indicates that the selected panel has been already selected before. So if the user sees for example 10 panels and 4 of them have this small sign, he knows that he has already clicked on those panels before.This work fine so far.The problem is now that I can't display the small sign and make the panel blue at the same time. I set the panel to blue with the css background: #6DB3F2; and the background image with background-image: url('images/checked.png'). But it seems that the background color is above the image so you cannot see the sign. Is it therefore possible to set z - indexes for the background color and the background image ? ");
            }
            return returnValue;
        }
    }
}

