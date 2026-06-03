using System.Collections.Generic;
using UnityEngine;

public class GodCatPackCard : RewardPackCard
{
    public override void OnInitialCreate()
    {
        if (SaveData.Packs.Count == 0)
        {
            var instance = new RewardPackInstance
            {
                PackId = "godcat_pack",
                GeneratedCards = new List<string>(),
                OpenedCount = 0
            };
            
            // 100% Linh thạch cấp thấp
            instance.GeneratedCards.Add("item_low_spirit_stone");

            // 15% random weapon (dùng sword hoặc spear tạm thời)
            if (UnityEngine.Random.value <= 0.15f)
            {
                string[] weapons = { "sword", "spear", "magic_wand", "bow" };
                instance.GeneratedCards.Add(weapons[UnityEngine.Random.Range(0, weapons.Length)]);
            }

            SaveData.Packs.Add(instance);
        }
    }
}
