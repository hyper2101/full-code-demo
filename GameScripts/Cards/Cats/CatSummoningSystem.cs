using System.Collections.Generic;
using UnityEngine;

public class CatSummoningSystem
{
    private WorldManager _world;

    public CatSummoningSystem(WorldManager world)
    {
        _world = world;
    }

    /// <summary>
    /// Triệu hồi một mèo mới tại vị trí chỉ định
    /// </summary>
    public GameCard SummonCat(Vector3 position, int highestBreakthroughLevel)
    {
        // Tính toán tỉ lệ Thiên Kiêu dựa trên Breakthrough level (Cấp càng cao, tỉ lệ càng tăng)
        float thienKieuChance = 0.20f + (highestBreakthroughLevel * 0.05f); // Base 20%, mỗi cấp +5%
        bool isThienKieu = Random.value <= thienKieuChance;

        string catId = "cat_basic"; // ID mặc định của mèo

        GameCard newCat = _world.CreateCard(position, catId, true, true, true);
        CatCardData catData = newCat.CardData as CatCardData;

        if (catData != null)
        {
            // Randomize Base Stats
            catData.Role = (CatRole)Random.Range(0, System.Enum.GetValues(typeof(CatRole)).Length);
            catData.Element = (CatElement)Random.Range(0, System.Enum.GetValues(typeof(CatElement)).Length);
            
            // Randomize Speed (80 to 120)
            catData.Speed = Random.Range(80, 121);
            catData.LineageGeneration = 1;
            
            string[] backgrounds = {
                "Truyền nhân của U Minh Miêu Tộc thế gia",
                "Thiên kiêu đệ tử của Thần Miêu Cổ Giáo",
                "Kẻ thức tỉnh linh căn ẩn giấu từ hoang dã",
                "Tán tu miêu tộc đắc đạo từ Phế tích viễn cổ",
                "Vạn cổ linh thú phục sinh tái thế",
                "Miêu tiên truyền nhân nắm giữ Lôi pháp thiên kiếp",
                "Chiến thần ngự tiền của Thần Mèo Đế Quốc",
                "Huyết mạch dị chủng thức tỉnh thời Thượng cổ"
            };
            string bg = backgrounds[Random.Range(0, backgrounds.Length)];
            catData.AddMemoir(Mewtations.Expedition.MemoirType.Birth, bg);

            if (isThienKieu)
            {
                // Thưởng chỉ số Thiên Kiêu và Thiên Phú định hình lối chơi
                string[] talents = { 
                    Mewtations.Expedition.HeavenlyTalent.HeavenlyPoisonBody, 
                    Mewtations.Expedition.HeavenlyTalent.DivineShieldProtection, 
                    Mewtations.Expedition.HeavenlyTalent.RageOvercharger, 
                    Mewtations.Expedition.HeavenlyTalent.MartialArtsCleave 
                };
                string rolledTalent = talents[Random.Range(0, talents.Length)];
                catData.AddTrait(rolledTalent);
                
                catData.CustomName = $"{Mewtations.Expedition.HeavenlyTalent.GetDisplayName(rolledTalent)} {catData.Name}";
                catData.Speed += 30; // Thần tốc tăng mạnh
                catData.AddMemoir(Mewtations.Expedition.MemoirType.Breakthrough, Mewtations.Expedition.HeavenlyTalent.GetDisplayName(rolledTalent), "Thiên biến thức tỉnh");
            }
        }

        return newCat;
    }

    /// <summary>
    /// Sa thải mèo
    /// </summary>
    public void DismissCat(GameCard catCard)
    {
        if (catCard.CardData is CatCardData)
        {
            // Có thể rơi ra một ít tài nguyên an ủi
            // _world.CreateCard(catCard.transform.position, "resource_gold", true, true, true);
            catCard.DestroyCard(true, true);
        }
    }
}
