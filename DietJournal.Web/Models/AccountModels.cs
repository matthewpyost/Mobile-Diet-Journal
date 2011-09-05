using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace DietJournal.Web.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "Email address")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Diet plan: ")]
        public int DietPlanId { get; set; }

        public IEnumerable<SelectListItem> AvailableDietPlans { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }
    }

    public class ProfileSettingsModel
    {
        [Display(Name = "Diet plan: ")]
        public int DietPlanId { get; set; }

        [Display(Name = "First name: ")]
        public string FirstName { get; set; }

        [Display(Name = "Last name: ")]
        public string LastName { get; set; }

        [Display(Name = "Birthday: ")]
        public string Birthday { get; set; }

        [Display(Name = "Gender: ")]
        public int Gender { get; set; }

        [Display(Name = "Capture calories")]
        public bool CaptureCalories { get; set; }

        [Display(Name = "Capture fat")]
        public bool CaptureFat { get; set; }

        [Display(Name = "Capture carbs")]
        public bool CaptureCarbs { get; set; }

        [Display(Name = "Capture protein")]
        public bool CaptureProtein { get; set; }

        [Display(Name = "Weight Goal:")]
        public int WeightGoal { get; set; }

        [Display(Name = "Calories Goal:")]
        public int CaloriesGoal { get; set; }

        public IEnumerable<SelectListItem> AvailableDietPlans { get; set; }
    }
}
