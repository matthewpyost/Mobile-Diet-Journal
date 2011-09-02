using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Objects;

namespace DietJournal.Data
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class JournalService : IJournalService
    {

        #region IJournalService Members

        public Journal GetDailyJournal(Guid userId, DateTime date)
        {
            var journal = new Journal
            {
                UserId = userId,
                Date = date,
            };

            date = date.Date;

            using (var model = new DietjournalEntities())
            {
                journal.ExerciseEnteries = model.ExerciseEntries.Where(e => e.UserId == userId && e.EntryDate >= date && e.EntryDate < date.AddDays(1)).ToList();
                journal.FoodEntries = model.FoodEntries.Where(e => e.UserId == userId && e.EntryDate >= date && e.EntryDate < date.AddDays(1)).ToList();
                journal.SupplementEntries = model.SupplementEntries.Where(e => e.UserId == userId && e.EntryDate >= date && e.EntryDate < date.AddDays(1)).ToList();
                journal.WaterEntries = model.WaterEntries.Where(e => e.UserId == userId && e.EntryDate >= date && e.EntryDate < date.AddDays(1)).ToList();
            }

            return journal;
        }

        public void DeleteJournalEntry(Guid userId, EntryType entryType, int entryId)
        {
            throw new NotImplementedException();
        }

        public void SaveJournalEntry(IEntry entry)
        {
            using (var entities = new DietjournalEntities())
            {
                if (entry is FoodEntry)
                {
                    var foodEntry = (FoodEntry)entry;

                    if (entry.Id > 0)
                        entities.Attach(foodEntry);
                    else
                    {
                        entities.FoodEntries.AddObject(foodEntry);
                    }
                }

                entities.SaveChanges();
            }
        }

        public IEnumerable<IEntry> GetJournalEntries(Guid userId, EntryType entryType, DateTime date)
        {
            var entries = new List<IEntry>();

            date = date.Date;

            using (var entities = new DietjournalEntities())
            {
                var results = from e in GetEntryTypeEntries(entities, entryType)
                              where e.UserId == userId && e.EntryDate >= date && e.EntryDate < date.AddDays(1)
                              select e;

                entries = results.ToList();
            }

            return entries;
        }

        public IEntry GetJournalEntry(int entryId, EntryType entryType)
        {
            IEntry entry = null;

            using (var entities = new DietjournalEntities())
            {
                var entryCollection = GetEntryTypeEntries(entities, entryType);
                entry = entryCollection.FirstOrDefault(e => e.Id == entryId);
            }

            return entry;
        }

        #endregion

        private Type ConvertEntryTypeToType(EntryType entryType)
        {
            switch (entryType)
            {
                case EntryType.BloodPressure:
                    return typeof(BloodPressureEntry);
                case EntryType.Exercise:
                    return typeof(ExerciseEntry);
                case EntryType.Food:
                    return typeof(FoodEntry);
                case EntryType.Supplement:
                    return typeof(SupplementEntry);
                case EntryType.Water:
                    return typeof(WaterEntry);
                case EntryType.Weight:
                    return typeof(WeightEntry);
                default:
                    throw new Exception(String.Format("Entry type '{0}' cannot be converted to a system type.", entryType));
            }
        }

        private IEnumerable<IEntry> GetEntryTypeEntries(DietjournalEntities entities, EntryType entryType)
        {
            switch(entryType)
            {
                case EntryType.BloodPressure:
                    return entities.BloodPressureEntries;
                case EntryType.Exercise:
                    return entities.ExerciseEntries;
                case EntryType.Food:
                    return entities.FoodEntries;
                case EntryType.Supplement:
                    return entities.SupplementEntries;
                case EntryType.Water:
                    return entities.WaterEntries;
                case EntryType.Weight:
                    return entities.WeightEntries;
                default:
                    return null;
            }
        }
    }
}
