using UnityEngine;
using System.Collections.Generic;

public class SaveLoader : MonoBehaviour
{
    public const int CURRENT_SAVE_VERSION = 2;

    public void LoadSaveData(string jsonData)
    {
        // 1. Parse JSON
        // 2. Check Version. If < CURRENT_SAVE_VERSION, reject!
        // 3. Clear RuntimeCardRegistry
        // 4. Reconstruct RuntimeStates and register them
        // 5. Spawn Views (GameCards)
        // 6. Rebuild Containers (restore ownership via TransactionSystem)
        // 7. Emit Reconstruction Events
    }

    public void SaveGame()
    {
        // 1. Gather all states from RuntimeCardRegistry
        // 2. Serialize to JSON with SaveVersion = CURRENT_SAVE_VERSION
        // 3. Write to disk
    }
}
