using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MESACCA.ViewModels.Member
{
    public class DeleteAccountViewModel
    {
        public int ID { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Account Type")]
        public string AccountType { get; set; }
        public string Center { get; set; }
        public string Email { get; set; }
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
    }
}