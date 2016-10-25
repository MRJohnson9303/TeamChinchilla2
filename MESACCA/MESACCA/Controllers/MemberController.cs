using System;
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
        public ActionResult AddDirectorAccount(string firstName, string lastName, string accountType, string message)
        {
            if (message != null)
            {
                ViewBag.Message = message;
            }
            AddAccountViewModel model = new AddAccountViewModel();
            model.FirstName = firstName;
            model.LastName = lastName;
            model.AccountType = accountType;
            model.Home = true;
            model.About_Us = true;
            model.Vision_Mission_Values = true;
            model.MESA_Schools_Program = true;
            model.MESA_Community_College_Program = true;
            model.MESA_Engineering_Program = true;
            model.News = true;
            model.Donate = true;
            return View(model);
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult AddDirectorAccount(AddAccountViewModel model)
        {
            Boolean success = false;
            User newUser = new User();
            //ID initialized for comparison
            int ID = 1;
            //userNameNoFound initialized for comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            //Storing the List object returned which contains all Users
            userList = SQLManager.sqlConnectionForUsersList();
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
            newUser.Vision_Mission_Values = "True";
            newUser.MESA_Schools_Program = "True";
            newUser.MESA_Community_College_Program = "True";
            newUser.MESA_Engineering_Program = "True";
            newUser.News = "True";
            newUser.Donate = "True";
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
                    return RedirectToAction("ManageAccounts");
                }
                else
                {
                    ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }
            }
            else
            {
                ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
            }
            return View(model);
        }

        //This method returns the AddStaffAccount View with the first name, last name and account type filled in using
        //passed in information, but the account type textfield is readonly.
        [HttpGet]
        [ValidateUser]
        public ActionResult AddStaffAccount(string firstName, string lastName, string accountType)
        {
            AddAccountViewModel model = new AddAccountViewModel();
            model.FirstName = firstName;
            model.LastName = lastName;
            model.AccountType = accountType;
            model.Home = true;
            model.About_Us = true;
            model.Vision_Mission_Values = true;
            model.MESA_Schools_Program = true;
            model.MESA_Community_College_Program = true;
            model.MESA_Engineering_Program = true;
            model.News = true;
            model.Donate = true;
            ViewData["CreatorAccountType"] = userAccountType;
            if (userAccountType.Equals("Director"))
            {
                model.Center = center;
            }
            return View(model);
        }

        //This method adds a Staff account with provided information to the SQL database and redirects user to ManageAccounts
        //if successful.
        [HttpPost]
        [ValidateUser]
        public ActionResult AddStaffAccount(AddAccountViewModel model)
        {
            Boolean success = false;
            User newUser = new User();
            //ID initialized for comparison
            int ID = 1;
            //userNameNoFound initialized for comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            //Storing the List object returned which contains all Users
            userList = SQLManager.sqlConnectionForUsersList();
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
            newUser.Vision_Mission_Values = model.Vision_Mission_Values.ToString();
            newUser.MESA_Schools_Program = model.MESA_Schools_Program.ToString();
            newUser.MESA_Community_College_Program = model.MESA_Community_College_Program.ToString();
            newUser.MESA_Engineering_Program = model.MESA_Engineering_Program.ToString();
            newUser.News = model.News.ToString();
            newUser.Donate = model.Donate.ToString();
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
                    return RedirectToAction("ManageAccounts");
                }
                else
                {
                    ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }
            }
            else
            {
                ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
            }
            return View(model);
        }

        #endregion

        #region EditAccounts

        //This method returns the Edit View with the EditViewModel passed in to display account information
        [HttpGet]
        [ValidateUser]
        public ActionResult EditAccount(int ID)
        {
            User foundUser = new User();
            EditViewModel model = new EditViewModel();
            //Getting User information based on User ID
            foundUser = SQLManager.sqlConnectionForUser(ID);
            //Storing the information in ViewData to be used to fill in the Edit form
            model.ID = foundUser.ID;
            model.FirstName = foundUser.FirstName;
            model.LastName = foundUser.LastName;
            model.AccountType = foundUser.AccountType;
            model.Center = foundUser.Center;
            model.Email = foundUser.Email;
            model.PhoneNumber = foundUser.PhoneNumber;
            model.Username = foundUser.Username;
            model.Home = Convert.ToBoolean(foundUser.Home);
            model.About_Us = Convert.ToBoolean(foundUser.About_Us);
            model.Vision_Mission_Values = Convert.ToBoolean(foundUser.Vision_Mission_Values);
            model.MESA_Schools_Program = Convert.ToBoolean(foundUser.MESA_Schools_Program);
            model.MESA_Community_College_Program = Convert.ToBoolean(foundUser.MESA_Community_College_Program);
            model.MESA_Engineering_Program = Convert.ToBoolean(foundUser.MESA_Engineering_Program);
            model.News = Convert.ToBoolean(foundUser.News);
            model.Donate = Convert.ToBoolean(foundUser.Donate);
            return View(model);
        }

        //This method allows the Admin to edit accounts displayed in Manage Accounts and saves changes in the SQL database
        [HttpPost]
        [ValidateUser]
        public ActionResult EditAccount(EditViewModel model)
        {
            Boolean success = false;
            //userNameFound and userList is for username comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            User foundUser = new User();
            //Getting SQL table entry based on User ID to obtain the user's password and for username comparison.
            foundUser = SQLManager.sqlConnectionForUser(model.ID);
            User updatedUser = new User();
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page
            updatedUser.FirstName = model.FirstName;
            updatedUser.LastName = model.LastName;
            updatedUser.AccountType = model.AccountType;
            updatedUser.Center = model.Center;
            updatedUser.Email = model.Email;
            updatedUser.PhoneNumber = model.PhoneNumber;
            updatedUser.Username = model.Username;
            //If the Admin decides not to update a User's password, then the current stored password is stored in 
            //updatedUser to be pushed into the database. Otherwise the new given password is stored to be pushed
            //into the database.
            if (String.IsNullOrEmpty(model.Password) == true)
            {
                updatedUser.Password = foundUser.Password;
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
                updatedUser.Vision_Mission_Values = "True";
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
                updatedUser.Vision_Mission_Values = model.Vision_Mission_Values.ToString();
                updatedUser.MESA_Schools_Program = model.MESA_Schools_Program.ToString();
                updatedUser.MESA_Community_College_Program = model.MESA_Community_College_Program.ToString();
                updatedUser.MESA_Engineering_Program = model.MESA_Engineering_Program.ToString();
                updatedUser.News = model.News.ToString();
                updatedUser.Donate = model.Donate.ToString();
            }
            //Storing the List object returned which contains all Users for username comparison.
            userList = SQLManager.sqlConnectionForUsersList();
            //Before creating an account all usernames are compared to the provided username. If there is a match,
            //then userNameNotFound becomes true.
            userNameFound = UserNameCheck(userList, updatedUser);
            //If a username in the database matches the provided username, then provide an error message.
            //If a username in the database does not match the provided username, then push new account changes to the database.
            //In the event the username is not changed, allow push of new account changes to the database.
            if (userNameFound == false || (updatedUser.Username.Equals(foundUser.Username) == true))
            {
                //Getting Boolean result of SQL entry information update
                success = SQLManager.sqlConnectionUpdateUser(model.ID, updatedUser);
                //If the update was successful, redirect the User to the Manage Accounts page
                if (success == true)
                {
                    return RedirectToAction("ManageAccounts");
                }
                else
                {
                    ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
                }
            }
            else
            {
                ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
            }
            return View(model);
        }

        #endregion

        #region DeleteAccounts

        //This method sends an entry's information from Manage Accounts into the View when the Delete link is clicked on
        [HttpGet]
        [ValidateUser]
        public ActionResult DeleteAccount(int ID)
        {
            User foundUser = new User();
            foundUser = SQLManager.sqlConnectionForUser(ID);
            return View(foundUser);
        }

        //This method deletes the user from the system if the delete button in the Edit View is clicked on and sends the User
        //to Manage Accounts and if the back button is clicked, then the Admin is sent back to ManageAccounts.
        [HttpPost]
        [ValidateUser]
        public ActionResult DeleteAccount(User model, string button)
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
                return RedirectToAction("ManageAccounts");
            }
            else
            {
                ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
            }
            return View(model);
        }

        #endregion

        #region Centers

        [HttpGet]
        [ValidateUser]
        public ActionResult ManageCenters()
        {
            List<Models.Center> centerList = new List<Models.Center>();
            //Storing the List object returned which contains all Centers
            centerList = SQLManager.sqlConnectionForCentersList();
            centerList.Sort(delegate (Models.Center x, Models.Center y)
            {
                return x.Name.CompareTo(y.Name);
            });
            return View(centerList);
        }
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
        //This method returns the AddCenter View
        [HttpGet]
        [ValidateUser]
        public ActionResult AddCenter()
        {
            return View();
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult AddCenter(AddCenterViewModel model)
        {
            /*if(String.IsNullOrEmpty(model.Name) == false)
            {
                return RedirectToAction("ManageAccounts");
            }
            if (model.Picture.ContentLength > 0)
            {
                return RedirectToAction("ManageAccounts");
            }*/
            Boolean success = false;
            Models.Center newCenter = new Models.Center();
            //ID initialized for comparison
            int ID = 1;
            List<Models.Center> centerList = new List<Models.Center>();
            //Storing the SortedList object returned which contains all Users
            centerList = SQLManager.sqlConnectionForCentersList();
            //ID is compared with the ID value of all Users and is incremented by 1 in each loop. If ID doesn't match
            //a User ID then break the loop and use the new ID value for the new User account ID.
            //This means if a User is deleted, then a new User will get the old ID
            if (centerList.Capacity > 0)
            {
                foreach (var item in centerList)
                {
                    if (ID != item.ID)
                    {
                        break;
                    }
                    ID += 1;
                }
            }
            newCenter.Name = model.Name;
            newCenter.Address = model.Address;
            newCenter.Location = model.Location;
            newCenter.CenterType = model.CenterType;
            newCenter.DirectorName = model.DirectorName;
            newCenter.OfficeNumber = model.OfficeNumber;
            newCenter.URL = model.URL;
            newCenter.Description = model.Description;

            success = SQLManager.sqlConnectionAddCenter(ID, newCenter);
            if (success == true)
            {
                return RedirectToAction("ManageCenters");
            }

            return View();
        }

        //This method returns the EditCenter View with the EditCenterViewModel passed in to display center information
        [HttpGet]
        [ValidateUser]
        public ActionResult EditCenter(int ID)
        {
            Models.Center foundCenter = new Models.Center();
            EditCenterViewModel model = new EditCenterViewModel();
            //Getting User information based on User ID
            foundCenter = SQLManager.sqlConnectionForCenter(ID);
            //Storing the information in ViewData to be used to fill in the Edit form
            model.ID = foundCenter.ID;
            model.Name = foundCenter.Name;
            model.Address = foundCenter.Address;
            model.Location = foundCenter.Location;
            model.CenterType = foundCenter.CenterType;
            model.DirectorName = foundCenter.DirectorName;
            model.OfficeNumber = foundCenter.OfficeNumber;
            model.URL = foundCenter.URL;
            model.Description = foundCenter.Description;
            return View(model);
        }

        //This method allows the Admin or Director to edit centers displayed in Manage Centers and saves changes in the SQL database
        [HttpPost]
        [ValidateUser]
        public ActionResult EditCenter(EditCenterViewModel model)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            //Getting SQL table entry based on User ID to obtain the user's password.
            foundCenter = SQLManager.sqlConnectionForCenter(model.ID);
            Models.Center updatedCenter = new Models.Center();
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page
            updatedCenter.Name = model.Name;
            updatedCenter.Address = model.Address;
            updatedCenter.Location = model.Location;
            updatedCenter.CenterType = model.CenterType;
            updatedCenter.DirectorName = model.DirectorName;
            updatedCenter.OfficeNumber = model.OfficeNumber;
            updatedCenter.URL = model.URL;
            updatedCenter.Description = model.Description;
            updatedCenter.Picture = model.Picture;
            //Getting Boolean result of SQL entry information update
            success = SQLManager.sqlConnectionUpdateCenter(model.ID, updatedCenter);
            //If the update was successful, redirect the User to the Manage Centers page
            if (success == true)
            {
                return RedirectToAction("ManageCenters");
            }
            else
            {
                ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
            }
            return View(model);
        }
        //This method sends an entry's information from Manage Centers into the View when the Delete link is clicked on
        [HttpGet]
        [ValidateUser]
        public ActionResult DeleteCenter(int ID)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            DeleteCenterViewModel model = new DeleteCenterViewModel();
            foundCenter = SQLManager.sqlConnectionForCenter(ID);
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
            }
            if (success == false)
            {
                ViewBag.Message = "Database error. Please click on 'Back to List' and try again. If the problem persists, contact the Administrator.";
            }
            //Passing success value into the View. If the Center could not be found, the 'Delete' button will be hidden to prevent the Use from
            //possibly deleting the Center anyway.
            ViewData["success"] = success;
            return View(model);
        }

        //This method deletes the Center from the database if the delete button in the Edit View is clicked on and sends the User
        //to Manage Accounts and if the back button is clicked, then the Admin is sent back to ManageCenters.
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
                return RedirectToAction("ManageCenters");
            }
            else
            {
                ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
            }
            return View(model);
        }

        #endregion

        #region Manage Site

        //This method returns the ManageSite View with buttons appearing based on user rights to the
        //web pages on the site named on the buttons.
        //The Admin has rights to all portions of the website.
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
                case "Vision Mission Values":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Schools Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Community College Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "MESA Engineering Program":
                    return RedirectToAction("ManagePersonalAccount");
                case "News":
                    return RedirectToAction("SelectNews", "News", new { referrer = "Admin" });
                case "Donate":
                    return RedirectToAction("ManagePersonalAccount");
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
            model.FirstName = foundUser.FirstName;
            model.LastName = foundUser.LastName;
            model.AccountType = foundUser.AccountType;
            model.Center = foundUser.Center;
            model.Email = foundUser.Email;
            model.PhoneNumber = foundUser.PhoneNumber;
            model.Username = foundUser.Username;
            return View(model);
        }

        //This method allows the User to edit personal account information, save the changes to the SQL database and
        //refreshes the page for the User showing the update information if successful
        [HttpPost]
        [ValidateUser]
        public ActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model, String button)
        {
            Boolean success = false;
            //userNameFound and userList is for username comparison
            Boolean userNameFound = false;
            List<User> userList = new List<User>();
            User foundUser = new User();
            //Send the User to the deletion confirmation verification page.
            if (button.Contains("delete"))
            {
                return RedirectToAction("DeletePersonalAccount");
            }
            else if (button.Contains("submit"))
            {
                //Getting SQL table entry based on User ID to obtain the user's rights since the user can't manage own rights
                //to update and for the username comparison.
                foundUser = SQLManager.sqlConnectionForUser(adminID);
                User updatedUser = new User();
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
                    updatedUser.Password = foundUser.Password;
                }
                else
                {
                    updatedUser.Password = model.Password;
                }
                //Using the foundUser object to pass the user's current rights to the database.
                updatedUser.Home = foundUser.Home;
                updatedUser.About_Us = foundUser.About_Us;
                updatedUser.Vision_Mission_Values = foundUser.Vision_Mission_Values;
                updatedUser.MESA_Schools_Program = foundUser.MESA_Schools_Program;
                updatedUser.MESA_Community_College_Program = foundUser.MESA_Community_College_Program;
                updatedUser.MESA_Engineering_Program = foundUser.MESA_Engineering_Program;
                updatedUser.News = foundUser.News;
                updatedUser.Donate = foundUser.Donate;
                //Storing the List object returned which contains all Users for username comparison.
                userList = SQLManager.sqlConnectionForUsersList();
                //Before creating an account all usernames are compared to the provided username. If there is a match,
                //then userNameNotFound becomes true.
                userNameFound = UserNameCheck(userList, updatedUser);
                //If a username in the database matches the provided username, then provide an error message.
                //If a username in the database does not match the provided username, then push new account changes to the database.
                //In the event the username is not changed, allow push of new account changes to the database.
                if (userNameFound == false || (updatedUser.Username.Equals(foundUser.Username) == true))
                {
                    //Getting Boolean result of SQL entry information update
                    success = SQLManager.sqlConnectionUpdateUser(adminID, updatedUser);
                    //If the update was successful, create a confirmation message for the User.
                    if (success == true)
                    {
                        ViewBag.Message = "Account was successfully updated";
                    }
                    else
                    {
                        ViewBag.Message = "Database error. Please try again and if the problem persists, contact the Administrator.";
                    }
                }
                else
                {
                    ViewBag.Message = "The Username provided is currently in use. Please provide another username.";
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

        [HttpGet]
        [ValidateUser]
        public ActionResult PictureTest()
        {
            /*
            model.Blob_Name = "";
            model.Uri_Name = "";
            model.Container_Name = "";
            */

            return View();
        }

        [HttpPost]
        [ValidateUser]
        public ActionResult PictureTest(HttpPostedFileBase File)
        {
            /* if(File.ContentLength > 0)
             {
                 return RedirectToAction("ManageAccounts");
             }
             return View();*/

            // Boolean success = false;
            // int ID = 1;
            // Boolean userNameFound = false;
            // List<BlobData> userList = new List<BlobData>();
            //userList = SQLManager.sqlConnectionForUsersList();

            if (File.ContentLength > 0)
            {

                CloudBlobContainer blobContainer = blobService.GetCloudBlobContainer();
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(File.FileName);
                blob.UploadFromStream(File.InputStream);

                BlobData BlobTest = new BlobData();



                BlobTest.name = blob.Name;
                BlobTest.uri_name = blob.Uri.ToString();
                BlobTest.container_name = blob.Container.Name;

                BlobTest.name = "222";
                BlobTest.uri_name = "333";
                BlobTest.container_name = "444444";

                /* AddBlobViewModel bb = new AddBlobViewModel();

                 bb.Blob_Name = name;
                 bb.Uri_Name = uri_name;
                 bb.Container_Name = container_name;
                 */

                return RedirectToAction("ViewBlob", new { BlobTest });
            }
            return RedirectToAction("ManageCenters");


        }
        [HttpGet]
        public ActionResult ViewBlob(BlobData bb)
        {
            return View(bb);
        }


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
    }
}