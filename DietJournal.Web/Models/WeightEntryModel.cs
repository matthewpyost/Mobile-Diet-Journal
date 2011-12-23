using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class WeightEntryModel : BaseEntryModel
    {
        [Display(Name = "Current weight (in lbs):")]
        [Required(ErrorMessage = "Required!")]
        public decimal Amount { get; set; }
    }
}