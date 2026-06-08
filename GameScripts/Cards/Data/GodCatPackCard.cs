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
            instance.GeneratedCards.Add("low_spirit_stone");

            // 15% random bonus (chỉ rớt đồ Mewtations)
            if (UnityEngine.Random.value <= 0.15f)
            {
                string[] bonusItems = { "broken_spirit_stone", "refined_spirit_stone" };
                instance.GeneratedCards.Add(bonusItems[UnityEngine.Random.Range(0, bonusItems.Length)]);
            }

            SaveData.Packs.Add(instance);
        }
    }
}
