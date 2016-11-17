using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.Member
{
    public class EditAccountViewModel
    {
        public int ID { get; set; }
        [Required]
        [StringLength(35)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(35)]
        public string LastName { get; set; }
        [Required]
        public string AccountType { get; set; }
        [Required]
        [StringLength(50)]
        public string Center { get; set; }
        public string CurrentCenter { get; set; }
        [Required]
        [StringLength(30)]
        public string Email { get; set; }
        [Required]
        [StringLength(12)]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
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
        //These are user rights that will allow a particular user to alter the named web page of the MCCA website
        public Boolean Home { get; set; }
        public Boolean About_Us { get; set; }
        public Boolean Vision_Mission_Values { get; set; }
        public Boolean MESA_Schools_Program { get; set; }
        public Boolean MESA_Community_College_Program { get; set; }
        public Boolean MESA_Engineering_Program { get; set; }
        public Boolean News { get; set; }
        public Boolean Donate { get; set; }
    }
}

