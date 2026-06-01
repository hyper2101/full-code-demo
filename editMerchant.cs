using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Expedition\ExpeditionManager.cs";
        string content = File.ReadAllText(path);

        string oldMerchantLogic = @"if \(foodIdx >= 0\)\s*\{\s*CurrentBackpack\.RemoveItemAt\(foodIdx\);\s*CurrentBackpack\.AddItem\(""item_ancient_relic_auto_collect""\);\s*DialogueResult\(""Giao dịch thành công"", ""Đổi 1 Food lấy Cổ Vật Tự Động Nhặt!""\);";

        string newMerchantLogic = @"if (foodIdx >= 0)
                          {
                              CurrentBackpack.RemoveItemAt(foodIdx);
                              
                              string[] merchantLootPool = { ""item_ancient_relic_auto_collect"", ""item_ancient_relic_auto_farm"", ""item_ancient_relic_insurance"", ""card_reward_pack"", ""potion_major_healing"" };
                              string chosenItem = merchantLootPool[UnityEngine.Random.Range(0, merchantLootPool.Length)];
                              
                              CurrentBackpack.AddItem(chosenItem);
                              DialogueResult(""Giao dịch thành công"", ""Đổi Food/Gold lấy một vật phẩm bí ẩn!"");";

        content = Regex.Replace(content, oldMerchantLogic, newMerchantLogic);

        string oldMerchantLogicGold = @"if \(goldIdx >= 0\)\s*\{\s*CurrentBackpack\.RemoveItemAt\(goldIdx\);\s*CurrentBackpack\.AddItem\(""item_ancient_relic_auto_collect""\);\s*DialogueResult\(""Giao dịch thành công"", ""Đổi 1 Gold lấy Cổ Vật Tự Động Nhặt!""\);";
        
        string newMerchantLogicGold = @"if (goldIdx >= 0)
                          {
                              CurrentBackpack.RemoveItemAt(goldIdx);
                              
                              string[] merchantLootPool = { ""item_ancient_relic_auto_collect"", ""item_ancient_relic_auto_farm"", ""item_ancient_relic_insurance"", ""card_reward_pack"", ""potion_major_healing"" };
                              string chosenItem = merchantLootPool[UnityEngine.Random.Range(0, merchantLootPool.Length)];
                              
                              CurrentBackpack.AddItem(chosenItem);
                              DialogueResult(""Giao dịch thành công"", ""Đổi Food/Gold lấy một vật phẩm bí ẩn!"");";
        
        content = Regex.Replace(content, oldMerchantLogicGold, newMerchantLogicGold);

        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
