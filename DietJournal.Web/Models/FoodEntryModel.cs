using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DietJournal.Web.Models
{
    public class FoodEntryModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Meal Type")]
        public int Meal { get; set; }

        [Required]
        [Display(Name = "Title (required):")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Display(Name = "Description (optional):")]
        public string Description { get; set; }

        public DateTime ConsumedDate { get; set; }

        [Display(Name = "Calories (optional):")]
        public decimal Calories { get; set; }

        [Display(Name = "Carbs (optional):")]
        public decimal Carbs { get; set; }

        [Display(Name = "Fat (optional):")]
        public decimal Fat { get; set; }

        [Display(Name = "Protein (optional):")]
        public decimal Protein { get; set; }

        public IEnumerable<SelectListItem> MealTypes { get; set; }
    }

    //public class MealModel : List<FoodEntryModel>
    //{
    //    public int Id { get; set; }
    //    public string Text { get; set; }
    //}

    public class FoodEntriesModel : Dictionary<string, IEnumerable<FoodEntryModel>>//List<MealModel>
    {
        public DateTime EntriesDate { get; set; }
    }
}