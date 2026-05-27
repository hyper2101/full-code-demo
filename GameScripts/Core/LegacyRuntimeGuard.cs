using System;
using UnityEngine;

namespace Mewtations.Core
{
    public class LegacyRuntimeAccessException : Exception
    {
        public LegacyRuntimeAccessException(string message) : base(message) { }
    }

    public static class LegacyRuntimeGuard
    {
        /// <summary>
        /// Validates that an object being accessed is not a legacy system.
        /// Throws an exception if a legacy marker is detected at runtime.
        /// </summary>
        public static void ValidateAccess(object systemInstance, string accessContext)
        {
            if (systemInstance == null) return;

            if (systemInstance is ILegacySystemMarker)
            {
                var type = systemInstance.GetType();
                var attribute = (LegacySystemAttribute)Attribute.GetCustomAttribute(type, typeof(LegacySystemAttribute));
                string category = attribute != null ? attribute.Category.ToString() : "Unknown Legacy";
                
                throw new LegacyRuntimeAccessException(
                    $"[LegacyRuntimeGuard] Illegal runtime access to quarantined system '{type.Name}' " +
                    $"({category}) from context: {accessContext}. This system is deprecated and should not be used."
                );
            }
        }
    }
}
