using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MESACCA;

namespace MESACCA.Utilities
{
    public static class MyUserManager
    {
        private static Dictionary<Guid, Users> UserDictionary = new Dictionary<Guid, Users>();
        private static Object thisLock = new Object();

        public static bool LoginUser(Users u)
        {
            Guid Key;
            Guid.TryParse(System.Web.HttpContext.Current.User.Identity.Name, out Key);

            lock (thisLock)
            {
                if (UserDictionary.ContainsKey(Key) || UserDictionary.ContainsValue(u))
                {
                    // Do not allow multiple log ins
                    return false;
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


        public static Users GetUser()
        {
            if (!(System.Web.HttpContext.Current.User.Identity.IsAuthenticated)) return null;

            Guid Key;
            Guid.TryParse(System.Web.HttpContext.Current.User.Identity.Name, out Key);

            lock (thisLock)
            {
                Users user;
                UserDictionary.TryGetValue(Key, out user);
                return user;
            }

        }
            
    }
}