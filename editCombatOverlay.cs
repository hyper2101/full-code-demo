using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Combat\UI\CombatOverlayUI.cs";
        string content = File.ReadAllText(path);

        // Make DrawOrderingInventoryUI public
        content = content.Replace("private void DrawOrderingInventoryUI(float screenWidth, float screenHeight)", "public void DrawOrderingInventoryUI(float screenWidth, float screenHeight)");

        // Add DrawInventoryExternal
        string externalMethod = @"
        public void DrawInventoryExternal()
        {
            InitializeStyles();
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            DrawOrderingInventoryUI(screenWidth, screenHeight);
        }
";
        if (!content.Contains("DrawInventoryExternal"))
        {
            content = Regex.Replace(content, @"public void DrawOrderingInventoryUI", externalMethod + "\n        public void DrawOrderingInventoryUI");
        }

        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
