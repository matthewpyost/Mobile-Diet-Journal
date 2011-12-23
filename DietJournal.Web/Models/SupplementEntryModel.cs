using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class SupplementEntryModel : BaseEntryModel
    {
        [Display(Name = "Name:")]
        [Required(ErrorMessage = "Required!")]
        public string Name { get; set; }

        [Display(Name = "Dosage (mg):")]
        public string Milligrams { get; set; }
    }

    public class SupplementsCollectionModel : Dictionary<int,string>
    {
        public DateTime EntriesDate { get; set; }
    }

}