using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.Member
{
    public class ManagePersonalAccountViewModel
    {
        [Required]
        [StringLength(35)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(35)]
        public string LastName { get; set; }
        public string AccountType { get; set; }
        [Required]
        [StringLength(50)]
        public string Center { get; set; }
        [Required]
        [StringLength(30)]
        public string Email { get; set; }
        [Required]
        [StringLength(12)]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(20)]
        public string Username { get; set; }
        public string CurrentUsername { get; set; }
        [StringLength(20)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string CurrentPassword { get; set; }
        /* These are the user rights that will allow a user to alter a certain page of the website*/
        public String Home { get; set; }
        public String About_Us { get; set; }
        public String Collaborations { get; set; }
        public String MESA_Schools_Program { get; set; }
        public String MESA_Community_College_Program { get; set; }
        public String MESA_Engineering_Program { get; set; }
        public String News { get; set; }
        public String Donate { get; set; }
    }
}
