﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.Member
{
    public class ManagePersonalAccountViewModel
    {
        [Required]
        [StringLength(12)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(12)]
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
        [StringLength(12)]
        public string Username { get; set; }
        [StringLength(20)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
