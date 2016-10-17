using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MESACCA.ViewModels.News;
using System.Configuration;
using System.Data.SqlClient;
using MESACCA.DataBaseManagers;
using MESACCA.Utilities;

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
                    CreatedByUser = MyUserManager.GetUser().ID
            };
                Boolean success = false;

                success = SQLManager.sqlConnectionAddNews(newsArticle);
                if (success == true)
                {
                    TempData["Message"] = "News Article Added Successfully.";
                }
                else
                {
                    TempData["Message"] = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }
                return RedirectToAction("SelectNews");
            }

            return View(model);
        }

        public ActionResult SelectNews()
        {
            try
            {
                SelectNewsViewModel snvm = new SelectNewsViewModel();
                snvm.Articles = SQLManager.getNewsPostsForAdmin();
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    TempData.Remove("Message");
                }
                return View(snvm);
            }
            
            catch (Exception ex)
            {

            }
            return RedirectToAction("Index", "Member");
        }

        public ActionResult DeleteNews(int id)
        {
            Boolean success = false;

                success = SQLManager.sqlConnectionDeleteNews(id);
                if (success == true)
                {
                    TempData["Message"] = "Successfully deleted news posting.";
                }
                else
                {
                    TempData["Message"] = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }

            return RedirectToAction("SelectNews");

        }

    }
}