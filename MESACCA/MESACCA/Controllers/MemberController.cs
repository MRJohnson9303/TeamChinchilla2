using System;
using System.IO;
using System.Collections.Generic;
using MESACCA.Models;
using S = System.Data.SqlClient;
using T = System.Threading;
using MESACCA.ViewModels.Member;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using MESACCA.FilterAttributes;
using MESACCA.DataBaseManagers;
using MESACCA.Utilities;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types


namespace MESACCA.Controllers
{
    public class MemberController : Controller
    {
        //Used for personal account management
        private static int adminID;
        private static Users User;
        //Used to sort account list for directors as well as for account creation
        private static String userAccountType;
        private static String center;
        BlobService blobService = new BlobService();
        //This method is a simple hello to the user when he or she signs in as well as saving the ID for personal account
        //management
        [ValidateUser]
        public ActionResult Index() //int ID, String firstName, String lastName
        {
            User = MyUserManager.GetUser();

            adminID = User.ID;
            userAccountType = User.AccountType;
            center = User.Center;
            ViewData["ID"] = User.ID;
            ViewData["firstName"] = User.FirstName;
            ViewData["lastName"] = User.LastName;

            return View();
        }

        #region ManageAccounts

        //This method collects all User accounts and passes them into the View to be displayed in Manage Accounts
        [ValidateUser]
        [HttpGet]
        public ActionResult ManageAccounts()
        {
            List<User> foundUserList = new List<User>();
            List<User> userList = new List<User>();
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Message");
            }
            //Storing the List object returned which contains all Users
            foundUserList = SQLManager.sqlConnectionForUsersList();
            //If the User is a Director, allow only Users who are Staff and in the Director's Center to be in the list
            if (userAccountType.Equals("Director"))
            {
                foreach (User member in foundUserList)
                {
                    if (member.AccountType.Equals("Staff") && member.Center.Equals(center))
                    {
                        userList.Add(member);
                    }
                }
                foundUserList = userList;
            }
            else
            {
                //Sorting the User list by account type and passing it into the View.
                foundUserList.Sort(delegate (User x, User y)
                {
                    return x.AccountType.CompareTo(y.AccountType);
                });
            }
            return View(foundUserList);
        }

        [ValidateUser]
        [HttpPost]
        public ActionResult ManageAccounts(String button, String name)
        {
            List<User> foundUserList = new List<User>();
            List<User> userList = new List<User>();
            List<User> searchList = new List<User>();
            //Storing the List object returned which contains all Users
            foundUserList = SQLManager.sqlConnectionForUsersList();
            //If the User is a Director, allow only Users who are Staff and in the Director's Center to be in the list
            if (userAccountType.Equals("Director"))
            {
                foreach (User member in foundUserList)
                {
                    if (member.AccountType.Equals("Staff") && member.Center.Equals(center))
                    {
                        userList.Add(member);
                    }
                }
                foundUserList = userList;
            }
            if (button.Equals("Add Account"))
            {
                return RedirectToAction(nameof(MemberController.AddAccount));
            }
            //If the search text bar passes a value, then a list is created and passed into the view containing
            //users whose names contain the given search input
            if (String.IsNullOrEmpty(name) == false)
            {
                foreach (User member in foundUserList)
                {
                    String username = member.FirstName + " " + member.LastName;
                    if (username.Contains(name))
                    {
                        searchList.Add(member);
                    }
                }
                foundUserList = searchList;
            }
            //If there is no value passed in the search text bar, sorting the list of Users is based on given sort 
            //button clicks with the list being ordered by account type by default 
            if (String.IsNullOrEmpty(name) == true)
            {
                switch (button)
                {
                    case "First Name":
                        {
                            foundUserList.Sort(delegate (User x, User y)
                            {
                                return x.FirstName.CompareTo(y.FirstName);
                            });
                            break;
                        }
                    case "Last Name":
                        {
                            foundUserList.Sort(delegate (User x, User y)
                            {
                                return x.LastName.CompareTo(y.LastName);
                            });
                            break;
                        }
                    default:
                        {
                            foundUserList.Sort(delegate (User x, User y)
                            {
                                return x.AccountType.CompareTo(y.AccountType);
                            });
                            break;
                        }
                }
            }
            return View(foundUserList);
        }

        #endregion

        #region Add Accounts

        //This method returns the AddAccount View
        [HttpGet]
        [ValidateUser]
        public ActionResult AddAccount()
        {
            if (userAccountType.Equals("Director"))
            {
                return RedirectToAction("AddStaffAccount", new { firstName = "", lastName = "", accountType = "Staff" });
            }
            return View();
        }

        //This method sends the Admin to the AddDirectorAccount or AddStaffAccount View based on the account type selected
        //in the provided dropbox along with the information in the First Name and Last Name fields.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddAccount(AddAccountViewModel model)
        {
            if (model.AccountType.Equals("Director"))
            {
                return RedirectToAction("AddDirectorAccount", new { model.FirstName, model.LastName, model.AccountType });
            }
            else if (model.AccountType.Equals("Staff"))
            {
                return RedirectToAction("AddStaffAccount", new { model.FirstName, model.LastName, model.AccountType });
            }
            return View();
        }

        //This method returns the AddDirectorAccount View with the first name, last name and account type filled in using
        //passed in information, but the account type textfield is readonly.
        [HttpGet]
        [ValidateUser]
        public ActionResult AddDirectorAccount(string firstName, string lastName, string accountType)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            AddAccountViewModel model = new AddAccountViewModel();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            model.FirstName = firstName;
            model.LastName = lastName;
            model.AccountType = accountType;
            model.Home = true;
            model.About_Us = true;
            model.Collaborations = true;
            model.MESA_Schools_Program = true;
            model.MESA_Community_College_Program = true;
            model.MESA_Engineering_Program = true;
            model.News = true;
            model.Donate = true;
            //This is to populate the Centers dropbox in the View.
            centerList = SQLManager.sqlConnectionForCentersList();
            errorCenterList = ErrorCenterListCheck(centerList);
            if (errorCenterList == false)
            {
                success = true;
                //Storing all center names into the SelectListItem List.
                foreach (var item in centerList)
                {
                    centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                }
                //Passing the items inside a SelectList into the View using ViewBag.
                ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
            }
            else
            {
                ViewBag.Message = "Database error. Could not load center list for account creation. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            ViewData["success"] = success;
            return View(model);
        }

        //This method adds a Director account with provided information to the SQL database and redirects user to ManageAccounts
        //with a success or fail message if all checks pass such as the username check.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddDirectorAccount(AddAccountViewModel model)
        {
            Boolean errorUserList = false;
            Boolean errorCenterList = false;
            Boolean success = false;
            User newUser = new User();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            //ID initialized for comparison
            int ID = 1;
            //userNameFound initialized for comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            //Storing the List object returned which contains all Users
            userList = SQLManager.sqlConnectionForUsersList();
            errorUserList = ErrorUserListCheck(userList);
            if (errorUserList == false)
            {
                //ID is compared with the ID value of all Users and is incremented by 1 in each loop. If ID doesn't match
                //a User ID then break the loop and use the new ID value for the new User account ID.
                //This means if a User is deleted, then a new User will get the old ID
                foreach (var item in userList)
                {
                    if (ID != item.ID)
                    {
                        break;
                    }
                    ID += 1;
                }
                newUser.FirstName = model.FirstName;
                newUser.LastName = model.LastName;
                newUser.AccountType = model.AccountType;
                newUser.Center = model.Center;
                newUser.Email = model.Email;
                newUser.PhoneNumber = model.PhoneNumber;
                newUser.Username = model.Username;
                newUser.Password = model.Password;
                newUser.Home = "True";
                newUser.About_Us = "True";
                newUser.MESA_Schools_Program = "True";
                newUser.MESA_Community_College_Program = "True";
                newUser.MESA_Engineering_Program = "True";
                newUser.News = "True";
                newUser.Donate = "True";
                newUser.Collaborations = "True";
                //Before creating an account all usernames are compared to the provided username. If there is a match,
                //then userNameNotFound becomes true.
                userNameFound = UserNameCheck(userList, newUser);
                //If a username in the database matches the provided username, then provide an error message.
                //If a username in the database does not match the provided username, then create an account.
                if (userNameFound == false)
                {
                    success = SQLManager.sqlConnectionAddUser(ID, newUser);
                    if (success == true)
                    {
                        TempData["Message"] = "Successfully created director account.";
                        return RedirectToAction("ManageAccounts");
                    }
                    else
                    {
                        TempData["Message"] = "Database error. Could not create director account. Please try again and if the problem persists, contact the Administrator.";
                        return RedirectToAction("ManageAccounts");
                    }
                }
                else
                {
                    //This is to populate the Centers dropbox in the View.
                    centerList = SQLManager.sqlConnectionForCentersList();
                    errorCenterList = ErrorCenterListCheck(centerList);
                    if (errorCenterList == false)
                    {
                        success = true;
                        //Storing all center names into the SelectListItem List.
                        foreach (var item in centerList)
                        {
                            centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                        }
                        //Passing the items inside a SelectList into the View using ViewBag.
                        ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
                        ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
                    }
                    //If errorCenterList is true.
                    else
                    {
                        TempData["Message"] = "Database error. The Username provided is currently in use and could not reload center list for account creation. Please try again and if the problem persists, contact the Administrator.";
                        return RedirectToAction("ManageAccounts");
                    } 
                }
            }
            //If errorUserList is true.
            else
            {
                TempData["Message"] = "Database error. Could not load user list for username comparison. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageAccounts");
            }
            //To make the submit button appear for the Admin to try again in the event of an error.
            ViewData["success"] = true;
            return View(model);
        }

        //This method returns the AddStaffAccount View with the first name, last name and account type filled in using
        //passed in information, but the account type textfield is readonly.
        [HttpGet]
        [ValidateUser]
        public ActionResult AddStaffAccount(string firstName, string lastName, string accountType)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            AddAccountViewModel model = new AddAccountViewModel();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            model.FirstName = firstName;
            model.LastName = lastName;
            model.AccountType = accountType;
            model.Home = true;
            model.About_Us = true;
            model.Collaborations = true;
            model.MESA_Schools_Program = true;
            model.MESA_Community_College_Program = true;
            model.MESA_Engineering_Program = true;
            model.News = true;
            model.Donate = true;
            //Passing User's account type into the View using ViewData for comparisons.
            //If the creator is an Admin, then use the list of centers.
            //If the creator is a Director, then allow only the Director's centers in the Center field.
            ViewData["CreatorAccountType"] = userAccountType;
            if (userAccountType.Equals("Director"))
            {
                //Giving model.Center the Director's global center value
                model.Center = center;
            }
            //This is to populate the Centers dropbox in the View.
            centerList = SQLManager.sqlConnectionForCentersList();
            errorCenterList = ErrorCenterListCheck(centerList);
            if (errorCenterList == false)
            {
                success = true;
                //Storing all center names into the SelectListItem List.
                foreach (var item in centerList)
                {
                    centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                }
                //Passing the items inside a SelectList into the View using ViewBag.
                ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
            }
            else
            {
                ViewBag.Message = "Database error. Could not load center list for account creation. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            ViewData["success"] = success;
            return View(model);
        }

        //This method adds a Staff account with provided information to the SQL database and redirects user to ManageAccounts
        //with a success or fail message if all checks pass such as the username check.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddStaffAccount(AddAccountViewModel model)
        {
            Boolean errorCenterList = false;
            Boolean errorUserList = false;
            Boolean success = false;
            //userNameNoFound initialized for comparison
            Boolean userNameFound = false;
            User newUser = new User();
            //ID initialized for comparison
            int ID = 1;
            List<User> userList = new List<User>();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            //Storing the List object returned which contains all Users
            userList = SQLManager.sqlConnectionForUsersList();
            errorUserList = ErrorUserListCheck(userList);
            if (errorUserList == false)
            {
                //ID is compared with the ID value of all Users and is incremented by 1 in each loop. If ID doesn't match
                //a User ID then break the loop and use the new ID value for the new User account ID.
                //This means if a User is deleted, then a new User will get the old ID
                foreach (var item in userList)
                {
                    if (ID != item.ID)
                    {
                        break;
                    }
                    ID += 1;
                }
                newUser.FirstName = model.FirstName;
                newUser.LastName = model.LastName;
                newUser.AccountType = model.AccountType;
                newUser.Center = model.Center;
                newUser.Email = model.Email;
                newUser.PhoneNumber = model.PhoneNumber;
                newUser.Username = model.Username;
                newUser.Password = model.Password;
                newUser.Home = model.Home.ToString();
                newUser.About_Us = model.About_Us.ToString();
                newUser.MESA_Schools_Program = model.MESA_Schools_Program.ToString();
                newUser.MESA_Community_College_Program = model.MESA_Community_College_Program.ToString();
                newUser.MESA_Engineering_Program = model.MESA_Engineering_Program.ToString();
                newUser.News = model.News.ToString();
                newUser.Donate = model.Donate.ToString();
                newUser.Collaborations = model.Collaborations.ToString();
                ViewData["CreatorAccountType"] = userAccountType;
                //Before creating an account all usernames are compared to the provided username. If there is a match,
                //then userNameNotFound becomes true.
                userNameFound = UserNameCheck(userList, newUser);
                //If a username in the database matches the provided username, then provide an error message.
                //If a username in the database does not match the provided username, then create an account.
                if (userNameFound == false)
                {
                    success = SQLManager.sqlConnectionAddUser(ID, newUser);
                    if (success == true)
                    {
                        TempData["Message"] = "Successfully created staff account.";
                        return RedirectToAction("ManageAccounts");
                    }
                    else
                    {
                        TempData["Message"] = "Database error. Could not create staff account. Please try again and if the problem persists, contact the Administrator.";
                        return RedirectToAction("ManageAccounts");
                    }
                }
                else
                {
                    ViewData["CreatorAccountType"] = userAccountType;
                    if (userAccountType.Equals("Director"))
                    {
                        //Giving model.Center the Director's global center value
                        model.Center = center;
                    }
                    //This is to populate the Centers dropbox in the View.
                    centerList = SQLManager.sqlConnectionForCentersList();
                    errorCenterList = ErrorCenterListCheck(centerList);
                    if (errorCenterList == false)
                    {
                        success = true;
                        //Storing all center names into the SelectListItem List.
                        foreach (var item in centerList)
                        {
                            centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                        }
                        //Passing the items inside a SelectList into the View using ViewBag.
                        ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
                        ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
                    }
                    //If errorCenterList is true.
                    else
                    {
                        TempData["Message"] = "Database error. The Username provided is currently in use and could not reload center list for account creation. Please try again and if the problem persists, contact the Administrator.";
                        return RedirectToAction("ManageAccounts");
                    }
                }
            }
            //If errorUserList is true.
            else
            {
                TempData["Message"] = "Database error. Could not load user list for username comparison. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageAccounts");
            }
            //To make the submit button appear for the Admin or Director to try again in the event of an error.
            ViewData["success"] = true;
            return View(model);
        }

        #endregion

        #region EditAccounts

        //This method returns the EditAccount View with the EditAccount View Model passed in to display account information.
        [HttpGet]
        [ValidateUser]
        public ActionResult EditAccount(int ID)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            User foundUser = new User();
            EditAccountViewModel model = new EditAccountViewModel();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            //Getting User information based on User ID
            foundUser = SQLManager.sqlConnectionForUser(ID);
            if (foundUser.FirstName != null)
            {
                success = true;
                //Storing the information in ViewData to be used to fill in the Edit form
                model.ID = foundUser.ID;
                model.FirstName = foundUser.FirstName;
                model.LastName = foundUser.LastName;
                model.AccountType = foundUser.AccountType;
                model.Center = foundUser.Center;
                model.Email = foundUser.Email;
                model.PhoneNumber = foundUser.PhoneNumber;
                model.Username = foundUser.Username;
                //The following three fields for models will be used for comparisons in the POST method.
                model.CurrentCenter = foundUser.Center;
                model.CurrentUsername = foundUser.Username;
                model.CurrentPassword = foundUser.Password;
                model.Home = Convert.ToBoolean(foundUser.Home);
                model.About_Us = Convert.ToBoolean(foundUser.About_Us);
                model.Collaborations = Convert.ToBoolean(foundUser.Collaborations);
                model.MESA_Schools_Program = Convert.ToBoolean(foundUser.MESA_Schools_Program);
                model.MESA_Community_College_Program = Convert.ToBoolean(foundUser.MESA_Community_College_Program);
                model.MESA_Engineering_Program = Convert.ToBoolean(foundUser.MESA_Engineering_Program);
                model.News = Convert.ToBoolean(foundUser.News);
                model.Donate = Convert.ToBoolean(foundUser.Donate);
                //Passing the User's account type into the View for comparison.
                //This will cause a change in the form such as an Admin given a dropbox for Centers and a Director,
                //will be given a readonly Center textbox.
                ViewData["EditorAccountType"] = userAccountType;
                //This is to populate the Centers dropbox in the View.
                centerList = SQLManager.sqlConnectionForCentersList();
                errorCenterList = ErrorCenterListCheck(centerList);
                if (errorCenterList == false)
                {
                    success = true;
                    //Storing the edited User's center in the list first, so it appears first on the list in the View.
                    centerNamesListItems.Add(new SelectListItem { Text = foundUser.Center, Value = foundUser.Center });
                    //Storing all other center names into the SelectListItem List.
                    foreach (var item in centerList)
                    {
                        if (item.Name.Equals(model.Center) != true)
                        {
                            centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                        }
                    }
                    //Passing the items inside a SelectList into the View using ViewBag.
                    ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
                }
                //If errorCenterList is true.
                else
                {
                    success = false;
                    ViewBag.Message = "Database error. Could not load center list. Please refresh the page. If the problem persists, contact the Administrator.";
                }
            }
            //If errorUserList is true.
            else
            {
                success = false;
                ViewBag.Message = "Database error. Could not load user information. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            ViewData["success"] = success;
            return View(model);
        }

        //This method allows the Admin or a Director to saves changes in the SQL database and redirects them to
        //ManageAccounts with a success or fail message if all checks pass such as the username check.
        [HttpPost]
        [ValidateUser]
        public ActionResult EditAccount(EditAccountViewModel model)
        {
            Boolean success = false;
            Boolean errorUserList = false;
            Boolean errorCenterList = false;
            //userNameFound and userList is for username comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            List<Models.Center> centerList = new List<Models.Center>();
            List<SelectListItem> centerNamesListItems = new List<SelectListItem>();
            User updatedUser = new User();
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page
            updatedUser.FirstName = model.FirstName;
            updatedUser.LastName = model.LastName;
            updatedUser.AccountType = model.AccountType;
            updatedUser.Center = model.Center;
            updatedUser.Email = model.Email;
            updatedUser.PhoneNumber = model.PhoneNumber;
            updatedUser.Username = model.Username;
            //If the Admin or Director decides not to update a User's password, then the current stored password is stored in 
            //updatedUser to be pushed into the database. Otherwise the new given password is stored to be pushed
            //into the database.
            if (String.IsNullOrEmpty(model.Password) == true)
            {
                updatedUser.Password = model.CurrentPassword;
            }
            else
            {
                updatedUser.Password = model.Password;
            }
            //If the account being edited is a Director, 
            //manually set the Director's rights to True because the disabled checkboxes return False.
            if (model.AccountType.Equals("Director"))
            {
                updatedUser.Home = "True";
                updatedUser.About_Us = "True";
                updatedUser.Collaborations = "True";
                updatedUser.MESA_Schools_Program = "True";
                updatedUser.MESA_Community_College_Program = "True";
                updatedUser.MESA_Engineering_Program = "True";
                updatedUser.News = "True";
                updatedUser.Donate = "True";
            }
            //If the account being edited is a Staff member, 
            //set the user's rights with the provided checkboxes returns.
            else if (model.AccountType.Equals("Staff"))
            {
                updatedUser.Home = model.Home.ToString();
                updatedUser.About_Us = model.About_Us.ToString();
                updatedUser.Collaborations = model.Collaborations.ToString();
                updatedUser.MESA_Schools_Program = model.MESA_Schools_Program.ToString();
                updatedUser.MESA_Community_College_Program = model.MESA_Community_College_Program.ToString();
                updatedUser.MESA_Engineering_Program = model.MESA_Engineering_Program.ToString();
                updatedUser.News = model.News.ToString();
                updatedUser.Donate = model.Donate.ToString();
            }
            //Storing the List object returned which contains all Users for username comparison.
            userList = SQLManager.sqlConnectionForUsersList();
            errorUserList = ErrorUserListCheck(userList);
            if(errorUserList == false)
            { 
                //Before creating an account all usernames are compared to the provided username. If there is a match,
                //then userNameNotFound becomes true.
                userNameFound = UserNameCheck(userList, updatedUser);
                //If a username in the database matches the provided username, then provide an error message.
                //If a username in the database does not match the provided username, then push new account changes to the database.
                //In the event the username is not changed, allow push of new account changes to the database.
                if (userNameFound == false || (updatedUser.Username.Equals(model.CurrentUsername) == true))
                {
                    //Getting Boolean result of SQL entry information update
                    success = SQLManager.sqlConnectionUpdateUser(model.ID, updatedUser);
                    //If the update was successful, redirect the User to the Manage Accounts page
                    if (success == true)
                    {
                        TempData["Message"] = "Successfully updated account.";
                        return RedirectToAction("ManageAccounts");
                    }
                    else
                    {
                        TempData["Message"] = "Database error. Could not update account. Please try again and if the problem persists, contact the Administrator.";
                        return RedirectToAction("ManageAccounts");
                    }
                }
                else
                {
                    //Passing the User's account type into the View for comparison.
                    //This will cause a change in the form such as an Admin given a dropbox for Centers and a Director,
                    //will be given a readonly Center textbox.
                    ViewData["EditorAccountType"] = userAccountType;
                    //This is to populate the Centers dropbox in the View.
                    centerList = SQLManager.sqlConnectionForCentersList();
                    errorCenterList = ErrorCenterListCheck(centerList);
                    if (errorCenterList == false)
                    {
                        success = true;
                        //Storing the edited User's center in the list first, so it appears first on the list in the View.
                        centerNamesListItems.Add(new SelectListItem { Text = model.CurrentCenter, Value = model.CurrentCenter });
                        //Storing all other center names into the SelectListItem List.
                        foreach (var item in centerList)
                        {
                            if (item.Name.Equals(model.CurrentCenter) != true)
                            {
                                centerNamesListItems.Add(new SelectListItem { Text = item.Name, Value = item.Name });
                            }
                        }
                        //Passing the items inside a SelectList into the View using ViewBag.
                        ViewBag.centerNamesList = new SelectList(centerNamesListItems, "Text", "Value");
                        ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
                    }
                    //If errorCenterList is true.
                    else
                    {
                        ViewBag.Message = "Database error. Could not load center list. Please refresh the page. If the problem persists, contact the Administrator.";
                    }
                }
            }
            //If errorUserList is true.
            else
            {
                TempData["Message"] = "Database error. Could not load user list for username comparison. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageAccounts");
            }
            //To make the submit button appear for the Admin to try again in the event of an error.
            ViewData["success"] = true;
            return View(model);
        }

        #endregion

        #region DeleteAccounts

        //This method returns the DeleteAccount View with the DeleteAccount View Model passed in to display account information.
        [HttpGet]
        [ValidateUser]
        public ActionResult DeleteAccount(int ID)
        {
            User foundUser = new User();
            DeleteAccountViewModel model = new DeleteAccountViewModel();
            foundUser = SQLManager.sqlConnectionForUser(ID);
            if (foundUser.FirstName != null)
            {
                model.ID = foundUser.ID;
                model.FirstName = foundUser.FirstName;
                model.LastName = foundUser.LastName;
                model.AccountType = foundUser.AccountType;
                model.Center = foundUser.Center;
                model.Email = foundUser.Email;
                model.PhoneNumber = foundUser.PhoneNumber;
                model.Username = foundUser.Username;
            }
            else
            {
                ViewBag.Message = "Database error. Could not load the account. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            return View(model);
        }

        //This method deletes the user from the system if the delete button in the DeleteAccount View is clicked on and sends the User
        //to Manage Accounts and if the back button is clicked, then the User is sent back to ManageAccounts.
        [HttpPost]
        [ValidateUser]
        public ActionResult DeleteAccount(DeleteAccountViewModel model, string button)
        {
            Boolean success = false;
            if (button.Contains("delete"))
            {
                success = SQLManager.sqlConnectionDeleteUser(model.ID);
            }
            else if (button.Contains("back"))
            {
                return RedirectToAction("ManageAccounts");
            }
            if (success == true)
            {
                TempData["Message"] = "Successfully deleted account.";
                return RedirectToAction("ManageAccounts");
            }
            else
            {
                TempData["Message"] = "Database error. Could not delete center. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageAccounts");
            }
        }

        #endregion

        #region ManageCenters

        //This method returns the ManageCenters View displaying all Centers in the database in alphabetical order.
        [HttpGet]
        [ValidateUser]
        public ActionResult ManageCenters()
        {
            List<Models.Center> centerList = new List<Models.Center>();
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
                TempData.Remove("Message");
            }
            //Storing the List object returned which contains all Centers
            centerList = SQLManager.sqlConnectionForCentersList();
            centerList.Sort(delegate (Models.Center x, Models.Center y)
            {
                return x.Name.CompareTo(y.Name);
            });
            return View(centerList);
        }

        //This method redirects the User to AddCenter, EditCenter or DeleteCenter based on input.
        [HttpPost]
        [ValidateUser]
        public ActionResult ManageCenters(String button)
        {
            List<Models.Center> centerList = new List<Models.Center>();
            //Storing the List object returned which contains all Centers
            centerList = SQLManager.sqlConnectionForCentersList();
            centerList.Sort(delegate (Models.Center x, Models.Center y)
            {
                return x.Name.CompareTo(y.Name);
            });
            //Redirecting User if the Add Center button is clicked.
            if (button.Equals("Add Center"))
            {
                return RedirectToAction("AddCenter");
            }
            else if (button.Equals("Picture Test"))
            {
                return RedirectToAction("PictureTest");
            }
            return View(centerList);
        }

        //This method returns the ManageCenters View displaying a Director's center information in the View.
        //This is for Directors.
        [HttpGet]
        [ValidateUser]
        public ActionResult ManageCenter()
        {
            Boolean success = false;
            EditCenterViewModel model = new EditCenterViewModel();
            Models.Center foundCenter = new Models.Center();
            //Getting User information based on the Director's center
            foundCenter = SQLManager.sqlConnectionForCenter(center);
            //Checking if the returned center object has null values which occurs if there is a problem with a SQL database.
            //If center object has a null value like Name, then display an error message and hide the save button in the View.
            if (foundCenter.Name != null)
            {
                success = true;
                model.ID = foundCenter.ID;
                model.Name = foundCenter.Name;
                model.Address = foundCenter.Address;
                model.Location = foundCenter.Location;
                model.CenterType = foundCenter.CenterType;
                model.DirectorName = foundCenter.DirectorName;
                model.OfficeNumber = foundCenter.OfficeNumber;
                model.URL = foundCenter.URL;
                model.Description = foundCenter.Description;
                model.ImageURL = foundCenter.ImageURL;
            }
            if (success == false)
            {
                ViewBag.Message = "Database error. Could not load center. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            //Passing success value into the View. If the Center could not be found, the 'Save' button will be hidden to prevent the User from
            //possibly updating the Center anyway.
            ViewData["success"] = success;
            return View(model);
        }

        //This saves changes in the Center fields for a Director center in the SQL database.
        [HttpPost]
        [ValidateUser]
        public ActionResult ManageCenter(EditCenterViewModel model)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            Boolean centerNameFound = false;
            Models.Center updatedCenter = new Models.Center();
            List<Models.Center> centerList = new List<Models.Center>();
            //Getting ViewModel model information given in the textfields of the Edit Center page
            updatedCenter.ID = model.ID;
            updatedCenter.Name = model.Name;
            updatedCenter.Address = model.Address;
            updatedCenter.Location = model.Location;
            updatedCenter.CenterType = model.CenterType;
            updatedCenter.DirectorName = model.DirectorName;
            updatedCenter.OfficeNumber = model.OfficeNumber;
            updatedCenter.URL = model.URL;
            updatedCenter.Description = model.Description;
            updatedCenter.ImageURL = model.ImageURL;
            //Storing the List object returned which contains all Centers
            centerList = SQLManager.sqlConnectionForCentersList();
            //If there is a problem with the SQL database, then null values will be given to values in the Center object.
            //Check for possible SQL errors using the "Name" field and if there is, prevent further progress and
            //displays an error message for the Admin or Director.
            errorCenterList = ErrorCenterListCheck(centerList);
            if (errorCenterList == false)
            {
                //Before creating a center all center names are compared to the provided center name. If there is a match,
                //then centerNameNotFound becomes true.
                centerNameFound = EditCenterNameCheck(centerList, updatedCenter);
                if (centerNameFound == false)
                {
                    //If no image is provided, then keep the current ImageURL and update the database.
                    if (model.Picture == null)
                    {
                        //Getting Boolean result of SQL entry information update
                        success = SQLManager.sqlConnectionUpdateCenter(model.ID, updatedCenter);
                        //If the update was successful, display a 'success' message for the Director.
                        if (success == true)
                        {
                            //Update the global variable used by Directors
                            center = model.Name;
                            ViewBag.Message = "Center updated successfully.";
                        }
                        else
                        {
                            ViewBag.Message = "Database error. Could not update center. Please try again and if the problem persists, contact the Administrator.";
                        }
                    }
                    //Otherwise store the provided image into the BLOB, store the new ImageURL in the Center object and update the database.
                    else
                    {
                        //Getting file extension.
                        string ext = Path.GetExtension(model.Picture.FileName);
                        //Check for the type of upload see if it's not the correct type of images.
                        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) == true ||
                            ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            //Check if the Admin provides an image with spaces or '/'s in it. This is to prevent problems with the BLOB and image deletion.
                            //If any are found, prevent progress and give a message.
                            if (model.Picture.FileName.Contains(" ") == false && model.Picture.FileName.Contains("/") == false)
                            {
                                updatedCenter.Picture = model.Picture;
                                //Deleting the old image from the BLOB.
                                BlobManager.deleteBlob(updatedCenter.ImageURL);
                                //Storing the new image into the BLOB and getting the URL.
                                updatedCenter.ImageURL = BlobManager.uploadAndGetImageBLOBURI(updatedCenter.Picture);
                                //Updating the model's ImageURL so the new proper center image appears.
                                model.ImageURL = updatedCenter.ImageURL;
                                //Getting Boolean result of SQL entry information update
                                success = SQLManager.sqlConnectionUpdateCenter(model.ID, updatedCenter);
                                //If the update was successful, display a 'success' message for the Director.
                                if (success == true)
                                {
                                    //Update the global variable used by Directors
                                    center = model.Name;
                                    ViewBag.Message = "Center was successfully updated";
                                }
                                else
                                {
                                    ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png' type image with no spaces or '/'s in the name.";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png' type image.";
                        }
                    }
                }
                //If centerNameFound is true.
                else
                {
                    ViewBag.Message = "Name error. The center name provided is currently in use. Please provide another name.";
                }
            }
            //If errorCenterList is true
            else
            {
                ViewBag.Message = "Database error. Could not load center list for name comparison. Please try again and if the problem persists, contact the Administrator.";
            }
            //To make the submit button appear for the Admin to try again in the event of an error.
            ViewData["success"] = true;
            return View(model);
        }

        #endregion

        #region AddCenters
        //This method returns the AddCenter View
        [HttpGet]
        [ValidateUser]
        public ActionResult AddCenter()
        {
            return View();
        }

        //This method adds a Center with provided information to the SQL database and redirects the Admin to ManageCenters
        //with a success or fail message if all checks pass such as the center name check.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddCenter(AddCenterViewModel model)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            Boolean centerNameFound = false;
            Models.Center newCenter = new Models.Center();
            List<Models.Center> centerList = new List<Models.Center>();
            if (model.Picture != null)
            {
                //Getting file extension.
                string ext = Path.GetExtension(model.Picture.FileName);
                //Check for the type of upload see if it's not the correct type of images.
                if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) == true ||
                    ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true)
                {
                    //Check if the Admin provides an image with spaces or '/'s in it. This is to prevent problems with the BLOB and image deletion.
                    //If any are found, prevent progress and give a message.
                    if (model.Picture.FileName.Contains(" ") == false && model.Picture.FileName.Contains("/") == false)
                    {
                        newCenter.Picture = model.Picture;
                        //Storing the image into the BLOB and getting the URI string from the BLOB to display image later.
                        newCenter.ImageURL = BlobManager.uploadAndGetImageBLOBURI(model.Picture);
                        //Storing the List object returned which contains all Centers for name comparison.
                        centerList = SQLManager.sqlConnectionForCentersList();
                        //If there is a problem with the SQL database, then null values will be given to values in the Center object.
                        //Check for possible SQL errors using the "Name" field and if there is, prevent further progress and
                        //displays an error message for the Admin.
                        errorCenterList = ErrorCenterListCheck(centerList);
                        if (errorCenterList == false)
                        {
                            newCenter.Name = model.Name;
                            newCenter.Address = model.Address;
                            newCenter.Location = model.Location;
                            newCenter.CenterType = model.CenterType;
                            newCenter.DirectorName = model.DirectorName;
                            newCenter.OfficeNumber = model.OfficeNumber;
                            newCenter.URL = model.URL;
                            newCenter.Description = model.Description;
                            //Before creating a center all center names are compared to the provided center name. If there is a match,
                            //then centerNameNotFound becomes true.
                            centerNameFound = AddCenterNameCheck(centerList, newCenter);
                            //If a center name in the database matches the provided center name, then provide an error message.
                            //If a center name in the database does not match the provided center name, then create a center.
                            if (centerNameFound == false)
                            {
                                success = SQLManager.sqlConnectionAddCenter(newCenter);
                                if (success == true)
                                {
                                    TempData["Message"] = "Successfully created center.";
                                    return RedirectToAction("ManageCenters");
                                }
                                else
                                {
                                    TempData["Message"] = "Database error. Could not add center. Please try again and if the problem persists, contact the Administrator.";
                                    return RedirectToAction("ManageCenters");
                                }
                            }
                            //If centerNameFound is true.
                            else
                            {
                                ViewBag.Message = "Name error. The center name provided is currently in use. Please provide another name.";
                            }
                        }
                        //If errorCenterList is true.
                        else
                        {
                            TempData["Message"] = "Database error. Could not load center list for name comparison. Please try again and if the problem persists, contact the Administrator.";
                            return RedirectToAction("ManageCenters");
                        }
                    }
                    //If there are spaces or '/'s in the files name.
                    else
                    {
                        ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png' type image with no spaces or '/'s in the name.";
                    }
                }
                //If an invalid image file was provided.
                else
                {
                    ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png'' type image.";
                }
            }
            //If model.Picture is null
            else
            {
                ViewBag.Message = "Please provide an image.";
            }
            return View(model);
        }

        #endregion

        #region EditCenters

        //This method returns the EditCenter View with the EditCenterViewModel passed in to display center information.
        [HttpGet]
        [ValidateUser]
        public ActionResult EditCenter(int ID)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            EditCenterViewModel model = new EditCenterViewModel();
            //Getting User information based on User ID
            foundCenter = SQLManager.sqlConnectionForCenter(ID);
            //Checking if the returned center object has null values which occurs if there is a problem with a SQL database.
            //If center object has a null value like Name, then display an error message and hide the save button in the View.
            if (foundCenter.Name != null)
            {
                success = true;
                model.ID = foundCenter.ID;
                model.Name = foundCenter.Name;
                model.Address = foundCenter.Address;
                model.Location = foundCenter.Location;
                model.CenterType = foundCenter.CenterType;
                model.DirectorName = foundCenter.DirectorName;
                model.OfficeNumber = foundCenter.OfficeNumber;
                model.URL = foundCenter.URL;
                model.Description = foundCenter.Description;
                model.ImageURL = foundCenter.ImageURL;
            }
            if (success == false)
            {
                ViewBag.Message = "Database error. Could not load center information. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            //Passing success value into the View. If the Center could not be found, the 'Save' button will be hidden to prevent the User from
            //possibly updating the Center anyway.
            ViewData["success"] = success;
            return View(model);
        }

        //This method allows the Admin to saves changes in the SQL database and redirects the Admin to Manage Accounts
        //with a success or fail message if all checks pass such as the center name check.
        [HttpPost]
        [ValidateUser]
        public ActionResult EditCenter(EditCenterViewModel model)
        {
            Boolean success = false;
            Boolean errorCenterList = false;
            Boolean centerNameFound = false;
            Models.Center updatedCenter = new Models.Center();
            List<Models.Center> centerList = new List<Models.Center>();
            //Storing the List object returned which contains all Centers
            centerList = SQLManager.sqlConnectionForCentersList();
            //If there is a problem with the SQL database, then null values will be given to values in the Center object.
            //Check for possible SQL errors using the "Name" field and if there is, prevent further progress and
            //displays an error message for the Admin or Director.
            errorCenterList = ErrorCenterListCheck(centerList);
            if (errorCenterList == false)
            {
                //Getting ViewModel model information given in the textfields of the Edit Center page
                updatedCenter.ID = model.ID;
                updatedCenter.Name = model.Name;
                updatedCenter.Address = model.Address;
                updatedCenter.Location = model.Location;
                updatedCenter.CenterType = model.CenterType;
                updatedCenter.DirectorName = model.DirectorName;
                updatedCenter.OfficeNumber = model.OfficeNumber;
                updatedCenter.URL = model.URL;
                updatedCenter.Description = model.Description;
                //Before creating a center all center names are compared to the provided center name. If there is a match,
                //then centerNameNotFound becomes true.
                centerNameFound = EditCenterNameCheck(centerList, updatedCenter);
                if (centerNameFound == false)
                {
                    //If no image is provided, then keep the current ImageURL and update the database.
                    if (model.Picture == null)
                    {
                        updatedCenter.ImageURL = model.ImageURL;
                        //Getting Boolean result of SQL entry information update
                        success = SQLManager.sqlConnectionUpdateCenter(updatedCenter.ID, updatedCenter);
                        //Redirect the Admin to the Manage Centers page and print a message regarding success
                        if (success == true)
                        {
                            TempData["Message"] = "Successfully updated center.";
                            return RedirectToAction("ManageCenters");
                        }
                        else
                        {
                            TempData["Message"] = "Database error. Could not update center. Please try again and if the problem persists, contact the Administrator.";
                            return RedirectToAction("ManageCenters");
                        }
                    }
                    //Otherwise store the provided image into the BLOB, store the new ImageURL in the Center object and update the database.
                    else
                    {
                        //Getting file extension.
                        string ext = Path.GetExtension(model.Picture.FileName);
                        //Check for the type of upload see if it's not the correct type of images.
                        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) == true || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) == true ||
                            ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            //Check if the Admin provides an image with spaces or '/'s in it. This is to prevent problems with the BLOB and image deletion.
                            //If any are found, prevent progress and give a message.
                            if (model.Picture.FileName.Contains(" ") == false && model.Picture.FileName.Contains("/") == false)
                            {
                                //Deleting old image from the BLOB.
                                BlobManager.deleteBlob(model.ImageURL);
                                updatedCenter.Picture = model.Picture;
                                //Storing the new image from the BLOB.
                                updatedCenter.ImageURL = BlobManager.uploadAndGetImageBLOBURI(updatedCenter.Picture);
                                //Getting Boolean result of SQL entry information update
                                success = SQLManager.sqlConnectionUpdateCenter(model.ID, updatedCenter);
                                //If the update was successful, redirect the Admin to the Manage Centers page
                                if (success == true)
                                {
                                    TempData["Message"] = "Center updated successfully.";
                                    return RedirectToAction("ManageCenters");
                                }
                                else
                                {
                                    TempData["Message"] = "Database error. Could not update center. Please try again and if the problem persists, contact the Administrator.";
                                    return RedirectToAction("ManageCenters");
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png' type image with no spaces or '/'s in the name.";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please provide a '.jpg', '.jpeg' or '.png' type image.";
                        }
                    }
                }
                //If centerNameFound is true.
                else
                {
                    ViewBag.Message = "Name error. The center name provided is currently in use. Please provide another name.";
                }
            }
            //If errorCenterList is true
            else
            {
                TempData["Message"] = "Database error. Could not load center list for name comparison. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageCenters");
            }
            //To make the submit button appear for the Admin to try again in the event of an error.
            ViewData["success"] = true;
            return View(model);
        }

        #endregion

        #region DeleteCenters
        //This method returns the DeleteCenter View with the DeleteCenterViewModel passed in to display center information.
        [HttpGet]
        [ValidateUser]
        public ActionResult DeleteCenter(int ID)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            DeleteCenterViewModel model = new DeleteCenterViewModel();
            foundCenter = SQLManager.sqlConnectionForCenter(ID);
            //Checking if the returned center object has null values which occurs if there is a problem with a SQL database.
            //If center object does not have a null value like 'Name', then update the 'success' boolean, display an error message and hide the save button in the View.
            if (foundCenter.Name != null)
            {
                success = true;
                model.ID = foundCenter.ID;
                model.Name = foundCenter.Name;
                model.Address = foundCenter.Address;
                model.Location = foundCenter.Location;
                model.CenterType = foundCenter.CenterType;
                model.DirectorName = foundCenter.DirectorName;
                model.OfficeNumber = foundCenter.OfficeNumber;
                model.URL = foundCenter.URL;
                model.Description = foundCenter.Description;
                model.ImageURL = foundCenter.ImageURL;
                //Trimming the Center's ImageURL of the URL to get the file name and passing the result into the ViewModel.
                //The file name is used to access the Center's logo in the BLOB and delete it from the BLOB.
                /*String trimName;
                String fileName = foundCenter.ImageURL;
                trimName = fileName.Substring(fileName.LastIndexOf("/") + 1);
                model.ImageURL = trimName;*/
            }
            if (success == false)
            {
                ViewBag.Message = "Database error. Could not load center. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            //Passing success value into the View. If the Center could not be found, the 'Delete' button will be hidden to prevent the Use from
            //possibly deleting the Center anyway.
            ViewData["success"] = success;
            return View(model);
        }

        //This method deletes the Center from the database if the delete button in the DeleteCenter View is clicked on and 
        //sends the Admin to Manage Accounts with a success or fail message and if the back button is clicked, 
        //then the Admin is sent back to ManageCenters.
        [HttpPost]
        [ValidateUser]
        public ActionResult DeleteCenter(DeleteCenterViewModel model, string button)
        {
            Boolean success = false;
            if (button.Contains("delete"))
            { 
                success = SQLManager.sqlConnectionDeleteCenter(model.ID);
            }
            else if (button.Contains("back"))
            {
                return RedirectToAction("ManageCenters");
            }
            if (success == true)
            {
                TempData["Message"] = "Successfully deleted center.";
                BlobManager.deleteBlob(model.ImageURL);
                return RedirectToAction("ManageCenters");
            }
            else
            {
                TempData["Message"] = "Database error. Could not delete center. Please try again and if the problem persists, contact the Administrator.";
                return RedirectToAction("ManageCenters");
            }
        }

        #endregion

        #region Manage Site

        //This method returns the ManageSite View with buttons appearing based on user rights to the
        //web pages on the site named on the buttons.
        //The Admin and Directors have rights to all portions of the website with Staff having access based on rights
        //given to them.
        [HttpGet]
        [ValidateUser]
        public ActionResult ManageSite()
        {
            User foundUser = new User();
            //Getting SQL table entry based on User ID
            foundUser = SQLManager.sqlConnectionForUser(adminID);
            return View(foundUser);
        }

        //This methods sends the user to the appropriate View based on which button was clicked.
        [HttpPost]
        [ValidateUser]
        public ActionResult ManageSite(String button)
        {
            switch (button)
            {
                case "Home":
                    return RedirectToAction("ManagePersonalAccount");
                case "About Us":
                    return RedirectToAction("ManagePersonalAccount");
                case "Collaborations":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Schools Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Community College Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Engineering Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "News":
                    return RedirectToAction("ManageNews", "News");
                case "Donate":
                    return RedirectToAction("Index", "Donation");
                default:
                    break;
            }
            return View();
        }

        #endregion

        #region Manage Personal Account

        //This method returns the ManagePersonalAccount View with the ManagePersonalAccountViewModel passed in to 
        //display account information
        [HttpGet]
        [ValidateUser]
        public ActionResult ManagePersonalAccount()
        {
            User foundUser = new User();
            ManagePersonalAccountViewModel model = new ManagePersonalAccountViewModel();
            //Getting SQL table entry based on User ID
            foundUser = SQLManager.sqlConnectionForUser(adminID);
            if (foundUser.FirstName != null)
            {
                model.FirstName = foundUser.FirstName;
                model.LastName = foundUser.LastName;
                model.AccountType = foundUser.AccountType;
                model.Center = foundUser.Center;
                model.Email = foundUser.Email;
                model.PhoneNumber = foundUser.PhoneNumber;
                model.Username = foundUser.Username;
                //The following two fields will be used for comparisons in the POST method.
                model.CurrentUsername = foundUser.Username;
                model.CurrentPassword = foundUser.Password;
                model.Home = foundUser.Home;
                model.About_Us = foundUser.About_Us;
                model.Collaborations = foundUser.Collaborations;
                model.MESA_Schools_Program = foundUser.MESA_Schools_Program;
                model.MESA_Community_College_Program = foundUser.MESA_Community_College_Program;
                model.MESA_Engineering_Program = foundUser.MESA_Engineering_Program;
                model.News = foundUser.News;
                model.Donate = foundUser.Donate;
            }
            else
            {
                ViewBag.Message = "Database error. Could not load center. Please refresh the page. If the problem persists, contact the Administrator.";
            }
            return View(model);
        }

        //This method allows the User to edit personal account information, save the changes to the SQL database and
        //refreshes the page for the User showing the update information if successful
        [HttpPost]
        [ValidateUser]
        public ActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model, String button)
        {
            Boolean errorUserList = false;
            Boolean success = false;
            //userNameFound and userList is for username comparison
            Boolean userNameFound = false;
            User updatedUser = new User();
            List<User> userList = new List<User>();
            //Send the User to the deletion confirmation verification page.
            if (button.Contains("delete"))
            {
                return RedirectToAction("DeletePersonalAccount");
            }
            else if (button.Contains("submit"))
            {
                
                //Getting ViewModel model information given in the textfields of the Manage Personal Account page that
                //an Admin is allowed to change
                updatedUser.FirstName = model.FirstName;
                updatedUser.LastName = model.LastName;
                updatedUser.AccountType = model.AccountType;
                updatedUser.Center = model.Center;
                updatedUser.Email = model.Email;
                updatedUser.PhoneNumber = model.PhoneNumber;
                updatedUser.Username = model.Username;
                //If the user decides not to update their password, then the current stored password is stored in 
                //updatedUser to be pushed into the database. Otherwise the new given password is stored to be pushed
                //into the database.
                if (String.IsNullOrEmpty(model.Password) == true)
                {
                    updatedUser.Password = model.CurrentPassword;
                }
                else
                {
                    updatedUser.Password = model.Password;
                }
                //Using the foundUser object to pass the user's current rights to the database.
                updatedUser.Home = model.Home;
                updatedUser.About_Us = model.About_Us;
                updatedUser.Collaborations = model.Collaborations;
                updatedUser.MESA_Schools_Program = model.MESA_Schools_Program;
                updatedUser.MESA_Community_College_Program = model.MESA_Community_College_Program;
                updatedUser.MESA_Engineering_Program = model.MESA_Engineering_Program;
                updatedUser.News = model.News;
                updatedUser.Donate = model.Donate;
                //Storing the List object returned which contains all Users for username comparison.
                userList = SQLManager.sqlConnectionForUsersList();
                errorUserList = ErrorUserListCheck(userList);
                if (errorUserList == false)
                {
                    //Before creating an account all usernames are compared to the provided username. If there is a match,
                    //then userNameNotFound becomes true.
                    userNameFound = UserNameCheck(userList, updatedUser);
                    //If a username in the database matches the provided username, then provide an error message.
                    //If a username in the database does not match the provided username, then push new account changes to the database.
                    //In the event the username is not changed, allow push of new account changes to the database.
                    if (userNameFound == false || (updatedUser.Username.Equals(model.CurrentUsername) == true))
                    {
                        //Getting Boolean result of SQL entry information update
                        success = SQLManager.sqlConnectionUpdateUser(adminID, updatedUser);
                        //If the update was successful, create a confirmation message for the User.
                        if (success == true)
                        {
                            ViewBag.Message = "Successfully updated account.";
                        }
                        else
                        {
                            ViewBag.Message = "Database error. Could not update account. Please try again and if the problem persists, contact the Administrator.";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Name error. The Username provided is currently in use. Please provide another username.";
                    }
                }
                else
                {
                    ViewBag.Message = "Database error. Could not load user list for username comparison. Please try again and if the problem persists, contact the Administrator.";

                }
            }
            return View(model);
        }

        #endregion

        #region Delete Personal Account

        //This method simply provides the confirmation verification page for the deletion of one's account from the database
        [HttpGet]
        [ValidateUser]
        public ActionResult DeletePersonalAccount()
        {
            return View();
        }
        //This method is called when the delete confirmation button is clicked on when deleting own account.
        //It deletes the User from the database and sends the User to the Home Page if successful.
        [HttpPost, ActionName("DeletePersonalAccount")]
        [ValidateUser]
        public ActionResult DeletePersonalAccountConfirmed()
        {
            Boolean success = false;
            success = SQLManager.sqlConnectionDeleteUser(adminID);
            if (success == true)
            {
                SecurityUtility.baseLogOut();
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return RedirectToAction("DeletePersonalAccount");
        }

        #endregion

        //This method does a username comparison between a User object and all Users in a List.
        //Returns a Boolean value based on comparisons.
        private Boolean UserNameCheck(List<User> userList, User newUser)
        {
            Boolean userNameFound = false;
            foreach (var item in userList)
            {
                if (item.Username.Equals(newUser.Username))
                {
                    userNameFound = true;
                }
            }
            return userNameFound;
        }

        //This method does a center name comparison between a Center object and all Centers in a List when updating a Center.
        //Returns a Boolean value based on comparisons.
        private Boolean EditCenterNameCheck(List<Models.Center> centerList, Models.Center newCenter)
        {
            Boolean centerNameFound = false;
            foreach (var item in centerList)
            {
                //Return true if a center with the same name and a different ID has been found.
                //There is a chance that a name was wasn't changed in editing, so we want to prevent
                //a false positive by including the ID.
                if (item.Name.Equals(newCenter.Name.TrimEnd(' ')) && (item.ID != newCenter.ID))
                {
                    centerNameFound = true;
                }
            }
            return centerNameFound;
        }

        //This method does a center name comparison between a Center object and all Centers in a List.
        //Returns a Boolean value based on comparisons.
        private Boolean AddCenterNameCheck(List<Models.Center> centerList, Models.Center newCenter)
        {
            Boolean centerNameFound = false;
            foreach (var item in centerList)
            {
                if (item.Name.Equals(newCenter.Name.TrimEnd(' ')))
                {
                    centerNameFound = true;
                }
            }
            return centerNameFound;
        }

        //This method does a first name comparison between all Center objects in a List and null.
        //Returns a Boolean value based on comparisons.
        private Boolean ErrorUserListCheck(List<User> userList)
        {
            Boolean errorUserList = false;
            foreach (var item in userList)
            {
                if (item.FirstName == null)
                {
                    errorUserList = true;
                }
            }
            return errorUserList;
        }

        //This method does a center name comparison between all Center objects in a List and null.
        //Returns a Boolean value based on comparisons.
        private Boolean ErrorCenterListCheck(List<Models.Center> centerList)
        {
            Boolean errorCenterList = false;
            foreach (var item in centerList)
            {
                if (item.Name == null)
                {
                    errorCenterList = true;
                }
            }
            return errorCenterList;
        }
    }
}