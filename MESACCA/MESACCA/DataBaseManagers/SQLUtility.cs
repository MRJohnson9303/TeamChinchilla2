using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MESACCA.Utilities;
using MESACCA.Models;
using System.Data.SqlClient;
using System.Threading;

namespace MESACCA.DataBaseManagers
{
    public static class SQLUtility
    {
        //This method attempts to connect to the SQL database and returns a User object
        public static User sqlConnection(String username, String password)
        {
            String _username = username;
            String _password = password;
            User foundUser = new Models.User();
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
        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided username and password and returns a User object with some information 
        public static User accessDatabase(string username, string password)
        {
            String _username = username;
            String _password = password;
            User foundUser = new Models.User();
            using (var sqlConnection = new System.Data.SqlClient.SqlConnection(GetSqlConnectionString()))
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

        ///This method returns an ADO.NET connection string. 
        public static string GetSqlConnectionString()
        {
            return Common.GetSqlConnectionString();
        }
        
    }
}