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

        #region Manage News

        [ValidateUser]
        public ActionResult ManageNews()
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

        #endregion

        #region Add News

        //This method returns the AddNews View
        [HttpGet]
        [ValidateUser]
        public ActionResult AddNews()
        {
            return View();
        }
        //This method adds a news article with provided information to the SQL database and file attachment to the BLOB
        //and redirects user to ManageNews with a success or fail message.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddNews(AddNewsViewModel model, HttpPostedFileBase File)
        {
            Boolean success = false;
            if (ModelState.IsValid)
            {
                String FileUri = "";
                //Check to see if there's a file attached, then read the file
                if (File != null)
                {
                    //Getting file extension
                    string ext = Path.GetExtension(File.FileName);
                    //Check for the type of upload see if it's empty, or not the correct type of files
                    //such as '.png', '.jpg' or '.pdf'.
                    if (String.IsNullOrEmpty(ext) == false &&
                    (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        //Storing the image into the BLOB and getting the URI string from the BLOB to display image later.
                        FileUri = BlobManager.uploadAndGetImageBLOBURI(File);
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
                success = SQLManager.sqlConnectionAddNews(newsArticle);
                if (success == true)
                {
                    TempData["Message"] = "Successfully created news article.";
                }
                else
                {
                    TempData["Message"] = "Database error. Could not create news article. Please try again and if the problem persists, contact the Administrator.";
                }
                return RedirectToAction("ManageNews");
            }
            return View(model);
        }

        #endregion

        #region Edit News

        //This method returns the EditNews View with the AddAccount View Model passed in to display news article information.
        [HttpGet]
        [ValidateUser]
        public ActionResult EditNews(int id)
        {
            NewsArticle na = new NewsArticle();
            AddNewsViewModel model = new AddNewsViewModel();
            //Getting News Article information based on ID
            na = SQLManager.sqlConnectionGetNews(id);
            //Storing the information in ViewData to be used to fill in the Edit form
            model.ArticleID = na.ArticleID;
            model.ArticleTitle = na.ArticleTitle;
            model.ArticleBody = na.ArticleBody;
            model.DateOfArticle = na.DateOfArticle;
            model.Attach1URL = na.Attach1URL;
            model.fileName = model.Attach1URL.Split('/').Last();
            return View(model);
        }

        //This method allows a authorized User to edit news articles displayed in Manage News and saves changes in the 
        //SQL database and BLOB.
        [HttpPost]
        [ValidateUser]
        public ActionResult EditNews(AddNewsViewModel anvm, HttpPostedFileBase File)
        {
            Boolean success = false;
            if (ModelState.IsValid)
            {
                //If there's a file to attach, replace the old file.
                //That way, even if the name is the same, it might be an updated version
                NewsArticle NV = SQLManager.sqlConnectionGetNews(anvm.ArticleID);
                String FileUri = NV.Attach1URL;
                //check to see if there's a file attached, then read the file
                if (File != null)
                {
                    //Getting file extension
                    string ext = Path.GetExtension(File.FileName);
                    //Check for the type of upload see if it's empty, or not the correct type of images.
                    if (String.IsNullOrEmpty(ext) == false &&
                    (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        //If the old news article already has a blob file attached, delete the file from the BLOB.
                        if (String.IsNullOrEmpty(FileUri) == false)
                        {
                            BlobManager.deleteBlob(FileUri);
                        }
                        //Storing the new file into the BLOB and getting the URI string from the BLOB.
                        FileUri = BlobManager.uploadAndGetImageBLOBURI(File);
                    }
                }
                var newsArticle = new NewsArticle()
                {
                    ArticleID = anvm.ArticleID,
                    ArticleTitle = anvm.ArticleTitle,
                    ArticleBody = anvm.ArticleBody,
                    DateOfArticle = DateTime.Now,
                    CreatedByUser = MyUserManager.GetUser().ID,
                    Attach1URL = FileUri
                };
                success = SQLManager.sqlConnectionEditNews(newsArticle);
                if (success == true)
                {
                    TempData["Message"] = "Successfully updated news article.";
                }
                else
                {
                    TempData["Message"] = "Database error. Could not update news article. Please try again and if the problem persists, contact the Administrator.";
                }
                return RedirectToAction("ManageNews");
            }
            return View(anvm);
        }

        #endregion

        #region Delete News

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
            return RedirectToAction("ManageNews");
        }

        [ValidateUser]
        public ActionResult DeleteNews(int id)
        {
            Boolean success = false;
            NewsArticle nA = SQLManager.sqlConnectionGetNews(id);
            string fileToDelete = nA.Attach1URL;
            success = SQLManager.sqlConnectionDeleteNews(id);
            if (success == true)
            {
                //If the news article has a file attachment, then delete the file from the BLOB storage.
                if (String.IsNullOrEmpty(fileToDelete) == false)
                {
                    BlobManager.deleteBlob(fileToDelete);
                }
                TempData["Message"] = "Successfully deleted news posting.";
            }
            else
            {
                TempData["Message"] = "Database error. Could not delete news article. Please try again and if the problem persists, contact the Administrator.";
            }
            return RedirectToAction("ManageNews");
        }

        #endregion
    }
}