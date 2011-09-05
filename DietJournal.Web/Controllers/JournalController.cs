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
    //[Authorize]
    public class JournalController : BaseController
    {
        public ActionResult Index(DateTime? date)
        {
            return View(date != null ? date : DateTime.Today);
        }

        public ActionResult Food(DateTime date)
        {
            ViewBag.Date = date;
            ViewBag.Back = true;

            var model = new FoodEntriesModel { EntriesDate = date };

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
                                          group e by e.Meal into g
                                          select g;

                        foreach (var mealEntry in mealEntries)
                        {
                            var meal = GetMealTypeDisplayText((MealType)mealEntry.Key);
                            var values = mealEntry.Select(m => ConvertToModel(m));
                            model.Add(meal, values);
                        }
                    }
                }

            }

            return View(model);
        }

        public ActionResult AddFoodEntry(DateTime date)
        {
            return FoodEntryView(new FoodEntryModel { ConsumedDate = date });
        }

        public ActionResult FoodEntry(int id)
        {
            FoodEntryModel model = null;

            using (var entities = new DietJournalEntities())
            {
                var result = entities.FoodEntries.FirstOrDefault(e => e.Id == id);
                
                if (result != null)
                    model = ConvertToModel(result);
            }

            return FoodEntryView(model);
        }

        [HttpPost]
        public JsonResult FoodEntry(FoodEntryModel model)
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
                    foodEntry.Meal = model.Meal;
                    foodEntry.Protein = model.Protein;
                    foodEntry.SavedDate = DateTime.Now;

                    entities.SaveChanges();
                }

                return Json(new { IsValid = true, ReturnUrl = Url.Action("Food", new { date = model.ConsumedDate }) });
            }

            return Json(new { IsValid = false, ErrorMessage = "" });
        }

        protected ActionResult FoodEntryView(FoodEntryModel model)
        {
            ViewBag.Back = model.Id > 0;

            model.MealTypes = from m in Enum.GetValues(typeof(MealType)).Cast<MealType>()
                              select new SelectListItem
                              {
                                  Value = ((int)m).ToString(),
                                  Text = GetMealTypeDisplayText(m)
                              };

            return View("FoodEntry", model);
        }

        public ActionResult Water(DateTime date)
        {
            ViewBag.Back = true;
            return View();
        }

        public ActionResult Supplements(DateTime date)
        {
            ViewBag.Back = true;
            return View();
        }

        public ActionResult Exercise(DateTime date)
        {
            ViewBag.Back = true;
            return View();
        }

        public ActionResult Settings()
        {
            ViewBag.CurrentTab = "Settings";
            return View();
        }

        private string GetMealTypeDisplayText(MealType mealType)
        {
            var text = new StringBuilder();
            foreach(char letter in mealType.ToString())
            {
                if (char.IsUpper(letter))
                    text.Append(" ");
                text.Append(letter);
            }

            return text.ToString().Trim();
        }

        private FoodEntryModel ConvertToModel(FoodEntry foodEntry)
        {
            if (foodEntry == null)
                return null;

            return new FoodEntryModel
            {
                Id = foodEntry.Id,
                Title = foodEntry.Title,
                Description = foodEntry.Description,
                Meal = foodEntry.Meal,
                Calories = foodEntry.Calories.HasValue ? foodEntry.Calories.Value : 0,
                Carbs = foodEntry.Carbs.HasValue ? foodEntry.Carbs.Value : 0,
                Fat = foodEntry.Fat.HasValue ? foodEntry.Fat.Value : 0,
                Protein = foodEntry.Protein.HasValue ? foodEntry.Protein.Value : 0,
                ConsumedDate = foodEntry.EntryDate
            };
        }
    }
}
