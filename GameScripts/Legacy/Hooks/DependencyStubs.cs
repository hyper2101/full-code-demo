using UnityEngine;
using System.Collections.Generic;

// --- DOTween ---
namespace DG.Tweening {
    public static class DOTween {
        public static Tween To(Core.DOGetter<float> getter, Core.DOSetter<float> setter, float endValue, float duration) { return null; }
        public static int KillAll(bool complete = false, params object[] idsOrTargetsToExclude) { return 0; }
    }
    public class Tween {}
    public class Sequence {}
    public class Tweener {}
    public static class ShortcutExtensions {
        public static Tween DOAnchorPos(this RectTransform target, Vector2 endValue, float duration, bool snapping = false) { return null; }
        public static Tween DOScale(this Transform target, Vector3 endValue, float duration) { return null; }
        public static Tween DOScale(this Transform target, float endValue, float duration) { return null; }
        public static Tween DOMove(this Transform target, Vector3 endValue, float duration, bool snapping = false) { return null; }
        public static Tween DOLocalMove(this Transform target, Vector3 endValue, float duration, bool snapping = false) { return null; }
        public static Tween DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f) { return null; }
        public static void DOKill(this Component target, bool complete = false) {}
    }
    namespace Core {
        public delegate T DOGetter<out T>();
        public delegate void DOSetter<in T>(T pNewValue);
    }
    namespace Plugins.Options {
        public struct FloatOptions {}
        public struct VectorOptions {}
    }
}

// --- Steamworks ---
namespace Steamworks {
    public static class SteamManager {
        public static bool Initialized = false;
    }
    public static class SteamUserStats {
        public static bool RequestCurrentStats() { return false; }
        public static bool GetAchievement(string pchName, out bool pbAchieved) { pbAchieved = false; return false; }
        public static bool SetAchievement(string pchName) { return false; }
        public static bool StoreStats() { return false; }
    }
    public static class SteamFriends {
        public static void ActivateGameOverlayToWebPage(string pchURL, Steamworks.EActivateGameOverlayToWebPageMode eMode = Steamworks.EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default) {}
    }
    public enum EActivateGameOverlayToWebPageMode {
        k_EActivateGameOverlayToWebPageMode_Default = 0
    }
}

// --- Shapes ---
namespace Shapes {
    public class Line : MonoBehaviour {
        public Vector3 Start;
        public Vector3 End;
        public Color Color;
        public float Thickness;
    }
    public class Disc : MonoBehaviour {
        public Color Color;
    }
}

// --- ImGui ---
namespace ImGuiNET {
    public static class ImGui {
        public static bool Begin(string name) { return false; }
        public static void End() {}
        public static void Text(string fmt) {}
        public static bool Button(string label) { return false; }
    }
}
namespace UImGui {
    public class UImGui : MonoBehaviour {
        public event System.Action Layout;
    }
}

// --- Harmony ---
namespace HarmonyLib {
    public class Harmony {
        public Harmony(string id) {}
        public void PatchAll() {}
    }
}
namespace DG.Tweening {
    public enum Ease { Linear, InOutQuad, OutBack, InQuad, OutQuad }
    public delegate void TweenCallback();
    public static class TweenSettingsExtensions {
        public static T SetEase<T>(this T t, Ease ease) where T : Tween { return t; }
        public static T OnComplete<T>(this T t, TweenCallback action) where T : Tween { action?.Invoke(); return t; }
        public static T From<T>(this T t, Vector2 fromValue, bool setImmediately = true, bool isRelative = false) where T : Tween { return t; }
        public static T SetDelay<T>(this T t, float delay) where T : Tween { return t; }
        public static T SetUpdate<T>(this T t, bool isIndependentUpdate) where T : Tween { return t; }
    }
}
