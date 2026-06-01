using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"GameScripts\Expedition\ExpeditionSystems.cs";
        string[] lines = File.ReadAllLines(path);
        List<string> newLines = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (i >= 182 && i <= 186) continue;
            if (i >= 204 && i <= 210) continue;
            newLines.Add(lines[i]);
        }
        
        File.WriteAllLines(path, newLines, System.Text.Encoding.UTF8);
    }
}
