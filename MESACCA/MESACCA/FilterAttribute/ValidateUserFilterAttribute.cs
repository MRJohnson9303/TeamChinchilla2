using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using MESACCA.Utilities;

namespace MESACCA.FilterAttributes
{
    public class ValidateUser : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            
            if (!SecurityUtility.IsUserSessionValid())
            {
                SecurityUtility.baseLogOut();
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}