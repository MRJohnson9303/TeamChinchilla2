using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESACCA.Models
{
    public class NewsArticle
    {
        public int ArticleID { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleBody { get; set; }
        public int CreatedByUser { get; set; }
        public System.DateTime DateOfArticle { get; set; }
        public User User { get; set; }
        public string Attach1URL { get; set; }
        public string fileName { get; set; }
    }
}