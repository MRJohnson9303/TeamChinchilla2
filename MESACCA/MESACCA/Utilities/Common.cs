using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace MESACCA.Utilities
{
    public enum UserRoles { director, admin, staff, };

    public static class Common
    { 
        public static string GetSqlConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;
        }

    }
}