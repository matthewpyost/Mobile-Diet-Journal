using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class ExerciseCollectionModel : Dictionary<string, IDictionary<int, string>>
    {
        public DateTime EntriesDate { get; set; }
    }

    public class ExerciseEntryModel : BaseEntryModel
    {
        [Required(ErrorMessage = "Required!")]
        public string ExerciseType { get; set; }

        [Display(Name = "Description:")]
        [Required(ErrorMessage = "Required!")]
        public string Description { get; set; }

        public IList<ExerciseEntryTypeValueModel> EntryValues { get; set; }

        public IEnumerable<SelectListItem> AvailableExerciseTypes { get; set; }
    }

    public class ExerciseTypeValueModel
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string InputType { get; set; }
        public ExerciseEntryTypeValueModel Value { get; set; }
    }

    public class ExerciseEntryTypeValueModel
    {
        public string Id { get; set; }
        public string EntryTypeValueId { get; set; }
        public string Value { get; set; }
    }

    public class ExerciseFavoriteSelectionModel
    {
        public string Selection { get; set; }
        public DateTime ConsumedDate { get; set; }
        public IEnumerable<SelectListItem> AvailableExerciseTypes { get; set; }
    }

    public class ExerciseFavoriteResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}