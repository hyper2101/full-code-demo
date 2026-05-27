using System.Collections.Generic;

namespace Mewtations.Core.Environment
{
    public class BiomeState
    {
        public string BiomeId { get; set; }
        public List<RegionModifier> ActiveModifiers { get; set; } = new List<RegionModifier>();
    }
}
