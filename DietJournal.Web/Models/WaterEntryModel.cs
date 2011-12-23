using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class WaterCollectionModel : Dictionary<int, decimal>
    {
        public DateTime EntriesDate { get; set; }
    }

    public class WaterEntryModel : BaseEntryModel
    {
        [Display(Name = "Water consumed (in ounces):")]
        [Required(ErrorMessage = "Required!")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a number greater than zero")]
        public decimal Ounces { get; set; }
    }
}