using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Expedition\ExpeditionRewardUI.cs";
        string content = File.ReadAllText(path);

        // Update HideRewards to RewardScreenClosed
        content = content.Replace("public void HideRewards()", "public void RewardScreenClosed()");
        content = content.Replace("HideRewards();", "RewardScreenClosed();");

        // Update OnGUI logic
        string oldGuiLogic = @"if \(GUILayout\.Button\(Mewtations\.Core\.MewtationsLoc\.Translate\(""exp_reward_close"", ""ĐÓNG \(Tiếp tục chuyến đi\)""\), _buttonStyle, GUILayout\.Height\(50\)\)\)\s*\{\s*RewardScreenClosed\(\);\s*\}";
        string newGuiLogic = @"
            if (_availableRewards.Count > 0)
            {
                if (GUILayout.Button(Mewtations.Core.MewtationsLoc.Translate(""exp_reward_skip"", ""BỎ QUA TẤT CẢ (SKIP REST)""), _buttonStyle, GUILayout.Height(50)))
                {
                    _availableRewards.Clear();
                }
            }
            else
            {
                if (GUILayout.Button(Mewtations.Core.MewtationsLoc.Translate(""exp_reward_continue"", ""TIẾP TỤC (CONTINUE)""), _buttonStyle, GUILayout.Height(50)))
                {
                    RewardScreenClosed();
                }
            }
";
        content = Regex.Replace(content, oldGuiLogic, newGuiLogic);

        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
