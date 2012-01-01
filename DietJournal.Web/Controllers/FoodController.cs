using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DietJournal.Web.Models;

namespace DietJournal.Web.Controllers
{
    public class FoodController : BaseController
    {
        [Authorize]
        public ActionResult Index(DateTime date)
        {
            ViewBag.Date = date;
            ViewBag.Back = true;

            var model = new FoodCollectionModel { EntriesDate = date };

            date = date.Date;
            var tomorrow = date.AddDays(1);

            if (CurrentUserId.HasValue)
            {
                using (var entities = new DietJournalEntities())
                {
                    var entries = entities.FoodEntries.Where(e => e.UserId == CurrentUserId.Value
                        && e.EntryDate >= date && e.EntryDate < tomorrow);

                    if (entries != null)
                    {
                        var mealEntries = from e in entries.Cast<FoodEntry>()
                                          group e by e.MealType into g
                                          select g;

                        foreach (var mealEntry in mealEntries)
                        {
                            var mealType = entities.FoodEntryTypes.FirstOrDefault(t => t.Id == mealEntry.Key);
                            var values = mealEntry.ToDictionary(e => e.Id, e => e.Title);
                            model.Add(mealType.Name, values);
                        }
                    }
                }

            }

            return View(model);
        }

        #region Entry

        [Authorize]
        public ActionResult Add(DateTime date)
        {
            var model = CreateEntryModel(date);
            return EntryView(model);
        }

        [Authorize]
        public ActionResult Entry(int id)
        {
            FoodEntryModel model = null;

            using (var entities = new DietJournalEntities())
            {
                var result = entities.FoodEntries.FirstOrDefault(e => e.Id == id);

                if (result != null && result.UserId == CurrentUserId.Value)
                    model = CreateEntryModel(result);
            }

            return EntryView(model);
        }

        [HttpPost]
        public JsonResult Entry(FoodEntryModel model)
        {
            if (ModelState.IsValid)
            {
                using (var entities = new DietJournalEntities())
                {
                    FoodEntry foodEntry = null;

                    if (model.Id > 0)
                        foodEntry = entities.FoodEntries.FirstOrDefault(e => e.Id == model.Id);

                    if (foodEntry == null)
                    {
                        foodEntry = new FoodEntry();
                        entities.FoodEntries.AddObject(foodEntry);
                    }

                    foodEntry.UserId = CurrentUserId.Value;
                    foodEntry.Title = model.Title;
                    foodEntry.Description = model.Description;
                    foodEntry.Calories = model.Calories;
                    foodEntry.Carbs = model.Carbs;
                    foodEntry.EntryDate = model.ConsumedDate;
                    foodEntry.Fat = model.Fat;
                    foodEntry.Protein = model.Protein;
                    foodEntry.SavedDate = DateTime.Now;
                    foodEntry.MealType = !String.IsNullOrEmpty(model.FoodEntryType) ? int.Parse(model.FoodEntryType) : 0;

                    if (model.Favorite)
                    {
                        var favorite = new FoodFavorite
                        {
                            UserId = CurrentUserId,
                            MealType = foodEntry.MealType,
                            Title = foodEntry.Title,
                            Description = foodEntry.Description,
                            Calories = foodEntry.Calories,
                            Carbs = foodEntry.Carbs,
                            Protein = foodEntry.Protein,
                            Fat = foodEntry.Fat
                        };

                        entities.FoodFavorites.AddObject(favorite);
                    }

                    entities.SaveChanges();

                    if (model.EntryValues != null && model.EntryValues.Count > 0)
                    {
                        foreach (var entryValue in model.EntryValues)
                        {
                            FoodEntryMealTypeValue value = null;

                            if (!String.IsNullOrEmpty(entryValue.Id))
                            {
                                var entryValueId = int.Parse(entryValue.Id);
                                value = entities.FoodEntryMealTypeValues.FirstOrDefault(v => v.Id == entryValueId);
                            }

                            if (value == null)
                            {
                                value = new FoodEntryMealTypeValue
                                {
                                    MealTypeValueId = int.Parse(entryValue.EntryTypeValueId),
                                    FoodEntryId = foodEntry.Id
                                };
                                entities.FoodEntryMealTypeValues.AddObject(value);
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

        public String EntryType(int foodEntryTypeId, int foodEntryId)
        {
            var model = new List<FoodEntryTypeValueModel>();

            using (var context = new DietJournalEntities())
            {
                foreach (var foodEntryTypeValue in context.FoodEntryTypeValues.Where(v => v.FoodEntryTypeId == foodEntryTypeId))
                {
                    var valueModel = new FoodEntryTypeValueModel
                    {
                        Id = foodEntryTypeValue.Id,
                        Label = foodEntryTypeValue.Label,
                        InputType = foodEntryTypeValue.ValueType,
                        IsRequired = foodEntryTypeValue.IsRequired
                    };

                    if (foodEntryId > 0)
                    {
                        var foodEntryMealTypeValue = context.FoodEntryMealTypeValues.FirstOrDefault(v => v.FoodEntryId == foodEntryId && v.MealTypeValueId == foodEntryTypeValue.Id);
                        if (foodEntryMealTypeValue != null)
                        {
                            valueModel.Value = new FoodEntryValueModel
                            {
                                Id = foodEntryMealTypeValue.Id.ToString(),
                                EntryTypeValueId = foodEntryMealTypeValue.MealTypeValueId.ToString(),
                                Value = foodEntryMealTypeValue.Value
                            };
                        }
                    }

                    var options = context.FoodEntryTypeValueOptions.Where(o => o.FoodEntryTypeValueId == foodEntryTypeValue.Id);
                    if (options != null && options.Count() > 0)
                    {
                        valueModel.Options = (from o in options.ToList()
                                              select new SelectListItem
                                              {
                                                  Value = o.Id.ToString(),
                                                  Text = o.Value
                                              });
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
                var entry = context.FoodEntries.FirstOrDefault(e => e.Id == id);
                if (entry != null && entry.UserId == CurrentUserId.Value)
                    context.FoodEntries.DeleteObject(entry);

                context.SaveChanges();
            }
        }

        #endregion

        #region Favorites

        [Authorize]
        public ActionResult Favorites()
        {
            ViewBag.Back = true;
            ViewBag.CurrentTab = "Favorites";

            Dictionary<string, IEnumerable<FoodFavoriteResultModel>> model = null;

            using (var context = new DietJournalEntities())
            {
                model = (from f in context.FoodFavorites
                         where f.UserId == CurrentUserId
                         group f by f.MealType into g
                         select new
                         {
                             FoodType = context.FoodEntryTypes.FirstOrDefault(t => t.Id == g.Key),
                             Entries = g.Select(e => new FoodFavoriteResultModel { Id = e.Id, Title = e.Title })
                         }).ToDictionary(i => i.FoodType.Name, i => i.Entries);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult SelectFavorite(DateTime date)
        {
            var model = new FoodFavoriteSelectionModel
                           {
                               ConsumedDate = date,
                               AvailableFoodEntryTypes = GetAvailableFoodEntryTypes(CurrentProfileSettings)
                           };
            return View(model);
        }


        [HttpPost]
        public ActionResult SelectFavorite(FoodFavoriteSelectionModel model)
        {
            var entryModel = new FoodEntryModel
            {
                ConsumedDate = model.ConsumedDate
            };

            if (ModelState.IsValid && !String.IsNullOrEmpty(model.Selection))
            {
                int favoriteId = int.Parse(model.Selection);

                using (var context = new DietJournalEntities())
                {
                    var favorite = context.FoodFavorites.FirstOrDefault(f => f.Id == favoriteId);
                    if (favorite != null)
                    {
                        entryModel.FoodEntryType = favorite.MealType.ToString();
                        entryModel.Title = favorite.Title;
                        entryModel.Description = favorite.Description;
                        entryModel.Calories = favorite.Calories.HasValue ? favorite.Calories.Value : 0;
                        entryModel.Protein = favorite.Protein.HasValue ? favorite.Protein.Value : 0;
                        entryModel.Carbs = favorite.Carbs.HasValue ? favorite.Carbs.Value : 0;
                        entryModel.Fat = favorite.Fat.HasValue ? favorite.Fat.Value : 0;
                        entryModel.Favorite = true;
                    }
                }
            }

            SetFoodEntryProfileSettings(entryModel);

            return EntryView(entryModel);
        }

        [HttpPost]
        public ActionResult FavoriteSearch(string FoodEntryType, string Title)
        {
            List<FoodFavoriteResultModel> searchResults = null;

            var mealType = !String.IsNullOrEmpty(FoodEntryType) ? int.Parse(FoodEntryType) : -1;
            using(var context = new DietJournalEntities())
            {
                searchResults = (from f in context.FoodFavorites
                                 where f.UserId == CurrentProfileSettings.UserId
                                 && (mealType < 0 || f.MealType == mealType)
                                 && (String.IsNullOrEmpty(Title) || f.Title.Contains(Title))
                                 select new FoodFavoriteResultModel
                                    {
                                        Id = f.Id,
                                        Title = f.Title
                                    }).ToList();
            }

            return PartialView("FavoriteSearchResults", searchResults);
        }

        [Authorize]
        public void DeleteFavorite(int id)
        {
            using (var context = new DietJournalEntities())
            {
                var foodFavorite = context.FoodFavorites.FirstOrDefault(f => f.UserId == CurrentUserId && f.Id == id);
                if (foodFavorite != null)
                    context.FoodFavorites.DeleteObject(foodFavorite);

                context.SaveChanges();
            }
        }

        #endregion

        public ActionResult About()
        {
            return View();
        }

        #region Private/Protected Methods

        protected ActionResult EntryView(FoodEntryModel model)
        {
            ViewBag.Back = model.Id > 0;

            return View("Entry", model);
        }

        private FoodEntryModel CreateEntryModel(FoodEntry foodEntry)
        {
            var profileSettings = CurrentProfileSettings;

            var model = new FoodEntryModel
            {
                Id = foodEntry.Id,
                Title = foodEntry.Title,
                Description = foodEntry.Description,
                ConsumedDate = foodEntry.EntryDate,
                FoodEntryType = foodEntry.MealType.ToString(),
                Calories = foodEntry.Calories.HasValue ? foodEntry.Calories.Value : 0,
                Carbs = foodEntry.Carbs.HasValue ? foodEntry.Carbs.Value : 0,
                Fat = foodEntry.Fat.HasValue ? foodEntry.Fat.Value : 0,
                Protein = foodEntry.Protein.HasValue ? foodEntry.Protein.Value : 0
            };

            SetFoodEntryProfileSettings(model);

            return model;
        }

        private FoodEntryModel CreateEntryModel(DateTime consumedDate)
        {
            var model = new FoodEntryModel
            {
                ConsumedDate = consumedDate,
            };

            SetFoodEntryProfileSettings(model);

            return model;
        }

        private void SetFoodEntryProfileSettings(FoodEntryModel model)
        {
            var profileSettings = CurrentProfileSettings;

            model.CaptureCalories = profileSettings.CaptureCalories;
            model.CaptureCarbs = profileSettings.CaptureCarbs;
            model.CaptureFat = profileSettings.CaptureFat;
            model.CaptureProtein = profileSettings.CaptureProtein;
            model.AvailableFoodEntryTypes = GetAvailableFoodEntryTypes(profileSettings);
        }

        private IEnumerable<SelectListItem> GetAvailableFoodEntryTypes(ProfileSetting profileSettings)
        {
            if (profileSettings.DietPlanId.HasValue)
            {
                using (var context = new DietJournalEntities())
                {
                    var dietPlan = context.DietPlans.FirstOrDefault(d => d.Id == profileSettings.DietPlanId.Value);

                    var foodEntryTypes = (from t in context.FoodEntryTypes
                                          where t.DietPlanId == profileSettings.DietPlanId.Value
                                                ||
                                                (dietPlan.ParentId.HasValue && t.DietPlanId == dietPlan.ParentId.Value)
                                          select t).ToList();

                    var availableFoodEntryTypes = foodEntryTypes.Select(t => new SelectListItem
                                                                                 {
                                                                                     Value = t.Id.ToString(),
                                                                                     Text = t.Name
                                                                                 }).ToList();

                    availableFoodEntryTypes.Insert(0, new SelectListItem {Text = "--Select Entry Type--", Value = ""});

                    return availableFoodEntryTypes;
                }
            }
            
            return null;
        }

        #endregion
    }
}
