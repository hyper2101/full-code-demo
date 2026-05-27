using System.Collections.Generic;

namespace Mewtations.Core.Narrative
{
    /// <summary>
    /// Replaces the old Stacklands Quest UI/Checklist.
    /// Tracks hidden triggers, world-state progression, unlock conditions,
    /// and expedition state memory for environmental narrative emergence.
    /// </summary>
    public class WorldStateTracker
    {
        private readonly HashSet<string> _unlockedFlags = new HashSet<string>();

        public void SetFlag(string flagId)
        {
            _unlockedFlags.Add(flagId);
        }

        public bool HasFlag(string flagId)
        {
            return _unlockedFlags.Contains(flagId);
        }
    }
}
