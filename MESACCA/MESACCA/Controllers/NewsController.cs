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
            snvm.Articles = SQLManager.sqlConnectionForNewsListForUser();
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
            String FileUri = "";
            if (ModelState.IsValid)
            {
                //If there is text for the article body, then allow progress. If the article body is empty,
                //prevent progress. The View Model message will appear.
                if (String.IsNullOrEmpty(model.ArticleBody) == false)
                { 
                    //Check to see if there's a file attached, then read the file
                    if (File != null)
                    {
                        //Getting file extension
                        string ext = Path.GetExtension(File.FileName);
                        //Check for the type of upload see if it's empty, or not the correct type of files
                        //such as '.png', '.jpg' or '.pdf'. Prevent progress and give message otherwise.
                        if (String.IsNullOrEmpty(ext) == false &&
                        (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true))
                        {
                            //Check if the User provides a file with spaces or '/'s in it. This is to prevent problems with the BLOB and image deletion.
                            //If any are found, prevent progress and give a message. Otherwise delete the old file from the BLOB, save the new file in the BLOB,
                            //and save changes in the article to the SQL database.
                            if (File.FileName.Contains(" ") == false && File.FileName.Contains("/") == false)
                            {
                                //Storing the image into the BLOB and getting the URI string from the BLOB to display image later.
                                FileUri = BlobManager.uploadAndGetImageBLOBURI(File);
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
                                    return RedirectToAction("ManageNews");
                                }
                                else
                                {
                                    TempData["Message"] = "Database error. Could not create news article. Please try again and if the problem persists, contact the Administrator.";
                                    return RedirectToAction("ManageNews");
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Please provide a '.jpg', '.png' or '.pdf' type file with no spaces or '/'s in the name.";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please provide a '.jpg', '.png' or '.pdf' type file.";
                        }
                    }
                    //If a file is not given, then simply create an article with an empty file attachment which can be changed later.
                    else
                    {
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
                            return RedirectToAction("ManageNews");
                        }
                        else
                        {
                            TempData["Message"] = "Database error. Could not create news article. Please try again and if the problem persists, contact the Administrator.";
                            return RedirectToAction("ManageNews");
                        }
                    }
                }
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
            Boolean success = false;
            NewsArticle na = new NewsArticle();
            AddNewsViewModel model = new AddNewsViewModel();
            //Getting News Article information based on ID
            na = SQLManager.sqlConnectionGetNews(id);
            //Checking if the returned news article object has null values which occurs if there is a problem with a SQL database.
            //If center object has a null value like ArticleTitle, then display an error message and hide the save button in the View.
            if (String.IsNullOrEmpty(na.ArticleTitle) == false)
            {
                success = true;
                //Storing the information in ViewModel to be used to fill in the Edit form.
                model.ArticleID = na.ArticleID;
                model.ArticleTitle = na.ArticleTitle;
                model.ArticleBody = na.ArticleBody;
                model.DateOfArticle = na.DateOfArticle;
                model.Attach1URL = na.Attach1URL;
                //To display file name attached to the news article.
                model.FileName = model.Attach1URL.Split('/').Last();
                //For possibly deleting the current attached file from the BLOB without an extra SQL call in the POST method.
                model.CurrentAttachedFile = model.Attach1URL;
            }
            else
            {
                ViewBag.Message = "Database error. Could not load news article. Please refresh the page and if the problem persists, contact the Administrator.";
            }
            //Passing success value into the View. If the NewsArticle could not be found, the 'Save' button will be hidden to prevent the User from
            //possibly updating the NewsArticle anyway.
            ViewData["success"] = success;
            return View(model);
        }

        //This method allows an authorized User to edit news articles displayed in Manage News and saves changes in the 
        //SQL database and BLOB.
        [HttpPost]
        [ValidateUser]
        public ActionResult EditNews(AddNewsViewModel anvm, HttpPostedFileBase File)
        {
            Boolean success = false;
            //If there's a file to attach, replace the old file.
            //That way, even if the name is the same, it might be an updated version
            String FileUri = anvm.CurrentAttachedFile;
            if (ModelState.IsValid)
            {
                //If there is text for the article body, then allow progress. If the article body is empty,
                //prevent progress. The View Model validation message will appear.
                if (String.IsNullOrEmpty(anvm.ArticleBody) == false)
                {
                    //check to see if there's a file attached, then read the file
                    if (File != null)
                    {
                        //Getting file extension
                        string ext = Path.GetExtension(File.FileName);
                        //Check for the type of upload see if it's empty, or not the correct type of images. Prevent progress
                        //and give message otherwise.
                        if (String.IsNullOrEmpty(ext) == false &&
                        (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) == true))
                        {
                            //Check if the User provides a file with spaces or '/'s in it. This is to prevent problems with the BLOB and image deletion.
                            //If any are found, prevent progress and give a message. Otherwise delete the old file from the BLOB, save the new file in the BLOB,
                            //and save changes in the article to the SQL database.
                            if (File.FileName.Contains(" ") == false && File.FileName.Contains("/") == false)
                            {
                                //If the old news article already has a blob file attached, delete the file from the BLOB.
                                if (String.IsNullOrEmpty(FileUri) == false)
                                {
                                    BlobManager.deleteBlob(FileUri);
                                }
                                //Storing the new file into the BLOB and getting the URI string from the BLOB.
                                FileUri = BlobManager.uploadAndGetImageBLOBURI(File);
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
                                    return RedirectToAction("ManageNews");
                                }
                                else
                                {
                                    TempData["Message"] = "Database error. Could not update news article. Please try again and if the problem persists, contact the Administrator.";
                                    return RedirectToAction("ManageNews");
                                }
                            }
                            //If the file has spaces or /'s in the name
                            else
                            {
                                //Renewing the current attached file display. The current attached file name will disappear
                                //from the View if this is not done.
                                //Check first if the current attached file passed in the view is null or empty.
                                //If not, then simply assign the FileName the substring of the current attached file.
                                if (String.IsNullOrEmpty(FileUri) == false)
                                {
                                    anvm.FileName = anvm.CurrentAttachedFile.Split('/').Last();
                                }
                                ViewBag.Message = "Please provide a '.jpg', '.png' or '.pdf' type file with no spaces or '/'s in the name.";
                            }
                        }
                        //If the provided has the incorrect file type
                        else
                        {
                            //Renewing the current attached file display. The current attached file name will disappear
                            //from the View if this is not done.
                            //Check first if the current attached file passed in the view is null or empty.
                            //If not, then simply assign the FileName the substring of the current attached file.
                            if (String.IsNullOrEmpty(FileUri) == false)
                            { 
                                anvm.FileName = anvm.CurrentAttachedFile.Split('/').Last();
                            }
                            ViewBag.Message = "Please provide a '.jpg', '.png' or '.pdf' type file.";
                        }
                    }
                    //If a file is not provided, then update the article with the provided information and keep the current attached file.
                    else
                    {
                        NewsArticle newsArticle = new NewsArticle();
                        newsArticle.ArticleID = anvm.ArticleID;
                        newsArticle.ArticleTitle = anvm.ArticleTitle;
                        newsArticle.ArticleBody = anvm.ArticleBody;
                        newsArticle.DateOfArticle = DateTime.Now;
                        newsArticle.CreatedByUser = MyUserManager.GetUser().ID;
                        //If the attached file is not null, then simply assign the value to the model.
                        if (String.IsNullOrEmpty(FileUri) == false)
                        {
                            newsArticle.Attach1URL = FileUri;
                        }
                        //If the attached file is null, then give it a space otherwise there will be SQL issues.
                        else
                        {
                            newsArticle.Attach1URL = " ";
                        }
                        success = SQLManager.sqlConnectionEditNews(newsArticle);
                        if (success == true)
                        {
                            TempData["Message"] = "Successfully updated news article.";
                            return RedirectToAction("ManageNews");
                        }
                        else
                        {
                            TempData["Message"] = "Database error. Could not update news article. Please try again and if the problem persists, contact the Administrator.";
                            return RedirectToAction("ManageNews");
                        }
                    }
                }
            }
            //To make the submit button appear for the User to try again in the event of an error.
            ViewData["success"] = true;
            return View(anvm);
        }

        #endregion

        #region Delete News

        //This method returns the ConfirmedDeleteNews View with the ConfirmedDeleteNews View Model passed in to display news article information.
        [HttpGet]
        [ValidateUser]
        public ActionResult ConfirmDeleteNews(int id, string title, string body, string attachedFile)
        {
            ConfirmDeleteNewsViewModel cdvnm = new ConfirmDeleteNewsViewModel();
            cdvnm.ArticleID = id;
            cdvnm.ArticleTitle = title;
            cdvnm.ArticleBody = body;
            if (attachedFile != null)
            {
                cdvnm.Attach1URL = attachedFile;
                cdvnm.FileName = attachedFile.Split('/').Last();
            }
            return View(cdvnm);
        }

        //This method calls the DeleteNews function and passed relevant information to it if the Delete button is clicked on. 
        [HttpPost]
        [ValidateUser]
        public ActionResult ConfirmDeleteNews(ConfirmDeleteNewsViewModel cdnvm, string button)
        {
            if (button.Contains("delete"))
            {
                return RedirectToAction("DeleteNews", "News", new { id = cdnvm.ArticleID, attachedFile = cdnvm.Attach1URL});
            }
            return RedirectToAction("ManageNews");
        }

        //This method delete the news article from the SQL database and the possibly attached file from the BLOB.
        [ValidateUser]
        public ActionResult DeleteNews(int id, String attachedFile)
        {
            Boolean success = false;
            string fileToDelete = attachedFile;
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