using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DietJournal.Web.Models
{
    public class FeedbackModel
    {
        [Required]
        [Display(Name = "Comments:")]

        public string Comments { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Adress (optional):")]
        public string EmailAddress { get; set; }
    }
}