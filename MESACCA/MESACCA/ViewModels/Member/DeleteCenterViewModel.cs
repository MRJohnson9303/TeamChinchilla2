using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MESACCA.ViewModels.Member
{
    public class DeleteCenterViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string CenterType { get; set; }
        public string DirectorName { get; set; }
        public string OfficeNumber { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
    }
}