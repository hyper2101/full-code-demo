using System;
using UnityEngine;
using Mewtations.Combat;

namespace Mewtations.Combat
{
    public static class PermanentScar
    {
        public const string CrippledMeridians = "scar_crippled_meridians"; // Phế Mạch: -30 Speed
        public const string BloodDepletion = "scar_blood_depletion";       // Khuyết Huyết: -15 Max HP
        public const string SoulScar = "scar_soul_scar";                   // Hồn Thương: -20 Starting Rage

        public static string GetDisplayName(string id)
        {
            switch (id)
            {
                case CrippledMeridians: return "Phế Mạch (Crippled)";
                case BloodDepletion: return "Khuyết Huyết (Blood Depleted)";
                case SoulScar: return "Hồn Thương (Soul Scarred)";
                default: return "Vết Sẹo Lạ";
            }
        }

        public static string GetDescription(string id)
        {
            switch (id)
            {
                case CrippledMeridians: return "Kinh mạch phế tắc, giảm vĩnh viễn 30 Thần tốc (Speed).";
                case BloodDepletion: return "Khuyết tổn huyết khí, giảm vĩnh viễn 15 Sinh mệnh tối đa (Max HP).";
                case SoulScar: return "Linh hồn bị thương tổn, vừa vào trận chiến bị khấu trừ 20 điểm Nộ khí.";
                default: return "Vết thương tích tụ trong linh mạch.";
            }
        }

        public static IMewtationsComponent CreateComponent(string id)
        {
            switch (id)
            {
                case CrippledMeridians: return new CrippledMeridiansComponent();
                case BloodDepletion: return new BloodDepletionComponent();
                case SoulScar: return new SoulScarComponent();
                default: return null;
            }
        }

        private class CrippledMeridiansComponent : IMewtationsComponent
        {
            public string Id => CrippledMeridians;
            public string DisplayName => GetDisplayName(CrippledMeridians);
            public string Description => GetDescription(CrippledMeridians);
            public void Initialize(CombatUnit unit)
            {
                unit.Speed = Mathf.Max(10, unit.Speed - 30);
            }
        }

        private class BloodDepletionComponent : IMewtationsComponent
        {
            public string Id => BloodDepletion;
            public string DisplayName => GetDisplayName(BloodDepletion);
            public string Description => GetDescription(BloodDepletion);
            public void Initialize(CombatUnit unit)
            {
                // Managed during MaxHP processed calculations
            }
        }

        private class SoulScarComponent : IMewtationsComponent
        {
            public string Id => SoulScar;
            public string DisplayName => GetDisplayName(SoulScar);
            public string Description => GetDescription(SoulScar);
            public void Initialize(CombatUnit unit)
            {
                unit.CurrentRage = Mathf.Max(0, unit.CurrentRage - 20);
            }
        }
    }
}
