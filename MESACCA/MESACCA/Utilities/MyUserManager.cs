using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MESACCA;
using MESACCA.Models;
namespace MESACCA.Utilities
{
    public static class MyUserManager
    {
        private static Dictionary<Guid, User> UserDictionary = new Dictionary<Guid, User>();
        private static Object thisLock = new Object();

        public static bool LoginUser(User u)
        {
            Guid Key;
            Guid.TryParse(System.Web.HttpContext.Current.User.Identity.Name, out Key);

            lock (thisLock)
            {
                if (UserDictionary.ContainsKey(Key) || UserDictionary.ContainsValue(u))
                {
                    //#TODO Test if the entry is removed when user closed browser or tab
                    // Do not allow multiple log ins for same user
                    //return false;
                }

                UserDictionary.Add(Key, u);
                return true;
            }
        }

        public static void LogOutUser()
        {

            Guid Key;
            Guid.TryParse(System.Web.HttpContext.Current.User.Identity.Name, out Key);

            lock (thisLock)
            {
                UserDictionary.Remove(Key);
            }
        }


        public static User GetUser()
        {
            if (!(System.Web.HttpContext.Current.User.Identity.IsAuthenticated)) return null;

            Guid Key;
            Guid.TryParse(System.Web.HttpContext.Current.User.Identity.Name, out Key);

            lock (thisLock)
            {
                User user;
                UserDictionary.TryGetValue(Key, out user);
                return user;
            }

        }
            
    }
}