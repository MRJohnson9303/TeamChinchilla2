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
        [ValidateUser]
        public ActionResult Index()
        {
            return View(SQLManager.sqlConnectionGetDonation());
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult UpdateDonation(DonationViewModel model)
        {
            SQLManager.sqlConnectionUpdateDonation(model);

            return RedirectToAction("Index");
        }
    }
}