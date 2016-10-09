using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace MESACCA.Utilities
{
    public class CustomPrincipal : ICustomPrincipal
    {
        public IIdentity Identity { get; private set; }
        public UserRoles Role { get; }
           
        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(string key)
        {
            this.Identity = new GenericIdentity(key);
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}