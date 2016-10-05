using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.News
{
    public class AddNewsViewModel
    {
        public int ArticleID { get; set; }
        [Required]
        [DisplayName("Title of News Posting")]
        public string ArticleTitle { get; set; }
        [Required]
        [DisplayName("Article Text")]
        [AllowHtml]
        public string ArticleBody { get; set; }
        public int CreatedByUser { get; set; }
        [Required]
        [DisplayName("Date of Publication")]
        [DataType(DataType.Date)]
        public DateTime DateOfArticle { get; set; }
    }
}

