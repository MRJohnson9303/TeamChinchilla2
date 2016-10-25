using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MESACCA.ViewModels.Member
{
    public class AddBlobViewModel
    {

        public string Blob_Name { get; set; }

        [Required]
        public string Uri_Name { get; set; }


        public string Container_Name { get; set; }
    }
}