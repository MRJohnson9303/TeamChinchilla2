using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MESACCA.Models
{
    public class Picture
    {
        public HttpPostedFileBase File { get; set; }
        /*public override int ContentLength { get; }
        public override string ContentType { get; }
        public override string FileName { get; }
        public override Stream InputStream { get;}*/
    }
}
