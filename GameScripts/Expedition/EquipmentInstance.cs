using System;
using System.Collections.Generic;

namespace Mewtations.Expedition
{
    [Serializable]
    public class EquipmentInstance
    {
        public string EquipmentId;
        public int UpgradeLevel;
        public string InstanceId;
        public Dictionary<string, float> RuntimeModifiers;
        public CombatStats CachedBaseStats;
        public int Version = 1;
        
        public EquipmentInstance(string equipmentId, int level = 0)
        {
            EquipmentId = equipmentId;
            UpgradeLevel = level;
            InstanceId = Guid.NewGuid().ToString();
            RuntimeModifiers = new Dictionary<string, float>();
        }

        public CardData GetBaseCardData()
        {
            if (WorldManager.instance == null || WorldManager.instance.GameDataLoader == null)
                return null;
                
            return WorldManager.instance.GameDataLoader.GetCardFromId(EquipmentId);
        }
    }
}
