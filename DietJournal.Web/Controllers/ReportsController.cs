using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;
using System.Data.Objects;

namespace DietJournal.Web.Controllers
{
    public class ReportsController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.CurrentTab = "Reports";
            return View();
        }

        public ActionResult Entries()
        {
            ViewBag.Back = true;
            ViewBag.CurrentTab = "Reports";

            var model = new EntriesSummaryReportModel
            {
                AvailableEntryTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Food", Value = "Food" },
                    new SelectListItem { Text = "Exercise", Value = "Exercise" },
                    new SelectListItem { Text = "Water", Value = "Water" },
                    new SelectListItem { Text = "Supplements", Value = "Supplement" }
                }
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Entries(EntriesSummaryReportModel model)
        {
            var results = new List<EntriesSummaryResults>();

            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.EndDate);
            var query = new Func<IEntry, bool>(e => e.UserId == CurrentUserId.Value && e.EntryDate >= startDate && endDate >= e.EntryDate);

            using (var context = new DietJournalEntities())
            {
                foreach (var entryType in model.EntryTypes)
                {
                    IEnumerable<IEntry> queryResults = null;
                    
                    switch (entryType)
                    {
                        case "Food":
                            queryResults = context.FoodEntries.Where(query);
                            CreateEntryResults<FoodEntry>(entryType, results, queryResults, delegate(FoodEntry foodEntry)
                            {
                                var category = context.FoodEntryTypes.FirstOrDefault(foodEntryType => foodEntryType.Id == foodEntry.MealType);
                                return new EntriesSummaryResult
                                {
                                    Category = category != null ? category.Name : string.Empty,
                                    Title = foodEntry.Title,
                                    Description = foodEntry.Description
                                };
                                return new EntriesSummaryResult();
                            });
                            break;
                        case "Exercise":
                            queryResults = context.ExerciseEntries.Where(query);
                            CreateEntryResults<ExerciseEntry>(entryType, results, queryResults, exerciseEntry => new EntriesSummaryResult
                            {
                                Category = context.ExerciseTypes.First(exerciseType => exerciseType.Id == exerciseEntry.Type).Name,
                                Title = exerciseEntry.Description
                            });
                            break;
                        case "Water":
                            queryResults = context.WaterEntries.Where(query);
                            CreateEntryResults<WaterEntry>(entryType, results, queryResults, waterEntry => new EntriesSummaryResult
                            {
                                Title = String.Format("{0} oz", waterEntry.Ounces)
                            });
                            break;
                        case "Supplements":
                            queryResults = context.SupplementEntries.Where(query);
                            CreateEntryResults<SupplementEntry>(entryType, results, queryResults, suppEntry => new EntriesSummaryResult
                            {
                                Title = suppEntry.Name,
                                Description = String.Format("{0} milligrams", suppEntry.Milligrams)
                            });
                            break;
                    }
                }
            }

            results = results.OrderBy(r => r.EntryDate).ToList();

            if (!String.IsNullOrEmpty(model.Recipient))
            {
                var messageModel = new EntriesShareReportModel
                {
                    To = model.Recipient,
                    Subject = "Fast Diet Journal Entries",
                    Message = model.Message,
                    Results = results
                };
                ShareReportResults<List<EntriesSummaryResults>>(messageModel, "EntriesEmail");
            }

            return PartialView("EntryResults", results);
        }

        private void CreateEntryResults<T>(string entryType, List<EntriesSummaryResults> results, IEnumerable<IEntry> queryResults, Func<T,EntriesSummaryResult> createDelegate) where T : IEntry
        {
            if (queryResults.Count() == 0)
                return;

            var queryResultsByDate = from r in queryResults
                                     group r by r.EntryDate.Date into g
                                     select new
                                     {
                                         EntryDate = g.Key,
                                         Entries = g.Select(entry => createDelegate((T)entry)).ToList()
                                     };
            foreach (var result in queryResultsByDate)
            {
                var entrySummaryResults = results.FirstOrDefault(e => e.EntryDate == result.EntryDate);
                if (entrySummaryResults == null)
                {
                    entrySummaryResults = new EntriesSummaryResults { EntryDate = result.EntryDate };
                    results.Add(entrySummaryResults);
                }

                entrySummaryResults.Add(entryType, result.Entries);
            }
        }

        private void ShareReportResults<T>(BaseShareReportModel<T> model, string templateName)
        {
            model.Message = model.Message.Replace("\r\n", "<br/>");
            var messageBody = RenderPartialViewToString(templateName, model);

            using (var smtp = new System.Net.Mail.SmtpClient())
            {
                var message = new System.Net.Mail.MailMessage("support@fastdietjournal.com", model.To);
                message.Subject = model.Subject;
                message.Body = messageBody;
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
        }
    }
}