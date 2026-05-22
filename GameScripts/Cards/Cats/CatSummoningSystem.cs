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
        // Tính toán tỉ lệ Thiên Kiêu dựa trên Breakthrough level (Cấp càng cao, tỉ lệ càng tăng nhẹ)
        float thienKieuChance = 0.05f + (highestBreakthroughLevel * 0.02f); // Base 5%, mỗi cấp +2%
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

            if (isThienKieu)
            {
                // Thưởng chỉ số Thiên Kiêu
                catData.CustomName = "Thiên Tài " + catData.Name;
                catData.Speed += 30; // Nhanh hơn
                // TODO: Áp dụng StatusEffect "Thiên Kiêu" đặc biệt định hình lối chơi
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
