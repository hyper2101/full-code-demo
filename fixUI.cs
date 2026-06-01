using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Combat\UI\CombatOverlayUI.cs";
        string[] lines = File.ReadAllLines(path);
        List<string> newLines = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("if (_draggedRingItem != null && Event.current.type == EventType.Repaint)"))
            {
                newLines.Add("            }");
                newLines.Add("        }");
            }
            newLines.Add(lines[i]);
        }
        
        File.WriteAllLines(path, newLines, System.Text.Encoding.UTF8);
    }
}
