using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MESACCA.ViewModels.News;
using System.Configuration;
using System.Data.SqlClient;

namespace MESACCA.Controllers
{
    public class NewsController : Controller
    {
        private MCCA_DatabaseEntities mccaDB = new MCCA_DatabaseEntities();
        // GET: News
        public ActionResult AddNews()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddNews(AddNewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newsArticle = new NewsArticle()
                {
                    ArticleTitle = model.ArticleTitle,
                    ArticleBody = model.ArticleBody,
                    DateOfArticle = DateTime.Now,
                    CreatedByUser = 1
                };

                mccaDB.NewsArticles.Add(newsArticle);
                mccaDB.SaveChanges();
                ViewBag.Message = "News Article Added Successfully.";
                return View();
            }

            return View(model);
        }
        
    }
}