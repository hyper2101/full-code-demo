using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChronicleManager
{
    private static readonly HashSet<string> _unlockedHintIds = new HashSet<string>();

    public static bool IsHintUnlocked(string hintId)
    {
        if (string.IsNullOrEmpty(hintId)) return false;
        return _unlockedHintIds.Contains(hintId.ToLower());
    }

    public static void UnlockHint(string hintId)
    {
        if (string.IsNullOrEmpty(hintId)) return;
        string key = hintId.ToLower();
        if (!_unlockedHintIds.Contains(key))
        {
            _unlockedHintIds.Add(key);
            Debug.Log($"[ChronicleManager] Unlocked secret lore hint: {key}");
        }
    }

    public static HashSet<string> GetUnlockedHints()
    {
        return _unlockedHintIds;
    }

    public static string Serialize()
    {
        return string.Join(",", _unlockedHintIds);
    }

    public static void Deserialize(string data)
    {
        _unlockedHintIds.Clear();
        if (string.IsNullOrEmpty(data)) return;
        
        string[] parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (!string.IsNullOrEmpty(part))
            {
                _unlockedHintIds.Add(part.Trim().ToLower());
            }
        }
        Debug.Log($"[ChronicleManager] Loaded {_unlockedHintIds.Count} unlocked hints.");
    }

    public static void Reset()
    {
        _unlockedHintIds.Clear();
    }
}
