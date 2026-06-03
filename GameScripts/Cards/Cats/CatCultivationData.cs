using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Cards.Cats
{
    [Serializable]
    public class CatCultivationData
    {
        public int RealmStage;
        public int BreakthroughCount;
        public List<string> InsertedSpiritPills = new List<string>();

        // Serialization helper
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static CatCultivationData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new CatCultivationData();
            
            try
            {
                return JsonUtility.FromJson<CatCultivationData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse CatCultivationData json: {ex.Message}. Returning new data.");
                return new CatCultivationData();
            }
        }

        public void InsertPill(string pillId)
        {
            if (!InsertedSpiritPills.Contains(pillId))
            {
                InsertedSpiritPills.Add(pillId);
            }
        }
    }
}
