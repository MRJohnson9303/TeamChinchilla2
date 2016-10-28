using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MESACCA.Models
{
    public class Center
    {
        //ID used by database
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string CenterType { get; set; }
        public string DirectorName { get; set; }
        public string OfficeNumber { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public HttpPostedFileBase Picture { get; set; }
        public string ImageURL { get; set; }
    }
}
