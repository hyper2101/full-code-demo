using System.Collections;

namespace Mewtations.Core
{
    public static class LegacyDemandHooks
    {
        public static IEnumerator CheckDemands(int currentMonth)
        {
            if (!LegacyRuntimeFlags.EnableDemands || DemandManager.instance == null) yield break;
            yield return DemandManager.instance.CheckDemands(currentMonth);
        }
    }
}
