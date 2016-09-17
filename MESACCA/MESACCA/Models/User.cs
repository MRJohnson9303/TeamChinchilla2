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
    }
}
