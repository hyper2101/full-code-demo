using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class RewardPackCard : CardData
{
    public RewardPackData PackData;

    public RewardPackSaveData SaveData = new RewardPackSaveData();

    public override string GetPersistentDataJson()
    {
        return JsonUtility.ToJson(this.SaveData);
    }

    public override void LoadPersistentDataJson(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            this.SaveData = JsonUtility.FromJson<RewardPackSaveData>(json);
        }
    }

    public override void OnInitialCreate()
    {
        base.OnInitialCreate();
        if (SaveData.Packs.Count == 0 && PackData != null)
        {
            var instance = new RewardPackInstance
            {
                PackId = PackData.PackId,
                GeneratedCards = RewardPackGenerator.GeneratePackCards(PackData),
                OpenedCount = 0
            };
            SaveData.Packs.Add(instance);
        }
    }

    public override void Clicked()
    {
        OpenNextCard();
    }

    public void OpenNextCard()
    {
        if (SaveData.Packs.Count == 0) return;

        var currentPack = SaveData.Packs[0];

        if (currentPack.OpenedCount < currentPack.GeneratedCards.Count)
        {
            string cardToSpawn = currentPack.GeneratedCards[currentPack.OpenedCount];
            currentPack.OpenedCount++;

            WorldManager.instance.CreateCard(this.MyGameCard.transform.position, cardToSpawn, faceUp: true, velocity: new Vector3(0, 5, 0));
        }

        if (currentPack.OpenedCount >= currentPack.GeneratedCards.Count)
        {
            SaveData.Packs.RemoveAt(0);
        }

        if (SaveData.Packs.Count == 0)
        {
            this.MyGameCard.DestroyCard();
        }
        else
        {
            // Cập nhật lại UI sau khi mở
            this.MyGameCard.UpdateCardText();
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        if (otherCard is RewardPackCard)
        {
            return true;
        }
        return base.CanHaveCard(otherCard);
    }

    public override void UpdateCard()
    {
        base.UpdateCard();
        
        if (this.MyGameCard.IsDemoCard) return;

        // Tự động gộp pack nếu người chơi xếp pack khác lên trên
        if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is RewardPackCard childPack && childPack != this)
        {
            this.SaveData.Packs.AddRange(childPack.SaveData.Packs);
            childPack.SaveData.Packs.Clear();
            
            // Xóa pack con sau khi đã gộp
            childPack.MyGameCard.DestroyCard();
            
            this.MyGameCard.UpdateCardText();
        }
    }

    protected override string GetTooltipText()
    {
        StringBuilder sb = new StringBuilder();
        string packName = this.Name;
        if (PackData != null && !string.IsNullOrEmpty(PackData.PackNameLocId))
        {
            packName = MewtationsLoc.Translate(PackData.PackNameLocId);
        }
        sb.AppendLine($"{packName}"); 

        if (PackData != null && PackData.Entries != null && PackData.Entries.Count > 0)
        {
            if (!string.IsNullOrEmpty(PackData.DescriptionLocId))
            {
                sb.AppendLine($"<size=70%><i>{MewtationsLoc.Translate(PackData.DescriptionLocId)}</i></size>");
            }
            sb.AppendLine();
            sb.AppendLine(MewtationsLoc.Translate("reward_pack_possible_loot", "Có thể chứa:")); // Fallback to Vietnamese
            
            HashSet<string> seen = new HashSet<string>();
            foreach (var entry in PackData.Entries)
            {
                if (!seen.Contains(entry.CardId))
                {
                    seen.Add(entry.CardId);
                    CardData prefab = WorldManager.instance.GetCardPrefab(entry.CardId);
                    if (prefab != null)
                    {
                        sb.AppendLine($"- {prefab.Name}");
                    }
                }
            }
        }
        
        if (SaveData.Packs.Count > 1)
        {
            sb.AppendLine();
            sb.AppendLine($"Stack x{SaveData.Packs.Count}");
        }

        return sb.ToString();
    }
}
