using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;

namespace DietJournal.Web.Controllers
{
    public class BaseController : Controller
    {
        private Guid? _CurrentUserId;

        public Guid? CurrentUserId
        {
            get
            {
                if (_CurrentUserId != null)
                    return _CurrentUserId;

                if (!String.IsNullOrEmpty(CurrentUserName))
                {
                    var member = Membership.Provider.GetUser(HttpContext.User.Identity.Name, true);
                    if (member != null)
                        _CurrentUserId = (Guid)member.ProviderUserKey;
                }

                return _CurrentUserId;
            }
        }

        public string CurrentUserName
        {
            get
            {
                if (HttpContext != null && HttpContext.User != null && HttpContext.User.Identity != null)
                    return HttpContext.User.Identity.Name;
                
                return string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return HttpContext != null && HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated;
            }
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action"); ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw); 
                
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
