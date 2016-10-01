using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.News
{
    public class AddNewsViewModel
    {
        public int ArticleID { get; set; }
        [Required]
        [DisplayName("Title")]
        public string ArticleTitle { get; set; }
        [Required]
        [DisplayName("Article Text")]
        public string ArticleBody { get; set; }
        public int CreatedByUser { get; set; }
        [Required]
        [DisplayName("Date of Publication")]
        [DataType(DataType.Date)]
        public DateTime DateOfArticle { get; set; }
    }
}

