using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MESACCA;
using System.Data.SqlClient;
using MESACCA.Utilities;

namespace MESACCA.DataBaseManagers
{
    public static class NewsArticleManager
    {
        public static List<NewsArticle> getNewsPosts()
        {
            List<NewsArticle> returnValue = new List<NewsArticle>();
            try
            {
                using (var sqlConnection = new System.Data.SqlClient.SqlConnection(Common.GetSqlConnectionString()))
                {
                    using (var dbCommand = sqlConnection.CreateCommand())
                    {
                        
                        //Opening SQL connection
                        sqlConnection.Open();
                        //Creating SQL query that updates the SQL table entry and returns the updated table entry
                        dbCommand.CommandText = @"SELECT n.ArticleTitle, n.ArticleBody, n.DateOfArticle, u.FirstName, u.LastName FROM (SELECT top 100 * FROM NewsArticles order by DateOfArticle desc) as n INNER JOIN Users as u on u.ID = n.CreatedByUser";
                        var dataReader = dbCommand.ExecuteReader();
                        var iterator = dataReader.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            NewsArticle article = new NewsArticle();
                            //Getting the SQL entry information 
                            //I trim all of the found User data because the SQL server seems to add spaces.
                            article.ArticleTitle = dataReader.GetString(0).TrimEnd(' ');
                            article.ArticleBody = dataReader.GetString(1).TrimEnd(' ');
                            article.DateOfArticle = dataReader.GetDateTime(2);
                            article.AuthorName = dataReader.GetString(3).TrimEnd(' ') + " " + dataReader.GetString(4).TrimEnd(' ');
                            returnValue.Add(article);
                        }
                        //Closing SQL connection
                        sqlConnection.Close();
                    }    
                }
            } catch(Exception ex)
            {

            }

            return returnValue;
           
            /*
            List<NewsArticle> returnValue = new List<NewsArticle>();
            for (int i = 1; i < 5; i++)
            {
                var newsArticle = new NewsArticle()
                {
                    ArticleTitle = "This is a test title",
                    ArticleBody = "I have panel which I colored blue if this panel is being selected (clicked on it). Additionally, I add a small sign (.png image) to that panel, which indicates that the selected panel has been already selected before. So if the user sees for example 10 panels and 4 of them have this small sign, he knows that he has already clicked on those panels before.This work fine so far.The problem is now that I can't display the small sign and make the panel blue at the same time. I set the panel to blue with the css background: #6DB3F2; and the background image with background-image: url('images/checked.png'). But it seems that the background color is above the image so you cannot see the sign. Is it therefore possible to set z - indexes for the background color and the background image ? ",
                    DateOfArticle = DateTime.Now,
                    CreatedByUser = 1,
                    AuthorName = "Test Author Thadius"
                };

                returnValue.Add(newsArticle);
            }
            return returnValue;
            */
        }
    }
}