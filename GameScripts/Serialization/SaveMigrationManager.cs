using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ISaveMigrationStep
{
    int FromVersion { get; }
    int ToVersion { get; }
    SaveGame Migrate(SaveGame save);
}

public static class SaveMigrationManager
{
    private static readonly List<ISaveMigrationStep> MigrationSteps = new List<ISaveMigrationStep>
    {
        // incremental steps will be registered here, e.g. v1 -> v2
    };

    public static SaveGame ExecuteMigration(SaveGame save, int targetVersion)
    {
        int safetyCounter = 0;
        while (save.SaveDataVersion < targetVersion && safetyCounter < 100)
        {
            safetyCounter++;
            int currentVersion = save.SaveDataVersion;
            var step = MigrationSteps.FirstOrDefault(s => s.FromVersion == currentVersion && s.ToVersion == currentVersion + 1);
            if (step == null)
            {
                // If there's no step, we assume direct bump to align with version updates
                save.SaveDataVersion = currentVersion + 1;
                continue;
            }
            save = step.Migrate(save);
            save.SaveDataVersion = currentVersion + 1;
        }
        return save;
    }
}
