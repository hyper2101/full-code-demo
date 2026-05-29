#if !USE_HARMONY
using System;

namespace HarmonyLib
{
    // STUB: Temporary door to bypass compiler errors.
    // To restore real modding:
    // 1. Add 0Harmony.dll to Plugins folder
    // 2. Add USE_HARMONY to Scripting Define Symbols in Player Settings
    
    public class Harmony
    {
        public Harmony(string id) 
        {
            UnityEngine.Debug.LogWarning("[HarmonyStub] Fake Harmony instance created for: " + id);
        }
    }

    public class HarmonyPatch : Attribute {}
    public class HarmonyPrefix : Attribute {}
    public class HarmonyPostfix : Attribute {}
    public class HarmonyTranspiler : Attribute {}
}
#endif
