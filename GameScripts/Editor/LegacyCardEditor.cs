#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Mewtations.Framework
{
    [CustomEditor(typeof(CardData), true)]
    public class LegacyCardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var targetType = target.GetType();
            var legacyAttr = targetType.GetCustomAttribute<LegacyContentAttribute>();

            if (legacyAttr != null)
            {
                EditorGUILayout.HelpBox($"[LEGACY CONTENT]\nThis card belongs to deprecated {legacyAttr.Origin} gameplay systems.\nReason: {legacyAttr.Reason}", MessageType.Warning);
            }

            base.OnInspectorGUI();
        }
    }
}
#endif
