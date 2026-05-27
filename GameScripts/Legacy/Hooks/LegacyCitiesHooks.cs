using System;

namespace Mewtations.Core
{
    public static class LegacyCitiesHooks
    {
        public static int Wellbeing
        {
            get => (!LegacyRuntimeFlags.EnableCitiesSystem || CitiesManager.instance == null) ? 0 : CitiesManager.instance.Wellbeing;
            set { if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.Wellbeing = value; }
        }

        public static int WellbeingStart
        {
            get => (!LegacyRuntimeFlags.EnableCitiesSystem || CitiesManager.instance == null) ? 0 : CitiesManager.instance.WellbeingStart;
        }

        public static void AddWellbeing(int value)
        {
            if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.AddWellbeing(value);
        }

        public static int NextConflictMonth
        {
            get => (!LegacyRuntimeFlags.EnableCitiesSystem || CitiesManager.instance == null) ? -1 : CitiesManager.instance.NextConflictMonth;
            set { if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.NextConflictMonth = value; }
        }

        public static string ActiveEvent
        {
            get => (!LegacyRuntimeFlags.EnableCitiesSystem || CitiesManager.instance == null) ? null : CitiesManager.instance.ActiveEvent;
            set { if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.ActiveEvent = value; }
        }

        public static void StopDrawCable(GameCard card)
        {
            if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.StopDrawCable(card);
        }

        public static void CheckCityHealth()
        {
            if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.CheckCityHealth();
        }
        
        public static void ResetWellbeingToStart()
        {
            if (LegacyRuntimeFlags.EnableCitiesSystem && CitiesManager.instance != null) CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
        }
    }
}
