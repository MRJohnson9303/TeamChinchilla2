using System;
using System.Collections.Generic;
using MESACCA.Models;
using S = System.Data.SqlClient;
using T = System.Threading;
using MESACCA.ViewModels.Admin;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using MESACCA.FilterAttributes;
using MESACCA.DataBaseManagers;
using MESACCA.Utilities;

namespace MESACCA.Controllers
{
    public class MemberController : Controller
    {
        //Used for personal account management
        private static int adminID;
        private static Users User;
        //This method is a simple hello to the user when he or she signs in as well as saving the ID for personal account
        //management
        [ValidateUser]
        public ActionResult Index() //int ID, String firstName, String lastName
        {
            User = MyUserManager.GetUser();

            adminID = User.ID;
            ViewData["ID"] = User.ID;
            ViewData["firstName"] = User.FirstName;
            ViewData["lastName"] = User.LastName;
            
            return View();
        }

        //This method collects all User accounts and passes them into the View to be displayed in Manage Accounts
        [ValidateUser]
        public ActionResult ManageAccounts(String name, String sortOrder)
        {
            List<User> userList = new List<User>();
            List<User> searchList = new List<User>();
            //Storing the List object returned which contains all Users
            userList = SQLManager.sqlConnectionForUsersList();
            //If the search text bar passes a value, then a list is created and passed into the view containing
            //users whose names contain the given search input
            if (String.IsNullOrEmpty(name) == false)
            {
                foreach (User member in userList)
                {
                    String username = member.FirstName + " " + member.LastName;
                    if (username.Contains(name))
                    {
                        searchList.Add(member);
                    }
                }
                userList = searchList;
            }
            //If there is no value passed in the search text bar, sorting the list of Users is based on given sort 
            //button clicks with the list being ordered by account type by default 
            if (String.IsNullOrEmpty(name) == true)
            {
                switch (sortOrder)
                {
                    case "First Name":
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.FirstName.CompareTo(y.FirstName);
                            });
                            break;
                        }
                    case "Last Name":
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.LastName.CompareTo(y.LastName);
                            });
                            break;
                        }
                    default:
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.AccountType.CompareTo(y.AccountType);
                            });
                            break;
                        }
                }
            }
            return View(userList);
        }

        #region Add Accounts

        //This method returns the AddAccount View
        [HttpGet]
        [ValidateUser]
        public ActionResult AddAccount()
        {
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
            ViewData["FirstName"] = firstName;
            ViewData["LastName"] = lastName;
            ViewData["AccountType"] = accountType;
            return View();
        }
       
        //This method returns the AddStaffAccount View with the first name, last name and account type filled in using
        //passed in information, but the account type textfield is readonly.
        [HttpGet]
        [ValidateUser]
        public ActionResult AddStaffAccount(string firstName, string lastName, string accountType)
        {
            return View();
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
            List<User> userList = new List<User>();
            //Storing the SortedList object returned which contains all Users
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
            success = SQLManager.sqlConnectionAddUser(ID, newUser);
            if (success == true)
            {
                return RedirectToAction("ManageAccounts");
            }
            return View();
        }

        #endregion

        #region Edit

        //This method returns the Edit View with the EditViewModel passed in to display account information
        [HttpGet]
        [ValidateUser]
        public ActionResult Edit(int ID)
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
        public ActionResult Edit(EditViewModel model)
        {
            Boolean success = false;
            User foundUser = new User();
            //Getting SQL table entry based on User ID to obtain the user's password.
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
            //Getting Boolean result of SQL entry information update
            success = SQLManager.sqlConnectionUpdateUser(model.ID, updatedUser);
            //If the update was successful, redirect the User to the Manage Accounts page
            if (success == true)
            {
                return RedirectToAction("ManageAccounts");
            }
            return View(model);
        }

        #endregion

        #region Delete

        //This method sends an entry's information from Manage Accounts into the View when the Delete link is clicked on
        [HttpGet]
        [ValidateUser]
        public ActionResult Delete(int ID)
        {
            User foundUser = new User();
            foundUser = SQLManager.sqlConnectionForUser(ID);
            return View(foundUser);
        }
        
        //This method deletes the user from the system if the delete button in the Edit View is clicked on and sends the User
        //to Manage Accounts and if the back button is clicked, then the Admin is sent back to ManageAccounts.
        [HttpPost]
        [ValidateUser]
        public ActionResult Delete(User model, string button)
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
            return RedirectToAction("Edit", new { ID = model.ID });
        }

        #endregion

        #region Centers

        [HttpGet]
        [ValidateUser]
        public ActionResult ManageCenters()
        {
            return View();
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
            }*/
            if (model.Picture.ContentLength > 0)
            {
                return RedirectToAction("ManageAccounts");
            }
            return RedirectToAction("ManageCenters");
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
            return View();
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
                    return RedirectToAction("AddNews", "News", new { referrer = "Admin" });
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
        //refreshes the page for the User showing the update informatin if successful
        [HttpPost]
        [ValidateUser]
        public ActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model)
        {
            Boolean success = false;
            User foundUser = new User();
            //Getting SQL table entry based on User ID to obtain the user's rights since the user can't manage own rights
            //to update.
            foundUser = SQLManager.sqlConnectionForUser(adminID);
            User updatedUser = new User();
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page that
            //an Admin is allowed to change
            updatedUser.FirstName = model.FirstName;
            updatedUser.LastName = model.LastName;
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
            //Getting Boolean result of SQL entry information update
            success = SQLManager.sqlConnectionUpdateUser(adminID, updatedUser);
            //If the update was successful, redirect the User to the Manage Personal Account View to refresh the page
            //with the updated information.
            if (success == true)
            {
                return RedirectToAction("ManagePersonalAccount");
            }
            return View(model);
        }

        #endregion
        
    }
}