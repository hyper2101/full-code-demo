#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class LocalizationValidator : EditorWindow
{
    private string _report = "";
    private Vector2 _scrollPos;

    [MenuItem("Window/Mewtations/Localization Validator")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationValidator>("Localization Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mewtations: Dogma - Localization Validator Tool", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Run Localization Validation Audit", GUILayout.Height(35)))
        {
            RunAudit();
        }

        GUILayout.Space(10);
        GUILayout.Label("Validation Audit Report:", EditorStyles.boldLabel);
        
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    private void RunAudit()
    {
        _report = "=== LOCALIZATION AUDIT REPORT ===\n";
        _report += $"Run Time: {DateTime.Now}\n\n";

        // Load all keys from TSV
        HashSet<string> tsvKeys = new HashSet<string>();
        HashSet<string> duplicateKeys = new HashSet<string>();
        string tsvPath = Path.Combine(Application.dataPath, "Core/Systems/MewtationsLocTable.tsv");
        
        if (File.Exists(tsvPath))
        {
            string[] lines = File.ReadAllLines(tsvPath);
            _report += $"[TSV Info] Found TSV table at: {tsvPath} with {lines.Length - 1} records.\n";
            
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] cols = line.Split('\t');
                if (cols.Length > 0)
                {
                    string key = cols[0].Trim().ToLower();
                    if (tsvKeys.Contains(key))
                    {
                        duplicateKeys.Add(key);
                    }
                    else
                    {
                        tsvKeys.Add(key);
                    }
                }
            }
        }
        else
        {
            _report += $"[Warning] External TSV file not found at: {tsvPath}\n";
        }

        if (duplicateKeys.Count > 0)
        {
            _report += $"\n[CRITICAL] Found {duplicateKeys.Count} duplicate keys in TSV:\n";
            foreach (string dup in duplicateKeys)
            {
                _report += $"  • {dup}\n";
            }
        }
        else
        {
            _report += "\n✓ No duplicate keys found in TSV table.\n";
        }

        // Scan all C# scripts under GameScripts
        string scriptsPath = Path.Combine(Application.dataPath, ""); // GameScripts directory
        if (!Directory.Exists(scriptsPath))
        {
            scriptsPath = Directory.GetCurrentDirectory(); // Fallback to current directory
        }

        _report += $"\nScanning C# files under: {scriptsPath}...\n";

        string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        _report += $"Found {csFiles.Length} script files to analyze.\n";

        int hardcodedCount = 0;
        int missingKeyCount = 0;
        HashSet<string> referencedKeys = new HashSet<string>();

        // Regex to match Translate("key") or TranslateFormat("key")
        Regex locRegex = new Regex(@"MewtationsLoc\.(Translate|TranslateFormat)\(\s*""([^""]+)""", RegexOptions.Compiled);
        
        // Regex to detect hardcoded Vietnamese characters in string literals
        Regex hardcodedViRegex = new Regex(@"""[^""]*[\u00C0-\u1EF9]+[^""]*""", RegexOptions.Compiled);

        foreach (string file in csFiles)
        {
            // Skip validator script itself to avoid scanning its own strings
            if (file.Contains("LocalizationValidator.cs") || file.Contains("MewtationsLoc.cs")) continue;

            string[] lines = File.ReadAllLines(file);
            string fileRelativePath = Path.GetRelativePath(Application.dataPath, file);

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                string line = lines[lineNum];
                if (line.Trim().StartsWith("//")) continue; // Skip comments

                // 1. Check for referenced keys
                MatchCollection matches = locRegex.Matches(line);
                foreach (Match match in matches)
                {
                    string refKey = match.Groups[2].Value.ToLower();
                    referencedKeys.Add(refKey);

                    if (tsvKeys.Count > 0 && !tsvKeys.Contains(refKey))
                    {
                        _report += $"[MISSING KEY] Script '{fileRelativePath}' line {lineNum + 1}: Key '{refKey}' is used in code but is missing from TSV table!\n";
                        missingKeyCount++;
                    }
                }

                // 2. Check for hardcoded Vietnamese strings which should be in TSV
                MatchCollection hardcodedMatches = hardcodedViRegex.Matches(line);
                foreach (Match hMatch in hardcodedMatches)
                {
                    // Skip log and debug lines
                    if (line.Contains("Debug.Log") || line.Contains("Debug.LogWarning") || line.Contains("Debug.LogError")) continue;
                    
                    _report += $"[HARDCODED TEXT] Script '{fileRelativePath}' line {lineNum + 1}: Found hardcoded Vietnamese literal: {hMatch.Value}\n";
                    hardcodedCount++;
                }
            }
        }

        // Check for unused keys
        int unusedCount = 0;
        _report += "\n=== UNUSED KEYS SEARCH ===\n";
        foreach (string key in tsvKeys)
        {
            if (!referencedKeys.Contains(key))
            {
                // Exclude system keys that might be loaded dynamically like hint_1_title, resource_gold etc.
                if (key.StartsWith("hint_") || key.StartsWith("recipe_") || key.StartsWith("talent_") || key.StartsWith("resource_") || key.StartsWith("shrine_") || key.StartsWith("food_")) continue;
                
                _report += $"[UNUSED KEY] Key '{key}' exists in TSV but is never referenced in C# scripts.\n";
                unusedCount++;
            }
        }

        _report += $"\n=== SUMMARY ===\n";
        _report += $"• Duplicate Keys: {duplicateKeys.Count}\n";
        _report += $"• Missing Keys: {missingKeyCount}\n";
        _report += $"• Hardcoded Literals Found: {hardcodedCount}\n";
        _report += $"• Unused Keys Found: {unusedCount}\n";
        _report += $"\nAudit Completed successfully.\n";
    }
}
#endif
