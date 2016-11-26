using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.News
{
    public class ConfirmDeleteNewsViewModel
    {
        public int ArticleID { get; set; }

        public string ArticleTitle { get; set; }
        public string ArticleBody { get; set; }
        public string Attach1URL { get; set; }
        public string FileName { get; set; }

    }
}

