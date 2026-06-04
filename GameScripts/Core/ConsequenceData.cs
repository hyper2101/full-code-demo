using System;

namespace Mewtations.Core
{
    public enum ConsequenceType
    {
        LoseResource,
        LoseCat,
        DestroyBuilding,
        PoliticalPunishment
    }

    [Serializable]
    public class ConsequenceData
    {
        public ConsequenceType Type;
        public int Magnitude;
        public Severity Severity;
    }
}
