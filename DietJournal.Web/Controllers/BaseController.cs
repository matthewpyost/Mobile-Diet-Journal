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
    }
}
