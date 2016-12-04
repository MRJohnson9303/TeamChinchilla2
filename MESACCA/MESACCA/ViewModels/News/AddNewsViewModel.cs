using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MESACCA.ViewModels.News
{
    public class AddNewsViewModel
    {
        public int ArticleID { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The title of your posting must be 100 characters or less.")]
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

        public string Attach1URL { get; set; }
        public string FileName { get; set; }
        public string CurrentAttachedFile { get; set; }

        public Boolean RemoveFile { get; set; }
    }
}

