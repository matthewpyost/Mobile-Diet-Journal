using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class SupplementsController : BaseController
    {
        [Authorize]
        public ActionResult Index(DateTime date)
        {
            ViewBag.Date = date;
            ViewBag.Back = true;

            var model = new SupplementsCollectionModel { EntriesDate = date };

            date = date.Date;
            var tomorrow = date.AddDays(1);

            if (CurrentUserId.HasValue)
            {
                using (var entities = new DietJournalEntities())
                {
                    var entries = entities.SupplementEntries.Where(e => e.UserId == CurrentUserId.Value
                        && e.EntryDate >= date && e.EntryDate < tomorrow);

                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            model.Add(entry.Id, entry.Name);
                        }
                    }
                }

            }

            return View("Index", model);
        }

        public ActionResult About()
        {
            return View();
        }

        [Authorize]
        public ActionResult Add(DateTime date)
        {
            var model = new SupplementEntryModel { ConsumedDate = date };
            return View("Entry", model);
        }

        [HttpPost]
        public JsonResult Add(SupplementEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DietJournalEntities())
                {
                    var entry = context.SupplementEntries.CreateObject();
                    context.SupplementEntries.AddObject(entry);

                    entry.Name = model.Name;
                    if (!String.IsNullOrEmpty(model.Milligrams))
                        entry.Milligrams = decimal.Parse(model.Milligrams);
                    entry.EntryDate = model.ConsumedDate;
                    entry.SavedDate = DateTime.Now;
                    entry.UserId = CurrentUserId.Value;

                    context.SaveChanges();
                }

                return Json(new { IsValid = true, ReturnUrl = Url.Action("Index", new { date = model.ConsumedDate }) });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }

        [Authorize]
        public ActionResult Entry(int id)
        {
            SupplementEntryModel model = null;

            using (var entities = new DietJournalEntities())
            {
                var result = entities.SupplementEntries.FirstOrDefault(e => e.Id == id);

                if (result != null && result.UserId == CurrentUserId.Value)
                {
                    model = new SupplementEntryModel
                    {
                        Id = id,
                        Name = result.Name,
                        ConsumedDate = result.EntryDate,
                        Milligrams = result.Milligrams > 0 ? result.Milligrams.ToString() : string.Empty,
                    };
                }
            }

            return EntryView(model);
        }

        [HttpPost]
        public JsonResult Entry(SupplementEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var entities = new DietJournalEntities())
                {
                    SupplementEntry entry = null;

                    if (model.Id > 0)
                        entry = entities.SupplementEntries.FirstOrDefault(e => e.Id == model.Id);

                    if (entry == null)
                    {
                        entry = new SupplementEntry();
                        entities.SupplementEntries.AddObject(entry);
                    }

                    entry.UserId = CurrentUserId.Value;
                    entry.Name = model.Name;
                    entry.EntryDate = model.ConsumedDate;
                    entry.SavedDate = DateTime.Now;
                    entry.Milligrams = !String.IsNullOrEmpty(model.Milligrams) ? decimal.Parse(model.Milligrams) : 0M;

                    if (model.Favorite)
                    {
                        var favorite = new SupplementFavorite
                        {
                            UserId = CurrentUserId.Value,
                            Milligrams = entry.Milligrams,
                            Name = entry.Name
                        };

                        entities.SupplementFavorites.AddObject(favorite);
                    }

                    entities.SaveChanges();
                }

                return Json(new { IsValid = true });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }

        [Authorize]
        public void Delete(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var entry = context.SupplementEntries.FirstOrDefault(e => e.Id == id);
                if (entry != null && entry.UserId == CurrentUserId.Value)
                {
                    context.SupplementEntries.DeleteObject(entry);
                    context.SaveChanges();
                }
            }
        }

        [Authorize]
        public ActionResult Favorites()
        {
            ViewBag.Back = true;
            ViewBag.CurrentTab = "Favorites";

            IEnumerable<SupplementFavoriteResultModel> model = null;

            using (var context = new DietJournalEntities())
            {
                model = (from f in context.SupplementFavorites
                         where f.UserId == CurrentUserId
                         select new SupplementFavoriteResultModel 
                         { 
                             Id = f.Id, 
                             Title = f.Name,
                             Milligrams = f.Milligrams.HasValue ? f.Milligrams.Value : 0M
                         }).ToList();
            }

            return View(model);
        }

        [Authorize]
        public ActionResult SelectFavorite(DateTime date)
        {
            var model = new SupplementFavoriteSelectionModel
            {
                ConsumedDate = date
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult FavoriteSearch(string Name)
        {
            List<SupplementFavoriteResultModel> searchResults = null;

            using (var context = new DietJournalEntities())
            {
                searchResults = (from f in context.SupplementFavorites
                                 where f.UserId == CurrentUserId.Value
                                 && (String.IsNullOrEmpty(Name) || f.Name.Contains(Name))
                                 select new SupplementFavoriteResultModel
                                 {
                                     Id = f.Id,
                                     Title = f.Name,
                                     Milligrams = f.Milligrams.HasValue ? f.Milligrams.Value : 0M,
                                 }).ToList();
            }

            return PartialView("FavoriteSearchResults", searchResults);
        }

        [HttpPost]
        public ActionResult SelectFavorite(SupplementFavoriteSelectionModel model)
        {
            var entryModel = new SupplementEntryModel
            {
                ConsumedDate = model.ConsumedDate
            };

            if (ModelState.IsValid && !String.IsNullOrEmpty(model.Selection))
            {
                int favoriteId = int.Parse(model.Selection);

                using (var context = new DietJournalEntities())
                {
                    var favorite = context.SupplementFavorites.FirstOrDefault(f => f.Id == favoriteId);
                    if (favorite != null)
                    {
                        entryModel.Milligrams = favorite.Milligrams.HasValue ? favorite.Milligrams.Value.ToString() : string.Empty;
                        entryModel.Name = favorite.Name;
                        entryModel.Favorite = true;
                    }
                }
            }

            return EntryView(entryModel);
        }

        [Authorize]
        public void DeleteFavorite(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var favorite = context.SupplementFavorites.FirstOrDefault(f => f.UserId == CurrentUserId && f.Id == id);
                if (favorite != null)
                    context.SupplementFavorites.DeleteObject(favorite);

                context.SaveChanges();
            }
        }

        private ActionResult EntryView(SupplementEntryModel model)
        {
            ViewBag.Back = model.Id > 0;

            return View("Entry", model);
        }
    }
}
