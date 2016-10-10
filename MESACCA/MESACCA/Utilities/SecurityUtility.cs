using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using MESACCA.Models;

namespace MESACCA.Utilities
{
    public static class SecurityUtility
    {
        public static string ParseSQL(string input)
        {
            input.Replace("'","\\'").Replace("\\","\\\\");
            return input;
        }

        public static bool IsUserSessionValid()
        {
            return (System.Web.HttpContext.Current.User != null 
                && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
        }
        
        public static void baseLogOut()
        {
            FormsAuthentication.SignOut();
            MyUserManager.LogOutUser();
        }

    }
}