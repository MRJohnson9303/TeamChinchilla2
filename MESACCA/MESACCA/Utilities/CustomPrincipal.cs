using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using MESACCA.Models;

namespace MESACCA.Utilities
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }
        public UserRoles Role { get; }

        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(Users user)
        {
            this.Identity = new GenericIdentity(Guid.NewGuid().ToString());

            UserRoles role = (UserRoles)Enum.Parse(typeof(UserRoles), user.AccountType.ToLower());
            if (Enum.IsDefined(typeof(UserRoles), role))
            {
                Role = role;
            }
        }
        
    }
}