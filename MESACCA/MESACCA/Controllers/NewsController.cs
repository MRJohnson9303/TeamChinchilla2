using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MESACCA.ViewModels.News;
using System.Configuration;
using System.Data.SqlClient;
using MESACCA.FilterAttributes;
using MESACCA.DataBaseManagers;
using MESACCA.Utilities;

namespace MESACCA.Controllers
{
    public class NewsController : Controller
    {
        private MCCA_DatabaseEntities mccaDB = new MCCA_DatabaseEntities();
        // GET: News
        [ValidateUser]
        public ActionResult AddNews()
        {
            return View();
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult AddNews(AddNewsViewModel model, HttpPostedFileBase File)
        {


            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("Starting here.");
                String FileUri = "";
                //System.Diagnostics.Debug.WriteLine(File.FileName);
                //check to see if there's a file attached, then read the file
                if (File != null)
                {//get string
                    string ext = Path.GetExtension(File.FileName);

                    //Check for the type of upload see if it's empty, or not the correct type of images.
                    if (String.IsNullOrEmpty(ext) == false &&
                 (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        System.Diagnostics.Debug.WriteLine("storing into blob");
                        //Storing the image into the BLOB and getting the URI string from the BLOB to display image later.
                        FileUri = BlobManager.uploadAndGetImageBLOBURI(File);
                        //Storing the SortedList object returned which contains all Centers
                        System.Diagnostics.Debug.WriteLine("FileUri " + FileUri);
                    }
                }


                var newsArticle = new NewsArticle()
                {
                    ArticleTitle = model.ArticleTitle,
                    ArticleBody = model.ArticleBody,
                    DateOfArticle = DateTime.Now,
                    CreatedByUser = MyUserManager.GetUser().ID,
                    Attach1URL = FileUri
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
        [ValidateUser]
        public ActionResult EditNews(int id)
        {
            NewsArticle na = new NewsArticle();
            AddNewsViewModel model = new AddNewsViewModel();
            //Getting User information based on User ID
            na = SQLManager.sqlConnectionGetNews(id);
            //Storing the information in ViewData to be used to fill in the Edit form
            model.ArticleID = na.ArticleID;
            model.ArticleTitle = na.ArticleTitle;
            model.ArticleBody = na.ArticleBody;
            model.DateOfArticle = na.DateOfArticle;
            return View(model);
        }
        [HttpPost]
        [ValidateUser]
        public ActionResult EditNews(AddNewsViewModel anvm)
        {
            if (ModelState.IsValid)
            {
                var newsArticle = new NewsArticle()
                {
                    ArticleID = anvm.ArticleID,
                    ArticleTitle = anvm.ArticleTitle,
                    ArticleBody = anvm.ArticleBody,
                    DateOfArticle = DateTime.Now,
                    CreatedByUser = MyUserManager.GetUser().ID
                };

                Boolean success = false;

                success = SQLManager.sqlConnectionEditNews(newsArticle);
                if (success == true)
                {
                    TempData["Message"] = "News Article Updated Successfully.";
                }
                else
                {
                    TempData["Message"] = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }
                return RedirectToAction("SelectNews");
            }

            return View(anvm);
        }
        [ValidateUser]
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
        [ValidateUser]
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
        [HttpGet]
        [ValidateUser]
        public ActionResult ConfirmDeleteNews(int id, string title)
        {
            ConfirmDeleteNewsViewModel cdvnm = new ConfirmDeleteNewsViewModel()
            {
                ArticleID = id,
                ArticleTitle = title
            };
            return View(cdvnm);
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult ConfirmDeleteNews(ConfirmDeleteNewsViewModel cdnvm, string button)
        {
            if (button.Contains("delete"))
            {
                return RedirectToAction("DeleteNews", "News", new { id = cdnvm.ArticleID });
            }

            return RedirectToAction("SelectNews");

        }

    }
}