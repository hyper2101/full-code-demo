using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Serialization
{
    public static class SaveMigrationPipeline
    {
        private static readonly List<ISaveMigrationStep> MigrationSteps = new List<ISaveMigrationStep>();

        public static void RegisterStep(ISaveMigrationStep step)
        {
            if (!MigrationSteps.Contains(step))
            {
                MigrationSteps.Add(step);
            }
        }

        public static void ProcessMigrations(SaveData saveData)
        {
            if (saveData == null) return;

            // Sort steps ascending by TargetVersion
            var sortedSteps = MigrationSteps.OrderBy(s => s.TargetVersion).ToList();

            foreach (var step in sortedSteps)
            {
                if (saveData.SaveDataVersion < step.TargetVersion)
                {
                    Debug.Log($"[SaveMigrationPipeline] Migrating save data to version {step.TargetVersion} ({step.GetType().Name})");
                    step.Migrate(saveData);
                    saveData.SaveDataVersion = step.TargetVersion;
                }
            }
        }
    }
}
