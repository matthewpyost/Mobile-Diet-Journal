using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DietJournal.Data
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IJournalService
    {
        [OperationContract]
        Journal GetDailyJournal(Guid userId, DateTime date);

        [OperationContract]
        void DeleteJournalEntry(Guid userId, EntryType entryType, int entryId);

        [OperationContract]
        [ServiceKnownType(typeof(FoodEntry))]
        void SaveJournalEntry(IEntry entry);

        [OperationContract]
        IEnumerable<IEntry> GetJournalEntries(Guid userId, EntryType entryType, DateTime date);

        [OperationContract]
        IEntry GetJournalEntry(int entryId, EntryType entryType);
    }

    public enum EntryType
    {
        Food,
        Water,
        Exercise,
        Supplement,
        Weight,
        BloodPressure
    }

    [DataContract]
    public class Journal
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public IEnumerable<FoodEntry> FoodEntries { get; set; }

        [DataMember]
        public IEnumerable<WaterEntry> WaterEntries { get; set; }

        [DataMember]
        public IEnumerable<ExerciseEntry> ExerciseEnteries { get; set; }

        [DataMember]
        public IEnumerable<SupplementEntry> SupplementEntries { get; set; }
    }
}
