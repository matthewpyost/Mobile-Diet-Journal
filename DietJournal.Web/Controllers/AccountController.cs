using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class AccountController : BaseController
    {

        private const int SelectDietPlanValue = 0;
        private const int NoDietPlanValue = 9999;

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            var model = new RegisterModel
            {
                AvailableDietPlans = GetDietPlanSelectItems()
            };

            return View(model);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                var membershipUser = Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false /* createPersistentCookie */);

                    using (var entities = new DietJournalEntities())
                    {
                        var settings = entities.ProfileSettings.CreateObject();
                        settings.UserId = (Guid)membershipUser.ProviderUserKey;
                        if (model.DietPlanId != NoDietPlanValue && model.DietPlanId != SelectDietPlanValue)
                            settings.DietPlanId = model.DietPlanId;
                        entities.ProfileSettings.AddObject(settings);

                        entities.SaveChanges();
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            model.AvailableDietPlans = GetDietPlanSelectItems();

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var errorMessage = string.Empty;
            var isValid = false;

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    isValid = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    isValid = false;
                }

                if (!isValid)
                {
                    errorMessage = "The current password is incorrect or the new password is invalid.";
                }
            }

            return Json(new { IsValid = isValid, ErrorMessage = errorMessage });
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            var errorMessage = string.Empty;
            bool isValid = true;

            if (ModelState.IsValid)
            {
                var user = Membership.GetUser(model.Email, false);
                if (user == null)
                {
                    errorMessage = "Unable to find a matching account for the specified email address.";
                    isValid = false;
                }
                else
                {
                    if (user.IsLockedOut)
                        user.UnlockUser();

                    var password = user.ResetPassword();

                    try
                    {
                        using (var smtp = new System.Net.Mail.SmtpClient())
                        {
                            var message = new System.Net.Mail.MailMessage("support@fastdietjournal.com", model.Email);
                            message.Subject = "Support Request";
                            message.Body = string.Format("Below you will find the new password you requested.\r\n\r\nPassword: {0}", password);
                            smtp.Send(message);
                        }
                    }
                    catch(Exception ex)
                    {
                        errorMessage = "We're sorry but there was an error sending you your new password.";
                        isValid = false;
                    }
                }
            }

            return Json(new { IsValid = isValid, ErrorMessage = errorMessage });
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public PartialViewResult ProfileSettings()
        {
            var model = new ProfileSettingsModel
            {
                AvailableDietPlans = GetDietPlanSelectItems()
            };

            var result = CurrentProfileSettings;
            if (result != null)
            {
                model.FirstName = result.FirstName;
                model.LastName = result.LastName;
                model.DietPlanId = result.DietPlanId.HasValue ? result.DietPlanId.Value : NoDietPlanValue;
                model.Birthday = result.BirthDay.HasValue ? result.BirthDay.Value.ToShortDateString() : string.Empty;
                model.WeightGoal = result.WeightGoal.HasValue ? result.WeightGoal.Value : 0;
                model.Gender = result.Gender.HasValue ? result.Gender.Value : 0;
                model.CaptureProtein = result.CaptureProtein;
                model.CaptureFat = result.CaptureFat;
                model.CaptureCarbs = result.CaptureCarbs;
                model.CaptureCalories = result.CaptureCalories;
                model.CaloriesGoal = result.CaloriesGoal.HasValue ? result.CaloriesGoal.Value : 0;
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ProfileSettings(ProfileSettingsModel model)
        {
            var membership = Membership.GetUser(HttpContext.User.Identity.Name);

            using (var context = new DietJournalEntities())
            {
                var result = context.ProfileSettings.FirstOrDefault(s => s.UserId == (Guid)membership.ProviderUserKey);
                if (result == null)
                {
                    result = context.ProfileSettings.CreateObject();
                    result.UserId = (Guid)membership.ProviderUserKey;
                    context.ProfileSettings.AddObject(result);
                }

                result.FirstName = model.FirstName.Trim();
                result.LastName = model.LastName.Trim();
                if (!String.IsNullOrEmpty(model.Birthday))
                    result.BirthDay = DateTime.Parse(model.Birthday);
                else
                    result.BirthDay = null;
                result.Gender = model.Gender;
                result.CaloriesGoal = model.CaloriesGoal;
                result.CaptureCalories = model.CaptureCalories;
                result.CaptureCarbs = model.CaptureCarbs;
                result.CaptureFat = model.CaptureFat;
                result.CaptureProtein = model.CaptureProtein;
                result.DietPlanId = model.DietPlanId;
                result.WeightGoal = model.WeightGoal;

                context.SaveChanges();

                CurrentProfileSettings = result;
            }

            return RedirectToAction("Settings", "Journal");
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        private IEnumerable<SelectListItem> GetDietPlanSelectItems()
        {
            var availablePlans = new List<SelectListItem>();
            foreach (var dietPlan in DietPlans)
            {
                if (dietPlan.DietPlans != null)
                {
                    foreach (var childPlan in dietPlan.DietPlans)
                    {
                        availablePlans.Add(new SelectListItem
                        {
                            Text = String.Format("{0}: {1}", dietPlan.Name, childPlan.Name),
                            Value = childPlan.Id.ToString()
                        });
                    }
                }
                else
                {
                    availablePlans.Add(new SelectListItem
                    {
                        Text = dietPlan.Name,
                        Value = dietPlan.Id.ToString()
                    });
                }
            }

            availablePlans.Insert(0, new GroupedSelectListItem { Value = SelectDietPlanValue.ToString(), Text = "--Select a diet plan--" });
            availablePlans.Add(new GroupedSelectListItem { Value = NoDietPlanValue.ToString(), Text = "No Diet Plan" });

            return availablePlans;
        }
    }
}
