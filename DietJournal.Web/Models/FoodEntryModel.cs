using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DietJournal.Web.Models
{
    public class FoodEntryModel : BaseEntryModel
    {
        [Required(ErrorMessage="Required!")]
        [Display(Name = "Title (required):")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Display(Name = "Description (optional):")]
        public string Description { get; set; }

        [Required(AllowEmptyStrings=true, ErrorMessage="Required!")]
        public string FoodEntryType { get; set; }

        [Display(Name = "Calories (optional):")]
        public decimal Calories { get; set; }

        [Display(Name = "Carbs (optional):")]
        public decimal Carbs { get; set; }

        [Display(Name = "Fat (optional):")]
        public decimal Fat { get; set; }

        [Display(Name = "Protein (optional):")]
        public decimal Protein { get; set; }

        public IList<FoodEntryValueModel> EntryValues { get; set; }

        public IEnumerable<SelectListItem> AvailableFoodEntryTypes { get; set; }

        public bool CaptureCalories { get; set; }
        public bool CaptureFat { get; set; }
        public bool CaptureCarbs { get; set; }
        public bool CaptureProtein { get; set; }
    }

    public class FoodCollectionModel : Dictionary<string, IDictionary<int,string>>
    {
        public DateTime EntriesDate { get; set; }
    }

    public class FoodEntryTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<FoodEntryTypeValueModel> Values { get; set; }
    }

    public class FoodEntryTypeValueModel
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string InputType { get; set; }
        public IEnumerable<SelectListItem> Options { get; set; }
        public FoodEntryValueModel Value { get; set; }
    }

    public class FoodEntryValueModel
    {
        public string Id { get; set; }
        public string EntryTypeValueId { get; set; }
        public string Value { get; set; }
    }

    public class FoodFavoriteSelectionModel
    {
        public string Selection { get; set; }
        public DateTime ConsumedDate { get; set; }
        public IEnumerable<SelectListItem> AvailableFoodEntryTypes { get; set; }
    }

    public class FoodFavoriteResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}