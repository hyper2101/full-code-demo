using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Combat\UI\CombatOverlayUI.cs";
        string content = File.ReadAllText(path);

        // Add class variables for drag and drop
        if (!content.Contains("private GameCard _draggedRingItem = null;"))
        {
            content = content.Replace("private GameCard _selectedRingItem = null;", 
                "private GameCard _selectedRingItem = null;\n        private GameCard _draggedRingItem = null;\n        private Vector2 _dragMousePosition;");
        }

        // Add Trash slot drawing
        string trashLogic = @"
            GUILayout.Space(20);
            Rect trashRect = GUILayoutUtility.GetRect(screenWidth, 60);
            GUI.Box(trashRect, MewtationsLoc.Translate(""exp_trash_slot"", ""[ THÙNG RÁC - KÉO THẢ VÀO ĐÂY ĐỂ XÓA VĨNH VIỄN ]""), _headerStyle);

            Event e = Event.current;
            if (e.type == EventType.MouseUp)
            {
                if (_draggedRingItem != null)
                {
                    if (trashRect.Contains(e.mousePosition))
                    {
                        ringCard.InventoryContainer.Remove(_draggedRingItem);
                        _draggedRingItem.DestroyCard(true, true);
                    }
                    _draggedRingItem = null;
                }
            }
            if (e.type == EventType.MouseDrag && _draggedRingItem != null)
            {
                _dragMousePosition = e.mousePosition;
            }
            GUILayout.EndVertical();
";
        content = Regex.Replace(content, @"GUILayout\.EndVertical\(\);\s*\}\s*private void DrawLogPanel", trashLogic + "\n        }\n\n        private void DrawLogPanel");

        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
