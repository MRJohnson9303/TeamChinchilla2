using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MESACCA.Utilities;
using MESACCA.Models;
using MESACCA.ViewModels.Donation;
using S = System.Data.SqlClient;
using T = System.Threading;
using System.Text.RegularExpressions;

namespace MESACCA.DataBaseManagers
{
    public static class SQLManager
    {

        #region ConnectionStrings
        ///This method returns an ADO.NET connection string. 
        public static string GetSqlConnectionString()
        {
            return Common.GetSqlConnectionString();
        }

        //This method attempts to connect to the SQL database and returns a User object
        public static Users sqlConnection(String username, String password)
        {
            String _username = username;
            String _password = password;
            Users foundUser = null;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        System.Threading.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    foundUser = accessDatabase(_username, _password);
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

        #endregion

        #region User

        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided username and password and returns a User object with some information 
        private static Users accessDatabase(string username, string password)
        {
            String _username = username;
            String _password = password;
            Users foundUser = null;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Users WHERE Username = @username AND Password = @password";
                    dbCommand.Parameters.AddWithValue("@username", SecurityUtility.ParseSQL(_username));
                    dbCommand.Parameters.AddWithValue("@password", SecurityUtility.ParseSQL(_password));
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object and returning it
                    //I trim all of the found User data because the SQL server seems to add spaces.

                    foundUser = new Users();
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd();
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd();
                    //Closing SQL connectioon
                    sqlConnection.Close();
                }
                return foundUser;
            }
        }

        //This method invokes "accessDatabaseForUser" to attempt to connect to the SQL database and returns a User object
        public static User sqlConnectionForUser(int ID)
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
        private static User accessDatabaseForUser(int ID)
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
                    foundUser.MESA_Schools_Program = dataReader.GetString(11).TrimEnd(' ');
                    foundUser.MESA_Community_College_Program = dataReader.GetString(12).TrimEnd(' ');
                    foundUser.MESA_Engineering_Program = dataReader.GetString(13).TrimEnd(' ');
                    foundUser.News = dataReader.GetString(14).TrimEnd(' ');
                    foundUser.Donate = dataReader.GetString(15).TrimEnd(' ');
                    foundUser.Collaborations = dataReader.GetString(16).TrimEnd(' ');
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return foundUser;
            }
        }

        //This method invokes "updateUserDatabase" to attempt to connect to the SQL database and returns a Boolean value regarding update confirmation
        public static Boolean sqlConnectionUpdateUser(int ID, User updatedUser)
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
        private static Boolean updateUserDatabase(int ID, User updatedUser)
        {
            Boolean success = false;
            User foundUser = new User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"UPDATE Users 
                                              SET FirstName = @FirstName, LastName = @LastName, AccountType = @AccountType, Center = @Center, Email = @Email,
                                                  PhoneNumber = @PhoneNumber, Username = @Username, Password = @Password,
                                                  Home = @Home, About_Us = @About_Us,
                                                  MESA_Schools_Program = @MESA_Schools_Program,
                                                  MESA_Community_College_Program = @MESA_Community_College_Program, 
                                                  MESA_Engineering_Program = @MESA_Engineering_Program, 
                                                  News = @News, Donate = @Donate, Collaborations = @Collaborations
                                              WHERE ID = @ID
                                              SELECT * FROM Users WHERE ID = @ID";
                    //Updating User information based on comparison with current and new User information
                    //I trim the end of all fields to remove empty spaces
                    dbCommand.Parameters.AddWithValue("@FirstName", updatedUser.FirstName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@LastName", updatedUser.LastName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@AccountType", updatedUser.AccountType.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Center", updatedUser.Center.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Email", updatedUser.Email.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@PhoneNumber", updatedUser.PhoneNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@UserName", updatedUser.Username.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Password", updatedUser.Password.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Home", updatedUser.Home.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@About_Us", updatedUser.About_Us.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Schools_Program", updatedUser.MESA_Schools_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Community_College_Program", updatedUser.MESA_Community_College_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Engineering_Program", updatedUser.MESA_Engineering_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@News", updatedUser.News.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Donate", updatedUser.Donate.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Collaborations", updatedUser.Collaborations.TrimEnd(' '));
                    //Specifing update by ID number to ensure correct User's information is updated
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //Getting the updated SQL entry information for comparison testing to verify the update was successful
                    //I trim all of the found User data because the SQL server seems to add spaces.
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
                    foundUser.MESA_Schools_Program = dataReader.GetString(11).TrimEnd(' ');
                    foundUser.MESA_Community_College_Program = dataReader.GetString(12).TrimEnd(' ');
                    foundUser.MESA_Engineering_Program = dataReader.GetString(13).TrimEnd(' ');
                    foundUser.News = dataReader.GetString(14).TrimEnd(' ');
                    foundUser.Donate = dataReader.GetString(15).TrimEnd(' ');
                    foundUser.Collaborations = dataReader.GetString(16).TrimEnd(' ');
                    //Determining if the update was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the updated information provided by the user.
                    if (dataReader.HasRows == true && updatedUser.FirstName.TrimEnd(' ').Equals(foundUser.FirstName) &&
                        updatedUser.LastName.TrimEnd(' ').Equals(foundUser.LastName) &&
                        updatedUser.AccountType.TrimEnd(' ').Equals(foundUser.AccountType) &&
                        updatedUser.Center.TrimEnd(' ').Equals(foundUser.Center) &&
                        updatedUser.Email.TrimEnd(' ').Equals(foundUser.Email) &&
                        updatedUser.PhoneNumber.TrimEnd(' ').Equals(foundUser.PhoneNumber) &&
                        updatedUser.Username.TrimEnd(' ').Equals(foundUser.Username) &&
                        updatedUser.Password.TrimEnd(' ').Equals(foundUser.Password) &&
                        updatedUser.Home.Equals(foundUser.Home) &&
                        updatedUser.About_Us.Equals(foundUser.About_Us) &&
                        updatedUser.Collaborations.Equals(foundUser.Collaborations) &&
                        updatedUser.MESA_Schools_Program.Equals(foundUser.MESA_Schools_Program) &&
                        updatedUser.MESA_Community_College_Program.Equals(foundUser.MESA_Community_College_Program) &&
                        updatedUser.MESA_Engineering_Program.Equals(foundUser.MESA_Engineering_Program) &&
                        updatedUser.News.Equals(foundUser.News) &&
                        updatedUser.Donate.Equals(foundUser.Donate))
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
        public static List<User> sqlConnectionForUsersList()
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
        private static List<User> accessDatabaseForUsers()
        {
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
        public static Boolean sqlConnectionAddUser(int newID, User newUser)
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
        private static Boolean accessDatabaseToAddUser(int newID, User newUser)
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
                                                                 Home, About_Us, MESA_Schools_Program, MESA_Community_College_Program, 
                                                                 MESA_Engineering_Program, News, Donate, Collaborations)
                                              Values (@ID, @FirstName, @LastName, @AccountType, @Center, @Email, @PhoneNumber, @Username, @Password,
                                                      @Home, @About_Us, @MESA_Schools_Program, @MESA_Community_College_Program, 
                                                      @MESA_Engineering_Program, @News, @Donate, @Collaborations)
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
                    dbCommand.Parameters.AddWithValue("@MESA_Schools_Program", newUser.MESA_Schools_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Community_College_Program", newUser.MESA_Community_College_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@MESA_Engineering_Program", newUser.MESA_Engineering_Program.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@News", newUser.News.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Donate", newUser.Donate.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Collaborations", newUser.Collaborations.TrimEnd(' '));
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
                    if (dataReader.HasRows == true && newUser.FirstName.TrimEnd(' ').Equals(foundUser.FirstName) &&
                        newUser.LastName.TrimEnd(' ').Equals(foundUser.LastName) &&
                        newUser.Center.TrimEnd(' ').Equals(foundUser.Center) &&
                        newUser.Email.TrimEnd(' ').Equals(foundUser.Email) &&
                        newUser.PhoneNumber.TrimEnd(' ').Equals(foundUser.PhoneNumber) &&
                        newUser.Username.TrimEnd(' ').Equals(foundUser.Username) &&
                        newUser.Password.TrimEnd(' ').Equals(foundUser.Password))
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
        public static Boolean sqlConnectionDeleteUser(int ID)
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
        private static Boolean accessDatabaseToDeleteUser(int ID)
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

        #endregion

        #region Centers

        //This method invokes "accessDatabaseForCenter" to attempt to connect to the SQL database and returns a Center object
        public static Models.Center sqlConnectionForCenter(int ID)
        {
            Models.Center foundCenter = new Models.Center();
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 5;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    foundCenter = accessDatabaseForCenter(ID);
                    //Break if a center from the SQL database was found 
                    if (foundCenter.CenterType != null)
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
            return foundCenter;
        }

        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided ID and returns a Center object with all information of the Center
        private static Models.Center accessDatabaseForCenter(int ID)
        {
            Models.Center foundCenter = new Models.Center();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Centers WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Getting the SQL entry information 
                    //I trim all of the found Center data because the SQL server seems to add spaces.
                    foundCenter.ID = dataReader.GetInt32(0);
                    foundCenter.Name = dataReader.GetString(1).TrimEnd(' ');
                    foundCenter.Address = dataReader.GetString(2).TrimEnd(' ');
                    foundCenter.Location = dataReader.GetString(3).TrimEnd(' ');
                    foundCenter.CenterType = dataReader.GetString(4).TrimEnd(' ');
                    foundCenter.DirectorName = dataReader.GetString(5).TrimEnd(' ');
                    foundCenter.OfficeNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundCenter.URL = dataReader.GetString(7).TrimEnd(' ');
                    foundCenter.Description = dataReader.GetString(8).TrimEnd(' ');
                    foundCenter.ImageURL = dataReader.GetString(9).TrimEnd(' ');
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return foundCenter;
            }
        }

        //This method invokes "accessDatabaseForCenter" to attempt to connect to the SQL database and returns a Center object
        public static Models.Center sqlConnectionForCenter(string name)
        {
            Models.Center foundCenter = new Models.Center();
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 5;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    foundCenter = accessDatabaseForCenter(name);
                    //Break if a center from the SQL database was found 
                    if (foundCenter.CenterType != null)
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
            return foundCenter;
        }

        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided name and returns a Center object with all information of the Center
        private static Models.Center accessDatabaseForCenter(string name)
        {
            Models.Center foundCenter = new Models.Center();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Centers WHERE Name = @Name";
                    dbCommand.Parameters.AddWithValue("@Name", name);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Getting the SQL entry information 
                    //I trim all of the found Center data because the SQL server seems to add spaces.
                    foundCenter.ID = dataReader.GetInt32(0);
                    foundCenter.Name = dataReader.GetString(1).TrimEnd(' ');
                    foundCenter.Address = dataReader.GetString(2).TrimEnd(' ');
                    foundCenter.Location = dataReader.GetString(3).TrimEnd(' ');
                    foundCenter.CenterType = dataReader.GetString(4).TrimEnd(' ');
                    foundCenter.DirectorName = dataReader.GetString(5).TrimEnd(' ');
                    foundCenter.OfficeNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundCenter.URL = dataReader.GetString(7).TrimEnd(' ');
                    foundCenter.Description = dataReader.GetString(8).TrimEnd(' ');
                    foundCenter.ImageURL = dataReader.GetString(9).TrimEnd(' ');
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return foundCenter;
            }
        }

        //This method invokes "accessDatabaseForCenters" to attempt to connect to the SQL database and returns a List object containing all Centers
        public static List<Models.Center> sqlConnectionForCentersList()
        {
            List<Models.Center> centerList = new List<Models.Center>();
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
                    centerList = accessDatabaseForCenters();
                    //Break if the List object is not empty
                    if (centerList.Count > 0)
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
            return centerList;
        }

        //This method connects to the database, collects all the entries in the Centers table into a list
        //and returns a List object.
        private static List<Models.Center> accessDatabaseForCenters()
        {
            List<Models.Center> centerList = new List<Models.Center>();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Centers";
                    var dataReader = dbCommand.ExecuteReader();
                    var iterator = dataReader.GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        Models.Center foundCenter = new Models.Center();
                        //Getting the SQL entry information 
                        //I trim all of the found User data because the SQL server seems to add spaces.
                        foundCenter.ID = dataReader.GetInt32(0);
                        foundCenter.Name = dataReader.GetString(1).TrimEnd(' ');
                        foundCenter.Address = dataReader.GetString(2).TrimEnd(' ');
                        foundCenter.Location = dataReader.GetString(3).TrimEnd(' ');
                        foundCenter.CenterType = dataReader.GetString(4).TrimEnd(' ');
                        foundCenter.DirectorName = dataReader.GetString(5).TrimEnd(' ');
                        foundCenter.OfficeNumber = dataReader.GetString(6).TrimEnd(' ');
                        foundCenter.URL = dataReader.GetString(7).TrimEnd(' ');
                        foundCenter.Description = dataReader.GetString(8).TrimEnd(' ');
                        foundCenter.ImageURL = dataReader.GetString(9).TrimEnd(' ');
                        centerList.Add(foundCenter);
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return centerList;
            }
        }

        public static Boolean sqlConnectionAddCenter(Models.Center newCenter)
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
                    success = accessDatabaseToAddCenter(newCenter);
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
        
        //This method connects to the database, adds to the Centers table, checks for rows affected,
        //and returns Boolean value regarding success
        private static Boolean accessDatabaseToAddCenter(Models.Center newCenter)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"INSERT INTO Centers (Name, Address, Location, CenterType, DirectorName,
														           OfficeNumber, URL, Description, ImageURL)
									          Values (@Name, @Address, @Location, @CenterType,
											          @DirectorName, @OfficeNumber, @URL, @Description, @ImageURL)";
                    //I trim the ends of empty spaces
                    dbCommand.Parameters.AddWithValue("@Name", newCenter.Name.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Address", newCenter.Address.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Location", newCenter.Location.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@CenterType", newCenter.CenterType.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@DirectorName", newCenter.DirectorName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@OfficeNumber", newCenter.OfficeNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@URL", newCenter.URL.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Description", newCenter.Description.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@ImageURL", newCenter.ImageURL);
                    //Building data reader
                    int dataReader = dbCommand.ExecuteNonQuery();

                    if (dataReader == 1)
                        success = true;
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }

        //This method invokes "updateCenterDatabase" to attempt to connect to the SQL database and returns a Boolean value regarding update confirmation
        public static Boolean sqlConnectionUpdateCenter(int ID, Models.Center updatedCenter)
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
                    success = accessDatabaseToEditCenter(ID, updatedCenter);
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

        //This method connects to the database, updates the specified SQL entry by the Center's ID, collects the 
        //SQL entry for comparison and return a Boolean value based on the comparisons performed.
        private static Boolean accessDatabaseToEditCenter(int ID, Models.Center updatedCenter)
        {
            Boolean success = false;
            Models.Center foundCenter = new Models.Center();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"UPDATE Users
                                              SET Center = @Name
                                              WHERE Center IN (SELECT [Name] FROM Centers WHERE ID = @ID)
                                              UPDATE Centers 
									          SET Name = @Name, Address = @Address, Location = @Location, CenterType = @CenterType, DirectorName = @DirectorName,
										      OfficeNumber = @OfficeNumber, URL = @URL, Description = @Description, ImageURL = @ImageURL
									          WHERE ID = @ID
									          SELECT * FROM Centers WHERE ID = @ID";
                    //Updating User information based on comparison with current and new User information
                    //I trim the end of all fields to remove empty spaces
                    dbCommand.Parameters.AddWithValue("@Name", updatedCenter.Name.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Address", updatedCenter.Address.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Location", updatedCenter.Location.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@CenterType", updatedCenter.CenterType.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@DirectorName", updatedCenter.DirectorName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@OfficeNumber", updatedCenter.OfficeNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@URL", updatedCenter.URL.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Description", updatedCenter.Description.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@ImageURL", updatedCenter.ImageURL);
                    //Specifing update by ID number to ensure correct Center's information is updated
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //Getting the updated SQL entry information for comparison testing to verify the update was successful
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    foundCenter.ID = dataReader.GetInt32(0);
                    foundCenter.Name = dataReader.GetString(1).TrimEnd(' ');
                    foundCenter.Address = dataReader.GetString(2).TrimEnd(' ');
                    foundCenter.Location = dataReader.GetString(3).TrimEnd(' ');
                    foundCenter.CenterType = dataReader.GetString(4).TrimEnd(' ');
                    foundCenter.DirectorName = dataReader.GetString(5).TrimEnd(' ');
                    foundCenter.OfficeNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundCenter.URL = dataReader.GetString(7).TrimEnd(' ');
                    foundCenter.Description = dataReader.GetString(8).TrimEnd(' ');
                    //Determining if the update was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the updated information provided by the user.
                    if (dataReader.HasRows == true && updatedCenter.Name.TrimEnd(' ').Equals(foundCenter.Name) &&
                        updatedCenter.Address.TrimEnd(' ').Equals(foundCenter.Address) &&
                        updatedCenter.Location.TrimEnd(' ').Equals(foundCenter.Location) &&
                        updatedCenter.CenterType.TrimEnd(' ').Equals(foundCenter.CenterType) &&
                        updatedCenter.DirectorName.TrimEnd(' ').Equals(foundCenter.DirectorName) &&
                        updatedCenter.OfficeNumber.TrimEnd(' ').Equals(foundCenter.OfficeNumber) &&
                        updatedCenter.URL.TrimEnd(' ').Equals(foundCenter.URL) &&
                        updatedCenter.Description.TrimEnd(' ').Equals(foundCenter.Description))
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
       
        //This method invokes "accessDatabaseToDeleteCenter" to attempt to connect to the SQL database and returns a Boolean value regarding deletion confirmation
        public static Boolean sqlConnectionDeleteCenter(int ID)
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
                    success = accessDatabaseToDeleteCenter(ID);
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
        private static Boolean accessDatabaseToDeleteCenter(int ID)
        {
            Boolean success = false;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"DELETE FROM Users
                                              WHERE Center IN (SELECT [Name] FROM Centers WHERE ID = @ID) AND AccountType != 'Admin'
                                              DELETE FROM Centers WHERE ID = @ID
									          SELECT * FROM Centers WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //If the Center can't be found, then the Center was successfully deleted 
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

        #endregion

        #region News

        public static List<NewsArticle> getNewsPosts()
        {
            List<NewsArticle> returnValue = new List<NewsArticle>();
            try
            {
                using (var sqlConnection = new S.SqlConnection(Common.GetSqlConnectionString()))
                {
                    using (var dbCommand = sqlConnection.CreateCommand())
                    {

                        //Opening SQL connection
                        sqlConnection.Open();
                        //Creating SQL query that updates the SQL table entry and returns the updated table entry
                        dbCommand.CommandText = @"SELECT n.ArticleTitle, n.ArticleBody, n.DateOfArticle, u.FirstName, u.LastName, n.Attach1URL  FROM (SELECT top 100 * FROM NewsArticles order by DateOfArticle desc) as n INNER JOIN Users as u on u.ID = n.CreatedByUser";
                        var dataReader = dbCommand.ExecuteReader();
                        var iterator = dataReader.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            NewsArticleExtension article = new NewsArticleExtension();
                            //Getting the SQL entry information 
                            //I trim all of the found User data because the SQL server seems to add spaces.
                            article.ArticleTitle = dataReader.GetString(0).TrimEnd(' ');
                            article.ArticleBody = dataReader.GetString(1).TrimEnd(' ');
                            article.DateOfArticle = dataReader.GetDateTime(2);
                            article.AuthorName = dataReader.GetString(3).TrimEnd(' ') + " " + dataReader.GetString(4).TrimEnd(' ');
                            article.Attach1URL = dataReader.GetString(5).TrimEnd(' ');
                            article.fileName = article.Attach1URL.Split('/').Last();
                            String readthis = article.fileName;
                            System.Diagnostics.Debug.WriteLine(article.Attach1URL);
                            returnValue.Add(article);
                        }
                        //Closing SQL connection
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return returnValue;

        }

        public static List<NewsArticleExtension> getNewsPostsForAdmin()
        {
            List<NewsArticleExtension> returnValue = new List<NewsArticleExtension>();
            try
            {
                using (var sqlConnection = new S.SqlConnection(Common.GetSqlConnectionString()))
                {
                    using (var dbCommand = sqlConnection.CreateCommand())
                    {

                        //Opening SQL connection
                        sqlConnection.Open();
                        //Creating SQL query that updates the SQL table entry and returns the updated table entry
                        dbCommand.CommandText = @"SELECT n.ArticleID, n.ArticleTitle, n.ArticleBody, n.DateOfArticle, u.FirstName, u.LastName, n.Attach1URL FROM NewsArticles as n INNER JOIN Users as u on u.ID = n.CreatedByUser order by DateOfArticle desc";
                        var dataReader = dbCommand.ExecuteReader();
                        var iterator = dataReader.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            NewsArticleExtension article = new NewsArticleExtension();
                            //Getting the SQL entry information 
                            //I trim all of the found User data because the SQL server seems to add spaces.
                            article.ArticleID = dataReader.GetInt32(0);
                            article.ArticleTitle = dataReader.GetString(1);
                            //if article body text is less than 50 chars long return that, else return the first 50
                            string articleBodyFormatted = dataReader.GetString(2);
                            articleBodyFormatted = Regex.Replace(articleBodyFormatted, "<.*?>", string.Empty);
                            article.ArticleBody = articleBodyFormatted.Length < 50 ? articleBodyFormatted : articleBodyFormatted.Substring(0, 50) + "...";
                            article.DateOfArticle = dataReader.GetDateTime(3);
                            article.AuthorName = dataReader.GetString(4) + " " + dataReader.GetString(5);
                            article.Attach1URL = dataReader.GetString(6).TrimEnd(' ');
                            //if(article.Attach1URL )
                            article.fileName = article.Attach1URL.Split('/').Last();
                            String readthis = article.fileName;
                            System.Diagnostics.Debug.WriteLine(article.Attach1URL);
                            returnValue.Add(article);
                        }
                        //Closing SQL connection
                        sqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return returnValue;

        }

        public static Boolean sqlConnectionDeleteNews(int ID)
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
                    success = accessDatabaseToDeleteNews(ID);
                    //Break if new post from the SQL database was found to be gone
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

        private static Boolean accessDatabaseToDeleteNews(int ID)
        {
            Boolean success = false;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @" DELETE FROM NewsArticles WHERE ArticleID = @ID
                                               SELECT * FROM NewsArticles WHERE ArticleID = @ID";
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

        public static Boolean sqlConnectionAddNews(NewsArticle na)
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
                    success = accessDatabaseToAddNews(na);
                    //Break if new post from the SQL database was found to be gone
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

        private static Boolean accessDatabaseToAddNews(NewsArticle na)
        {
            Boolean success = false;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"INSERT INTO NewsArticles (ArticleTitle, ArticleBody, CreatedByUser, DateofArticle, Attach1URL)
                                              Values (@ArticleTitle, @ArticleBody, @CreatedByUser, @DateofArticle, @Attach1URLL)";

                    dbCommand.Parameters.AddWithValue("@ArticleTitle", na.ArticleTitle);
                    dbCommand.Parameters.AddWithValue("@ArticleBody", na.ArticleBody);
                    dbCommand.Parameters.AddWithValue("@CreatedByUser", na.CreatedByUser);
                    dbCommand.Parameters.AddWithValue("@DateofArticle", na.DateOfArticle);
                    dbCommand.Parameters.AddWithValue("@Attach1URLL", na.Attach1URL);

                    //Building data reader
                    int dataReader = dbCommand.ExecuteNonQuery();

                    if (dataReader == 1)
                        success = true;
                }
                //Closing SQL connection
                sqlConnection.Close();
            }
            return success;
        }

        public static NewsArticle sqlConnectionGetNews(int id)
        {
            NewsArticle na = new NewsArticle();
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
                    na = accessDatabaseToGetNews(id);
                    //Break if an account from the SQL database was found 
                    if (na.ArticleID != 0)
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
            return na;
        }

        private static NewsArticle accessDatabaseToGetNews(int id)
        {
            NewsArticle na = new NewsArticle();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT ArticleID, ArticleTitle, ArticleBody, DateOfArticle, Attach1URL FROM NewsArticles WHERE ArticleID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", id);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object and returning it
                    na.ArticleID = dataReader.GetInt32(0);
                    na.ArticleTitle = dataReader.GetString(1);
                    na.ArticleBody = dataReader.GetString(2);
                    na.DateOfArticle = dataReader.GetDateTime(3);
                    na.Attach1URL = dataReader.GetString(4);
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return na;
            }
        }

        public static Boolean sqlConnectionEditNews(NewsArticle na)
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
                    success = accessDatabaseToEditNews(na);
                    //Break if new post from the SQL database was found to be gone
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

        private static Boolean accessDatabaseToEditNews(NewsArticle na)
        {
            Boolean success = false;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"UPDATE NewsArticles 
                                            SET ArticleTitle = @ArticleTitle, ArticleBody = @ArticleBody, CreatedByUser = @CreatedByUser, DateofArticle = @DateofArticle, Attach1URL = @Attach1URL
                                            WHERE ArticleID = @id";

                    dbCommand.Parameters.AddWithValue("@ArticleTitle", na.ArticleTitle);
                    dbCommand.Parameters.AddWithValue("@ArticleBody", na.ArticleBody);
                    dbCommand.Parameters.AddWithValue("@CreatedByUser", na.CreatedByUser);
                    dbCommand.Parameters.AddWithValue("@DateofArticle", na.DateOfArticle);
                    dbCommand.Parameters.AddWithValue("@id", na.ArticleID);
                    dbCommand.Parameters.AddWithValue("@Attach1URL", na.Attach1URL);
                    //Building data reader
                    int dataReader = dbCommand.ExecuteNonQuery();

                    if (dataReader == 1)
                        success = true;
                }
                //Closing SQL connection
                sqlConnection.Close();
            }
            return success;
        }
        #endregion

        #region Donation

        public static DonationViewModel sqlConnectionGetDonation()
        {
            DonationViewModel na = null;
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
                    na = accessDatabaseGetDonation();
                    //Break if an account from the SQL database was found 
                    if (na != null)
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
            return na;
        }

        private static DonationViewModel accessDatabaseGetDonation()
        {
            DonationViewModel na = null;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT top 1 * from Donation";
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();

                    na = new DonationViewModel();

                    na.ArticleBody = dataReader.GetString(0);
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return na;
            }
        }
        
        public static Boolean sqlConnectionUpdateDonation(DonationViewModel na)
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
                    success = accessDatabaseUpdateDonation(na);
                    //Break if new post from the SQL database was found to be gone
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

        private static Boolean accessDatabaseUpdateDonation(DonationViewModel na)
        {
            Boolean success = false;
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"UPDATE Donation set Body = @ArticleBody";

                    dbCommand.Parameters.AddWithValue("@ArticleBody", SecurityUtility.ParseSQL(na.ArticleBody));

                    //Building data reader
                    int dataReader = dbCommand.ExecuteNonQuery();

                    if (dataReader == 1)
                        success = true;
                }
                //Closing SQL connection
                sqlConnection.Close();
            }
            return success;
        }


        #endregion
    }
}