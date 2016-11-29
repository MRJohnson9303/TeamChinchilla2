using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MESACCA.FilterAttributes;
using MESACCA.DataBaseManagers;
using MESACCA.ViewModels.Donation;

namespace MESACCA.Controllers
{
    public class DonationController : Controller
    {
        // GET: Donation
        [HttpGet]
        [ValidateUser]
        public ActionResult Index()
        {
            return View(SQLManager.sqlConnectionGetDonation());
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult Index(DonationViewModel model)
        {
            Boolean success = false;
            success = SQLManager.sqlConnectionUpdateDonation(model);
            //If the update was successful, create a confirmation message for the User.
            if (success == true)
            {
                ViewBag.Message = "Successfully updated donation page.";
            }
            //Else give an error message.
            else
            {
                ViewBag.Message = "Database error. Could not update the donation page. Please try again and if the problem persists, contact the Administrator.";
            }
            return View(model);
        }
    }
}