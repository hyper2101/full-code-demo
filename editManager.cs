using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Expedition\ExpeditionManager.cs";
        string content = File.ReadAllText(path);

        string oldRewardLogic = @"if (CurrentBackpack != null && CurrentBackpack.IsFull)
                      {
                          DialogueResult(""Túi Đồ Đã Đầy!"", ""Không còn chỗ chứa! Hãy mở túi đồ (góc trái) để vứt bớt vật phẩm không cần thiết, sau đó quay lại nhặt tiếp."");
                          return;
                      }
                      if (CurrentBackpack != null) CurrentBackpack.AddItem(""card_reward_pack"");
                      DialogueResult(""Thu Hoạch Bất Ngờ"", ""Đã thu thập 1 Thẻ Phần Thưởng! Hãy mang về Base để mở."");
                      CompleteNodeResolution();";

        string newRewardLogic = @"if (ExpeditionRewardUI.Instance != null)
                      {
                          List<string> rewards = new List<string> { ""card_reward_pack"", ""wood"", ""iron_ore"" }; // Simple pool
                          ExpeditionRewardUI.Instance.ShowRewards(rewards);
                      }
                      else
                      {
                          CompleteNodeResolution();
                      }";

        // Note: Because of exact string matching with potential different indents/line endings, it's safer to use Regex.
        content = Regex.Replace(content, @"if\s*\(CurrentBackpack\s*!=\s*null\s*&&\s*CurrentBackpack\.IsFull\)\s*\{[^}]*\}\s*if\s*\(CurrentBackpack\s*!=\s*null\)\s*CurrentBackpack\.AddItem\(""card_reward_pack""\);\s*DialogueResult\([^;]*\);", newRewardLogic);
        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
