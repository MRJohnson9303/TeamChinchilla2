using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MESACCA.Models;

namespace MESACCA.ViewModels.News
{
    public class SelectNewsViewModel
    {
        public List<NewsArticleExtension> Articles { get; set;}
    }
}

