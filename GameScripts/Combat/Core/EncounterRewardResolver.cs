using UnityEngine;

namespace Mewtations.Combat.Core
{
    public enum RewardDeliveryType
    {
        DirectSpawn,
        RewardPopup,
        PackDrop
    }

    public static class EncounterRewardResolver
    {
        /// <summary>
        /// Resolves and delivers rewards based on the EncounterType.
        /// </summary>
        public static void ResolveRewards(Encounters.EncounterContext context, Vector3 spawnPosition)
        {
            RewardDeliveryType deliveryType = GetDeliveryTypeForContext(context);
            
            switch (deliveryType)
            {
                case RewardDeliveryType.DirectSpawn:
                    Debug.Log($"[EncounterRewardResolver] Directly spawning rewards at {spawnPosition} for {context}");
                    
                    if (WorldManager.instance != null)
                    {
                        // Temporary: Spawn 5 low-level spirit stones (linh thạch cấp thấp)
                        for (int i = 0; i < 5; i++)
                        {
                            Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                            WorldManager.instance.CreateCard(spawnPosition + offset, "resource_crystal", true, true, true);
                        }
                    }
                    break;
                    
                case RewardDeliveryType.RewardPopup:
                    // Open the standard Reward Screen (like Expedition)
                    Debug.Log($"[EncounterRewardResolver] Opening Reward Popup for {context}");
                    break;
                    
                case RewardDeliveryType.PackDrop:
                    // Drop a card pack
                    Debug.Log($"[EncounterRewardResolver] Dropping Pack for {context}");
                    if (context == Encounters.EncounterContext.BlackAltar)
                    {
                        // Drop an elixir booster pack
                        if (WorldManager.instance != null)
                        {
                            WorldManager.instance.CreateCard(spawnPosition, "booster_elixirs", true, true, true);
                        }
                    }
                    break;
            }
        }
        
        private static RewardDeliveryType GetDeliveryTypeForContext(Encounters.EncounterContext context)
        {
            switch (context)
            {
                case Encounters.EncounterContext.Expedition:
                    return RewardDeliveryType.RewardPopup;
                case Encounters.EncounterContext.DogTax:
                    return RewardDeliveryType.DirectSpawn;
                case Encounters.EncounterContext.BlackAltar:
                    return RewardDeliveryType.PackDrop;
                default:
                    return RewardDeliveryType.DirectSpawn;
            }
        }
    }
}
