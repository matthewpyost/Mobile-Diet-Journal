using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class ExerciseController : BaseController
    {
        [Authorize]
        public ActionResult Index(DateTime date)
        {
            ViewBag.Date = date;
            ViewBag.Back = true;

            var model = new ExerciseCollectionModel { EntriesDate = date };

            date = date.Date;
            var tomorrow = date.AddDays(1);

            if (CurrentUserId.HasValue)
            {
                using (var entities = new DietJournalEntities())
                {
                    var entries = entities.ExerciseEntries.Where(e => e.UserId == CurrentUserId.Value
                        && e.EntryDate >= date && e.EntryDate < tomorrow);

                    if (entries != null)
                    {
                        var exerciseEntries = from e in entries.Cast<ExerciseEntry>()
                                          group e by e.Type into g
                                          select g;

                        foreach (var exerciseEntry in exerciseEntries)
                        {
                            var exerciseType = entities.ExerciseTypes.FirstOrDefault(t => t.Id == exerciseEntry.Key);
                            var values = exerciseEntry.ToDictionary(e => e.Id, e => e.Description);
                            model.Add(exerciseType.Name, values);
                        }
                    }
                }

            }

            return View(model);
        }

        [Authorize]
        public ActionResult Add(DateTime date)
        {
            var model = new ExerciseEntryModel 
            { 
                ConsumedDate = date,
                AvailableExerciseTypes = GetAvailableExerciseTypes()
            };
            return View("Entry", model);
        }

        public ActionResult About()
        {
            return View();
        }

        [Authorize]
        public ActionResult Entry(int id)
        {
            ExerciseEntryModel model = null;

            using (var entities = new DietJournalEntities())
            {
                var result = entities.ExerciseEntries.FirstOrDefault(e => e.Id == id);

                if (result != null && result.UserId == CurrentUserId.Value)
                {
                    model = new ExerciseEntryModel
                    {
                        Id = id,
                        Description = result.Description,
                        ConsumedDate = result.EntryDate,
                        ExerciseType = result.Type.ToString(),
                        AvailableExerciseTypes = GetAvailableExerciseTypes()
                    };
                }
            }

            return EntryView(model);
        }

        [HttpPost]
        public JsonResult Entry(ExerciseEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var entities = new DietJournalEntities())
                {
                    ExerciseEntry entry = null;

                    if (model.Id > 0)
                        entry = entities.ExerciseEntries.FirstOrDefault(e => e.Id == model.Id);

                    if (entry == null)
                    {
                        entry = new ExerciseEntry();
                        entities.ExerciseEntries.AddObject(entry);
                    }

                    entry.UserId = CurrentUserId.Value;
                    entry.Description = model.Description;
                    entry.EntryDate = model.ConsumedDate;
                    entry.SavedDate = DateTime.Now;
                    entry.Type = !String.IsNullOrEmpty(model.ExerciseType) ? int.Parse(model.ExerciseType) : 0;

                    if (model.Favorite)
                    {
                        var favoriteModel = new ExerciseFavorite
                        {
                            Type = entry.Type,
                            Description = entry.Description,
                            UserId = CurrentUserId.Value
                        };
                        entities.ExerciseFavorites.AddObject(favoriteModel);
                    }

                    entities.SaveChanges();

                    if (model.EntryValues != null && model.EntryValues.Count > 0)
                    {
                        foreach (var entryValue in model.EntryValues)
                        {
                            ExerciseEntryExerciseTypeValue value = null;

                            if (!String.IsNullOrEmpty(entryValue.Id))
                            {
                                var entryValueId = int.Parse(entryValue.Id);
                                value = entities.ExerciseEntryExerciseTypeValues.FirstOrDefault(v => v.Id == entryValueId);
                            }

                            if (value == null)
                            {
                                value = new ExerciseEntryExerciseTypeValue
                                {
                                    ExerciseTypeId = int.Parse(entryValue.EntryTypeValueId),
                                    ExerciseEntryId = entry.Id
                                };
                                entities.ExerciseEntryExerciseTypeValues.AddObject(value);
                            }

                            value.Value = entryValue.Value;
                        }

                        entities.SaveChanges();
                    }
                }

                return Json(new { IsValid = true });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }

        public String EntryType(int exerciseTypeId, int exerciseEntryId)
        {
            var model = new List<ExerciseTypeValueModel>();

            using (var context = new DietJournalEntities())
            {
                foreach (var exerciseTypeValue in context.ExerciseTypeValues.Where(v => v.ExerciseTypeId == exerciseTypeId))
                {
                    var valueModel = new ExerciseTypeValueModel
                    {
                        Id = exerciseTypeValue.Id,
                        Label = exerciseTypeValue.Label,
                        InputType = exerciseTypeValue.InputType,
                        IsRequired = exerciseTypeValue.IsRequired
                    };

                    if (exerciseEntryId > 0)
                    {
                        var exerciseEntryMealTypeValue = context.ExerciseEntryExerciseTypeValues.FirstOrDefault(v => v.ExerciseEntryId == exerciseEntryId && v.ExerciseTypeId == exerciseTypeValue.Id);
                        if (exerciseEntryMealTypeValue != null)
                        {
                            valueModel.Value = new ExerciseEntryTypeValueModel
                            {
                                Id = exerciseEntryMealTypeValue.Id.ToString(),
                                EntryTypeValueId = exerciseEntryMealTypeValue.ExerciseTypeId.ToString(),
                                Value = exerciseEntryMealTypeValue.Value
                            };
                        }
                    }

                    model.Add(valueModel);
                }
            }

            var rtnString = RenderPartialViewToString("EntryType", model);

            return rtnString;
        }

        [Authorize]
        public void Delete(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var entry = context.ExerciseEntries.FirstOrDefault(e => e.Id == id);
                if (entry != null && entry.UserId == CurrentUserId.Value)
                    context.ExerciseEntries.DeleteObject(entry);

                context.SaveChanges();
            }
        }


        #region Favorites

        [Authorize]
        public ActionResult Favorites()
        {
            ViewBag.Back = true;
            ViewBag.CurrentTab = "Favorites";

            Dictionary<string, IEnumerable<ExerciseFavoriteResultModel>> model = null;

            using (var context = new DietJournalEntities())
            {
                model = (from f in context.ExerciseFavorites
                         where f.UserId == CurrentUserId
                         group f by f.Type into g
                         select new
                         {
                             ExerciseType = context.ExerciseTypes.FirstOrDefault(t => t.Id == g.Key),
                             Entries = g.Select(e => new ExerciseFavoriteResultModel { Id = e.Id, Title = e.Description })
                         }).ToDictionary(i => i.ExerciseType.Name, i => i.Entries);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult SelectFavorite(DateTime date)
        {
            var model = new ExerciseFavoriteSelectionModel
            {
                ConsumedDate = date,
                AvailableExerciseTypes = GetAvailableExerciseTypes()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SelectFavorite(ExerciseFavoriteSelectionModel model)
        {
            var entryModel = new ExerciseEntryModel
            {
                ConsumedDate = model.ConsumedDate
            };

            if (ModelState.IsValid && !String.IsNullOrEmpty(model.Selection))
            {
                int favoriteId = int.Parse(model.Selection);

                using (var context = new DietJournalEntities())
                {
                    var favorite = context.ExerciseFavorites.FirstOrDefault(f => f.Id == favoriteId);
                    if (favorite != null)
                    {
                        entryModel.ExerciseType = favorite.Type.ToString();
                        entryModel.Description = favorite.Description;
                        entryModel.Favorite = true;
                    }
                }
            }

            entryModel.AvailableExerciseTypes = GetAvailableExerciseTypes();

            return EntryView(entryModel);
        }


        [HttpPost]
        public ActionResult FavoriteSearch(string ExerciseType, string Description)
        {
            List<ExerciseFavoriteResultModel> searchResults = null;

            var exerciseType = !String.IsNullOrEmpty(ExerciseType) ? int.Parse(ExerciseType) : 0;
            using (var context = new DietJournalEntities())
            {
                searchResults = (from f in context.ExerciseFavorites
                                 where f.UserId == CurrentUserId.Value
                                 && (exerciseType == 0 || f.Type == exerciseType)
                                 && (String.IsNullOrEmpty(Description) || f.Description.Contains(Description))
                                 select new ExerciseFavoriteResultModel
                                 {
                                     Id = f.Id,
                                     Title = f.Description
                                 }).ToList();
            }

            return PartialView("FavoriteSearchResults", searchResults);
        }

        [Authorize]
        public void DeleteFavorite(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var favorite = context.ExerciseFavorites.FirstOrDefault(f => f.UserId == CurrentUserId && f.Id == id);
                if (favorite != null)
                    context.ExerciseFavorites.DeleteObject(favorite);

                context.SaveChanges();
            }
        }

        #endregion

        private IEnumerable<SelectListItem> GetAvailableExerciseTypes()
        {
            var availableExerciseTypes = new List<SelectListItem>();
            availableExerciseTypes.Add(new SelectListItem
            {
                Value = "",
                Text = "--Select Exercise Type--"
            });
            using (var entities = new DietJournalEntities())
            {
                foreach (var exerciseType in entities.ExerciseTypes.Where(t => t.IsActive))
                {
                    availableExerciseTypes.Add(new SelectListItem
                    {
                        Value = exerciseType.Id.ToString(),
                        Text = exerciseType.Name
                    });
                }
            }

            return availableExerciseTypes;
        }

        private ActionResult EntryView(ExerciseEntryModel model)
        {
            ViewBag.Back = model.Id > 0;

            return View("Entry", model);
        }
    }
}
