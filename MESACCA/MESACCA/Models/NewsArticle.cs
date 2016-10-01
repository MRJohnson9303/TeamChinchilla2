using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MESACCA.Models
{
    public class NewsArticle
    {
        public int ArticleID { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleBody { get; set; }
        public int CreatedByUser { get; set; }
        public DateTime DateOfArticle { get; set; }
    }
}
