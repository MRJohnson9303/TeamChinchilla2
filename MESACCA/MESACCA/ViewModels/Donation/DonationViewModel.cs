using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MESACCA.ViewModels.Donation
{
    public class DonationViewModel
    {
        [DisplayName("")]
        [AllowHtml]
        public string ArticleBody { get; set; }
    }
}