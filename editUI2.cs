using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Combat\UI\CombatOverlayUI.cs";
        string content = File.ReadAllText(path);

        string buttonLogic = @"
                        if (GUILayout.Button(buttonText, slotStyle, GUILayout.Width(60), GUILayout.Height(60)))
                        {
                            if (itemCard != null) _selectedRingItem = itemCard;
                        }
                        Rect btnRect = GUILayoutUtility.GetLastRect();
                        Event eContext = Event.current;
                        if (eContext.type == EventType.MouseDrag && btnRect.Contains(eContext.mousePosition) && itemCard != null)
                        {
                            _draggedRingItem = itemCard;
                        }
";
        content = Regex.Replace(content, @"if\s*\(GUILayout\.Button\(buttonText,\s*slotStyle,\s*GUILayout\.Width\(60\),\s*GUILayout\.Height\(60\)\)\)\s*\{\s*if\s*\(itemCard\s*!=\s*null\)\s*_selectedRingItem\s*=\s*itemCard;\s*\}", buttonLogic);

        string ghostCardLogic = @"
            if (_draggedRingItem != null && Event.current.type == EventType.Repaint)
            {
                Rect ghostRect = new Rect(_dragMousePosition.x - 30, _dragMousePosition.y - 30, 60, 60);
                GUI.Box(ghostRect, _draggedRingItem.CardData.Name, _unitCardStyle);
            }
        }
";
        content = Regex.Replace(content, @"}\s*$", ghostCardLogic);

        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
    }
}
