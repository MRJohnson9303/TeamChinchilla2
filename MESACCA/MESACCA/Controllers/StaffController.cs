using System;
using System.Collections.Generic;
using MESACCA.Models;
using S = System.Data.SqlClient;
using T = System.Threading;
using MESACCA.ViewModels.Staff;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace MESACCA.Controllers
{
    public class StaffController : Controller
    {
        //Used for personal account management
        private static int staffID;
        //This method is a simple hello to the user when he or she signs in as well as saving the ID for personal account
        //management
        public ActionResult Index(int ID, String firstName, String lastName)
        {
            staffID = ID;
            ViewData["firstName"] = firstName;
            ViewData["lastName"] = lastName;
            return View();
        }
        public ActionResult ManageSite()
        {
            return View();
        }
        //This method returns the ManagePersonalAccount View with the ManagePersonalAccountViewModel passed in to 
        //display account information
        [HttpGet]
        public ActionResult ManagePersonalAccount()
        {
            User foundUser = new Models.User();
            ManagePersonalAccountViewModel model = new ManagePersonalAccountViewModel();
            //Getting SQL table entry based on User ID
            foundUser = sqlConnectionForUser(staffID);
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
        //refreshes the page for the User showing the update information if successful. It also allows the User to delete
        //own account by first takes the User to a confirmation page before deletion.
        [HttpPost]
        public ActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model, string button)
        {
            Boolean success = false;
            User foundUser = new User();
            if (button.Contains("submit"))
            {

                //Getting SQL table entry based on User ID to obtain the User's rights since the user can't manage own rights
                //to update.
                foundUser = sqlConnectionForUser(staffID);
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
                //Getting Boolean result of SQL entry information update
                success = sqlConnectionUpdateUser(staffID, updatedUser);
            }
            else if (button.Contains("delete"))
            {
                return RedirectToAction("DeletePersonalAccount");
            }
            //If the update was successful, redirect the User to the Manage Personal Account View to refresh the page
            //with the updated information.
            if (success == true)
            {
                return RedirectToAction("ManagePersonalAccount");
            }
            return View(model);
        }
        //This method simply provides the confirmation page for the deletion of one's account from the database
        [HttpGet]
        public ActionResult DeletePersonalAccount()
        {
            return View();
        }
        //This method is called when the delete confirmation button is clicked on when deleting own account.
        //It deletes the User from the database and sends the User to the Home Page if successful.
        [HttpPost, ActionName("DeletePersonalAccount")]
        public ActionResult DeletePersonalAccountConfirmed()
        {
            Boolean success = false;
            success = sqlConnectionDeleteUser(staffID);
            if (success == true)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return RedirectToAction("DeletePersonalAccount");
        }
        //This method invokes "accessDatabaseForUser" to attempt to connect to the SQL database and returns a User object
        private User sqlConnectionForUser(int ID)
        {
            User foundUser = new User();
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    foundUser = accessDatabaseForUser(ID);
                    //Break if an account from the SQL database was found 
                    if (foundUser.AccountType != null)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return foundUser;
        }
        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided username and password and returns a User object with all information of the User
        private User accessDatabaseForUser(int ID)
        {
            User foundUser = new User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object and returning it
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                    foundUser.Home = dataReader.GetString(9).TrimEnd(' ');
                    foundUser.About_Us = dataReader.GetString(10).TrimEnd(' ');
                    foundUser.Vision_Mission_Values = dataReader.GetString(11).TrimEnd(' ');
                    foundUser.MESA_Schools_Program = dataReader.GetString(12).TrimEnd(' ');
                    foundUser.MESA_Community_College_Program = dataReader.GetString(13).TrimEnd(' ');
                    foundUser.MESA_Engineering_Program = dataReader.GetString(14).TrimEnd(' ');
                    foundUser.News = dataReader.GetString(15).TrimEnd(' ');
                    foundUser.Donate = dataReader.GetString(16).TrimEnd(' ');
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return foundUser;
            }
        }
        //This method invokes "updateUserDatabase" to attempt to connect to the SQL database and returns a Boolean value regarding update confirmation
        private Boolean sqlConnectionUpdateUser(int ID, User updatedUser)
        {
            Boolean success = false;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    success = updateUserDatabase(ID, updatedUser);
                    //Break if an account from the SQL database was found 
                    if (success == true)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return success;
        }
        //This method connects to the database, updates the specified SQL entry by the User's ID, collects the 
        //SQL entry for comparison and return a Boolean value based on the comparisons performed.
        private Boolean updateUserDatabase(int ID, User updatedUser)
        {
            Boolean success = false;
            User foundUser = new Models.User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"UPDATE Users 
                                              SET FirstName = @FirstName, LastName = @LastName, Center = @Center, Email = @Email,
                                                  PhoneNumber = @PhoneNumber, Username = @Username, Password = @Password,
                                                  Home = @Home, About_Us = @About_Us, Vision_Mission_Values = @Vision_Mission_Values,
                                                  MESA_Schools_Program = @MESA_Schools_Program,
                                                  MESA_Community_College_Program = @MESA_Community_College_Program, 
                                                  MESA_Engineering_Program = @MESA_Engineering_Program, 
                                                  News = @News, Donate = @Donate
                                              WHERE ID = @ID
                                              SELECT * FROM Users WHERE ID = @ID";
                    //Updating User information based on comparison with current and new User information
                    //I trim the end of all fields to remove empty spaces
                    dbCommand.Parameters.AddWithValue("@FirstName", updatedUser.FirstName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@LastName", updatedUser.LastName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Center", updatedUser.Center.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Email", updatedUser.Email.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@PhoneNumber", updatedUser.PhoneNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@UserName", updatedUser.Username.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Password", updatedUser.Password.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Home", updatedUser.Home.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@About_Us", updatedUser.About_Us.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Vision_Mission_Values", updatedUser.Vision_Mission_Values.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Schools_Program", updatedUser.MESA_Schools_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Community_College_Program", updatedUser.MESA_Community_College_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Engineering_Program", updatedUser.MESA_Engineering_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@News", updatedUser.News.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Donate", updatedUser.Donate.TrimEnd(' '));
                    //Specifing update by ID number to ensure correct User's information is updated
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //Getting the updated SQL entry information for comparison testing to verify the update was successful
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                    //Determining if the update was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the updated information provided by the user.
                    if (dataReader.HasRows == true && updatedUser.FirstName.Equals(foundUser.FirstName) &&
                        updatedUser.LastName.Equals(foundUser.LastName) &&
                        updatedUser.Center.Equals(foundUser.Center) &&
                        updatedUser.Email.Equals(foundUser.Email) &&
                        updatedUser.PhoneNumber.Equals(foundUser.PhoneNumber) &&
                        updatedUser.Username.Equals(foundUser.Username) &&
                        updatedUser.Password.Equals(foundUser.Password))
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        //This method invokes "accessDatabaseForUsers" to attempt to connect to the SQL database and returns a List object containing all Users
        private List<User> sqlConnectionForUsersList()
        {
            //SortedList<String, User> userList = new SortedList<String, User>();
            List<User> userList = new List<User>();
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    userList = accessDatabaseForUsers();
                    //Break if the List object is not empty
                    if (userList.Count > 0)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return userList;
        }
        //This method connects to the database, collects all the entries in the Users table into a list
        //based on Users' account type and returns the list.
        private List<User> accessDatabaseForUsers()
        {
            //SortedList<String, User> userList = new SortedList<String, User>();
            List<User> userList = new List<User>();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"SELECT * FROM Users";
                    var dataReader = dbCommand.ExecuteReader();
                    var iterator = dataReader.GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        User foundUser = new User();
                        //Getting the SQL entry information 
                        //I trim all of the found User data because the SQL server seems to add spaces.
                        foundUser.ID = dataReader.GetInt32(0);
                        foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                        foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                        foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                        foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                        foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                        foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                        foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                        foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                        userList.Add(foundUser);
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return userList;
            }
        }
        //This method invokes "accessDatabaseToAddUser" to attempt to connect to the SQL database and a Boolean value regarding account creation confirmation
        private Boolean sqlConnectionAddUser(int newID, User newUser)
        {
            Boolean success = false;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    success = accessDatabaseToAddUser(newID, newUser);
                    //Break if an account added to the SQL database 
                    if (success == true)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return success;
        }
        //This method connects to the database, adds to the Users table, collects Users from the table for comparison,
        //and returns Boolean value regarding success
        private Boolean accessDatabaseToAddUser(int newID, User newUser)
        {
            Boolean success = false;
            User foundUser = new User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"INSERT INTO Users (ID, FirstName, LastName, AccountType, Center, Email, PhoneNumber, Username, Password,
                                                                 Home, About_Us, Vision_Mission_Values, MESA_Schools_Program, MESA_Community_College_Program, 
                                                                 MESA_Engineering_Program, News, Donate)
                                              Values (@ID, @FirstName, @LastName, @AccountType, @Center, @Email, @PhoneNumber, @Username, @Password,
                                                      @Home, @About_Us, @Vision_Mission_Values, @MESA_Schools_Program, @MESA_Community_College_Program, 
                                                      @MESA_Engineering_Program, @News, @Donate)
                                              Select * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", newID);
                    //I trim the ends of empty spaces
                    dbCommand.Parameters.AddWithValue("@FirstName", newUser.FirstName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@LastName", newUser.LastName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@AccountType", newUser.AccountType.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Center", newUser.Center.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Email", newUser.Email.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@PhoneNumber", newUser.PhoneNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Username", newUser.Username.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Password", newUser.Password.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Home", newUser.Home.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@About_Us", newUser.About_Us.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Vision_Mission_Values", newUser.Vision_Mission_Values.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Schools_Program", newUser.MESA_Schools_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Community_College_Program", newUser.MESA_Community_College_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Engineering_Program", newUser.MESA_Engineering_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@News", newUser.News.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Donate", newUser.Donate.TrimEnd(' '));
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object 
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                    //Determining if the table entry was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the new User's information.
                    if (dataReader.HasRows == true && newUser.FirstName.Equals(foundUser.FirstName) &&
                        newUser.LastName.Equals(foundUser.LastName) &&
                        newUser.Center.Equals(foundUser.Center) &&
                        newUser.Email.Equals(foundUser.Email) &&
                        newUser.PhoneNumber.Equals(foundUser.PhoneNumber) &&
                        newUser.Username.Equals(foundUser.Username) &&
                        newUser.Password.Equals(foundUser.Password))
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        //This method invokes "accessDatabaseToDeleteUser" to attempt to connect to the SQL database and returns a Boolean value regarding deletion confirmation
        private Boolean sqlConnectionDeleteUser(int ID)
        {
            Boolean success = false;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    success = accessDatabaseToDeleteUser(ID);
                    //Break if an account from the SQL database was found to be gone
                    if (success == true)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return success;
        }
        //This method connects to the database, delete the entry with the given ID, connects with the database again
        //to check if the entry is gone and returns the Boolean result of the check.
        private Boolean accessDatabaseToDeleteUser(int ID)
        {
            Boolean success = false;
            User foundUser = new User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @" DELETE FROM Users WHERE ID = @ID
                                               SELECT * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //If the User can't be found, then the User was successfully deleted 
                    if (dataReader.HasRows == false)
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        ///This method returns an ADO.NET connection string. 
        private static string GetSqlConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;
        }
    }
}