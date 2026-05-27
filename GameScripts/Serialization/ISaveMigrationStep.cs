using System.Collections.Generic;

namespace Mewtations.Serialization
{
    public interface ISaveMigrationStep
    {
        int TargetVersion { get; }
        void Migrate(SaveData save);
    }
}
