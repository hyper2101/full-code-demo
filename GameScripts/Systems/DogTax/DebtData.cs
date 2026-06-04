using System;
using System.Collections.Generic;

namespace GameScripts.Systems.DogTax
{
    [Serializable]
    public class DebtData
    {
        public List<string> RequiredResources = new List<string>();
        public int ExpirationMonth;
        public Mewtations.Core.Severity Severity;
    }
}
