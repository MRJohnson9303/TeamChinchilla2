using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MESACCA.ViewModels.Member
{
    public class AddCenterViewModel
    {
        //ID used by database
        public int ID { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Address { get; set; }
        [Required]
        [StringLength(30)]
        public string Location { get; set; }
        [Required]
        [StringLength(4)]
        public string CenterType { get; set; }
        [Required]
        [StringLength(71)]
        public string DirectorName { get; set; }
        [Required]
        [StringLength(12)]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string OfficeNumber { get; set; }
        [Required]
        [StringLength(30)]
        public string URL { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
        [Required]
        public HttpPostedFileBase Picture { get; set; }
    }
}

