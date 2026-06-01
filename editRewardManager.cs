using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Expedition\ExpeditionManager.cs";
        string content = File.ReadAllText(path);

        string oldLogic = @"List<string> rewards = new List<string> \{ ""card_reward_pack"", ""wood"", ""iron_ore"" \}; // Simple pool";
        
        string newLogic = @"int rewardCount = UnityEngine.Random.Range(2, 6);
                          List<string> rewards = new List<string>();
                          string[] pool = { ""card_reward_pack"", ""resource_gold"", ""resource_food"", ""item_iron_ore"", ""item_wood"" };
                          for (int i = 0; i < rewardCount; i++) rewards.Add(pool[UnityEngine.Random.Range(0, pool.Length)]);";

        content = Regex.Replace(content, oldLogic, newLogic);
        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
