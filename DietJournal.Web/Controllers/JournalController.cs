using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DietJournal.Web.Models;
using System.Text;
using System.ServiceModel;

namespace DietJournal.Web.Controllers
{
    [Authorize]
    public class JournalController : BaseController
    {
        public ActionResult Index(DateTime? date)
        {
            ViewBag.CurrentTab = "Entries";

            var model = new JournalIndexModel
            {
                Date = date != null ? date.Value.Date : DateTime.Today
            };

            var tomorrow = model.Date.AddDays(1);

            using (var context = new DietJournalEntities())
            {
                var weight = context.WeightEntries.FirstOrDefault(e => e.UserId == CurrentUserId
                    && e.EntryDate >= model.Date && e.EntryDate < tomorrow);

                if (weight != null)
                    model.Wieght = weight.Amount;
            }

            return View(model);
        }

        public ActionResult Settings()
        {
            ViewBag.CurrentTab = "Settings";
            return View();
        }

        public ActionResult Reports()
        {
            ViewBag.CurrentTab = "Reports";
            return View();
        }

        public ActionResult Favorites()
        {
            ViewBag.CurrentTab = "Favorites";
            return View();
        }

        public ActionResult WeightEntry(DateTime date)
        {
            var model = new WeightEntryModel { ConsumedDate = date };
            return View(model);
        }

        [HttpPost]
        public JsonResult WeightEntry(WeightEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DietJournalEntities())
                {
                    WeightEntry weightEntry = null;
                    if (model.Id > 0)
                    {
                        weightEntry = context.WeightEntries.FirstOrDefault(e => e.Id == model.Id);
                    }

                    if (weightEntry == null)
                    {
                        weightEntry = context.WeightEntries.CreateObject();
                        context.WeightEntries.AddObject(weightEntry);
                    }


                    weightEntry.Amount = model.Amount;
                    weightEntry.EntryDate = model.ConsumedDate;
                    weightEntry.SavedDate = DateTime.Now;
                    weightEntry.UserId = CurrentUserId.Value;

                    context.SaveChanges();
                }

                return Json(new { IsValid = true, ReturnUrl = Url.Action("Index", new { date = model.ConsumedDate }) });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }
    }
}

