using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.Models
{
    public class User
    {
        //ID used by database
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountType { get; set; }
        public string Center { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        /* These are the user rights that will allow a user to alter a certain page of the website*/
        public string Home { get; set; }
        public string About_Us { get; set; }
        public string Vision_Mission_Values { get; set; }
        public string MESA_Schools_Program { get; set; }
        public string MESA_Community_College_Program { get; set; }
        public string MESA_Engineering_Program { get; set; }
        public string News { get; set; }
        public string Donate { get; set; }
    }
}
