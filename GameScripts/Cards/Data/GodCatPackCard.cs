using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GodCatPackCard : RewardPackCard
{
    public override void OnInitialCreate()
    {
        if (this.Id == Cards.godcat_pack_low)
        {
            RegisterPack(Cards.godcat_pack_low, 80, 15, 0, 5);
        }
        else if (this.Id == Cards.godcat_pack_mid)
        {
            RegisterPack(Cards.godcat_pack_mid, 45, 45, 8, 2);
        }
        else if (this.Id == Cards.godcat_pack_high)
        {
            RegisterPack(Cards.godcat_pack_high, 25, 45, 25, 5);
        }
    }

    private void RegisterPack(string packId, int lowWeight, int midWeight, int highWeight, int bonusWeight)
    {
        // Kiểm tra tránh ghi đè pack nếu đã tồn tại
        if (!SaveData.Packs.Any(p => p.PackId == packId))
        {
            var instance = new RewardPackInstance
            {
                PackId = packId,
                GeneratedCards = new List<string>(),
                OpenedCount = 0
            };
            
            // Xây dựng danh sách weighted pool
            // Lấy 1 vật phẩm chính dựa trên trọng số
            string mainItem = GetRandomItemByWeight(lowWeight, midWeight, highWeight);
            if (!string.IsNullOrEmpty(mainItem))
            {
                instance.GeneratedCards.Add(mainItem);
            }

            // Xử lý tỉ lệ rớt bonus
            int totalBonusChance = 100;
            int randomVal = UnityEngine.Random.Range(0, totalBonusChance);
            if (randomVal < bonusWeight)
            {
                // Pool bonus khác nhau tùy theo cấp độ (có thể mở rộng sau)
                if (packId == Cards.godcat_pack_high)
                {
                    string[] rareItems = { "broken_spirit_stone", "refined_spirit_stone", "high_spirit_stone" }; // Tạm thời dùng các item này làm bonus
                    instance.GeneratedCards.Add(rareItems[UnityEngine.Random.Range(0, rareItems.Length)]);
                }
                else
                {
                    string[] bonusItems = { "broken_spirit_stone", "refined_spirit_stone" };
                    instance.GeneratedCards.Add(bonusItems[UnityEngine.Random.Range(0, bonusItems.Length)]);
                }
            }

            SaveData.Packs.Add(instance);
        }
    }

    private string GetRandomItemByWeight(int lowWeight, int midWeight, int highWeight)
    {
        int totalWeight = lowWeight + midWeight + highWeight;
        if (totalWeight <= 0) return "low_spirit_stone";

        int roll = UnityEngine.Random.Range(0, totalWeight);
        if (roll < lowWeight)
            return "low_spirit_stone";
        roll -= lowWeight;
        
        if (roll < midWeight)
            return "refined_spirit_stone";
            
        return "high_spirit_stone";
    }
}
