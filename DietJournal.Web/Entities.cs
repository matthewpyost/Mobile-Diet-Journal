using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace DietJournal.Web
{
    public enum EntryType
    {
        Food,
        Water,
        Exercise,
        Supplement,
        Weight,
        BloodPressure
    }

    public interface IEntry : IEntityWithKey
    {
        int Id { get; set; }
        Guid UserId { get; set; }
        DateTime EntryDate { get; set; }
        DateTime SavedDate { get; set; }
    }

    public enum MealType
    {
        Breakfast,
        MorningSnack,
        Lunch,
        AfternoonSnack,
        Dinner,
        EveningSnack
    }

    public partial class BloodPressureEntry : IEntry
    {

    }

    public partial class ExerciseEntry : IEntry
    {

    }

    public partial class FoodEntry : IEntry
    {

    }

    public partial class SupplementEntry : IEntry
    {

    }

    public partial class WaterEntry : IEntry
    {

    }

    public partial class WeightEntry : IEntry
    {

    }
}