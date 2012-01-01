using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DietJournal.Web.Models
{
    public class BaseReportModel
    {
        public string Recipient { get; set; }
        public string Message { get; set; }
    }

    public abstract class BaseShareReportModel<T>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public T Results { get; set; }
    }

    public class EntriesSummaryResults : Dictionary<string, List<EntriesSummaryResult>>
    {
        public DateTime EntryDate { get; set; }
    }

    public class EntriesSummaryResult
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class EntriesSummaryReportModel : BaseReportModel
    {
        [Required(ErrorMessage = "Start date required!")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "End date required!")]
        public string EndDate { get; set; }

        public string[] EntryTypes { get; set; }

        public List<SelectListItem> AvailableEntryTypes { get; set; }
    }

    public class EntriesShareReportModel : BaseShareReportModel<List<EntriesSummaryResults>>
    {

    }
}