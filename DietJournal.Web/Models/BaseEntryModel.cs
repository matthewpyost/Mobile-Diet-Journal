using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class BaseEntryModel
    {
        public int Id { get; set; }

        public DateTime ConsumedDate { get; set; }

        [Display(Name = "Save as Favorite")]
        public bool Favorite { get; set; }
    }
}