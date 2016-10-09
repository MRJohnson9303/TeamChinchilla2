using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace MESACCA.Utilities
{
    interface ICustomPrincipal : IPrincipal
    {
        int Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }
}
