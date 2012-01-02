using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Configuration;

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

        public ProfileSetting CurrentProfileSettings
        {
            get
            {
                if (String.IsNullOrEmpty(CurrentUserName))
                    return new ProfileSetting();

                var profileSettings = HttpContext.Cache[CurrentUserName] as ProfileSetting;
                if (profileSettings != null)
                    return profileSettings;

                var membership = Membership.GetUser(CurrentUserName);
                if (membership == null)
                    return new ProfileSetting();

                using (var context = new DietJournalEntities())
                {
                    profileSettings = context.ProfileSettings.FirstOrDefault(s => s.UserId == (Guid)membership.ProviderUserKey);
                }

                return profileSettings != null ? profileSettings : new ProfileSetting();
            }
            set
            {
                if (string.IsNullOrEmpty(CurrentUserName))
                    return;

                HttpContext.Cache[CurrentUserName] = value;
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

        protected List<DietPlan> DietPlans
        {
            get
            {
                var key = "DietPlans";
                var plans = HttpContext.Cache[key] as List<DietPlan>;
                if (plans == null)
                {
                    using (var entities = new DietJournalEntities())
                    {
                        plans = (from d in entities.DietPlans
                                where d.ParentId == null
                                select d).ToList();

                        plans.ForEach(p => p.DietPlans.ToList());
                    }

                    HttpContext.Cache.Insert(key, plans);
                }

                return plans;
            }
        }

        //protected override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    //Only check if we are already on a secure connectuion and    
        //    // we don't have a [RequireHttpsAttribute] defined   
        //    if (Request.IsSecureConnection)
        //    {
        //        var requireHttps = filterContext.ActionDescriptor
        //                    .GetCustomAttributes(
        //                       typeof(RequireHttpsAttribute), false)
        //                    .Count() >= 1;

        //        //If we don't need SSL and we are not on a child action   
        //        if (!requireHttps && !filterContext.IsChildAction)
        //        {
        //            var uriBuilder = new UriBuilder(Request.Url)
        //            {
        //                Scheme = "http",
        //                Port = 80
        //            };
        //            filterContext.Result =
        //                 this.Redirect(uriBuilder.Uri.AbsoluteUri);
        //        }
        //    }
        //    base.OnAuthorization(filterContext);
        //}
    }

    //public class BaseSecureController : BaseController
    //{
    //    protected override void OnAuthorization(AuthorizationContext filterContext)
    //    {
    //        if (bool.Parse(ConfigurationManager.AppSettings["SecureEnabled"]) && !Request.IsSecureConnection)
    //        {
    //            var secureUrl = ConfigurationManager.AppSettings["SecureUrl"];
    //            if (string.IsNullOrEmpty(secureUrl))
    //                throw new ConfigurationErrorsException("SecureUrl appSetting has not been set in the configuration file.");

    //            secureUrl = String.Concat(secureUrl, Request.Url.PathAndQuery);
    //            filterContext.Result =
    //                 this.Redirect(secureUrl);
    //        }
    //        base.OnAuthorization(filterContext);
    //    }
    //}
}
