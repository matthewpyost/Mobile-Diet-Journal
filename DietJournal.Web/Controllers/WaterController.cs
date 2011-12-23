using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class WaterController : BaseController
    {
        [Authorize]
        public ActionResult Index(DateTime date)
        {
            ViewBag.Date = date;
            ViewBag.Back = true;

            var model = new WaterCollectionModel { EntriesDate = date };

            date = date.Date;
            var tomorrow = date.AddDays(1);

            if (CurrentUserId.HasValue)
            {
                using (var entities = new DietJournalEntities())
                {
                    var entries = entities.WaterEntries.Where(e => e.UserId == CurrentUserId.Value
                        && e.EntryDate >= date && e.EntryDate < tomorrow);

                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            model.Add(entry.Id, entry.Ounces);
                        }
                    }
                }

            }

            return View("Index", model);
        }

        [Authorize]
        public ActionResult Add(DateTime date)
        {
            var model = new WaterEntryModel { ConsumedDate = date };
            return View("Entry", model);
        }

        [HttpPost]
        public JsonResult Add(WaterEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DietJournalEntities())
                {
                    var waterEntry = context.WaterEntries.CreateObject();
                    context.WaterEntries.AddObject(waterEntry);

                    waterEntry.Ounces = model.Ounces;
                    waterEntry.EntryDate = model.ConsumedDate;
                    waterEntry.SavedDate = DateTime.Now;
                    waterEntry.UserId = CurrentUserId.Value;

                    context.SaveChanges();
                }

                return Json(new { IsValid = true, ReturnUrl = Url.Action("Index", new { date = model.ConsumedDate }) });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }

        [Authorize]
        public void Delete(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var entry = context.WaterEntries.FirstOrDefault(e => e.Id == id);
                if (entry != null && entry.UserId == CurrentUserId.Value)
                {
                    context.WaterEntries.DeleteObject(entry);
                    context.SaveChanges();
                }
            }
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
