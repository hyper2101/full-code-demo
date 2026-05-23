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
        
        // 5 Vết sẹo mới bổ sung chiều sâu gameplay
        public const string BrokenClaws = "scar_broken_claws";             // Phế Trảo: -5 ATK
        public const string CursedMeridians = "scar_cursed_meridians";     // Phế Ấn: Khóa Ultimate Skill
        public const string BrokenFireVein = "scar_broken_fire_vein";       // Đứt Mạch Hỏa Linh: Phản phệ vũ khí Hỏa
        public const string HeartDemonPossessed = "scar_heart_demon_pos";  // Tâm Ma Quấn Thân: 20% giảm trúng
        public const string ShatteredSoul = "scar_shattered_soul";         // Hồn Phách Nứt Vỡ: Khóa Exp thường

        public static string GetDisplayName(string id)
        {
            switch (id)
            {
                case CrippledMeridians: return "Phế Mạch (Crippled)";
                case BloodDepletion: return "Khuyết Huyết (Blood Depleted)";
                case SoulScar: return "Hồn Thương (Soul Scarred)";
                case BrokenClaws: return "Phế Trảo (Broken Claws)";
                case CursedMeridians: return "Phế Ấn Kinh Mạch (Cursed)";
                case BrokenFireVein: return "Hỏa Mạch Đứt Gãy (Broken Fire Vein)";
                case HeartDemonPossessed: return "Tâm Ma Xâm Nhập (Heart Demon)";
                case ShatteredSoul: return "Hồn Phách Tổn Hao (Shattered Soul)";
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
                case BrokenClaws: return "Trảo lực suy kiệt, giảm vĩnh viễn 5 Sức tấn công (ATK).";
                case CursedMeridians: return "Bản mệnh chiêu thức bị phong ấn vĩnh viễn, không thể thi triển Ultimate Skill.";
                case BrokenFireVein: return "Kinh mạch hệ Hỏa đứt gãy. Sử dụng vũ khí/kỹ năng Hỏa gây phản phệ 2 HP lên chính mình.";
                case HeartDemonPossessed: return "Tâm ma quấy rối đạo tâm, làm giảm 20% tỷ lệ đánh trúng trong combat.";
                case ShatteredSoul: return "Hồn phách nứt vỡ, không thể nhận Exp viễn chinh thường. Chỉ có thể thăng cấp tại Thần Mèo.";
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
                case BrokenClaws: return new BrokenClawsComponent();
                case CursedMeridians: return new CursedMeridiansComponent();
                case BrokenFireVein: return new BrokenFireVeinComponent();
                case HeartDemonPossessed: return new HeartDemonPossessedComponent();
                case ShatteredSoul: return new ShatteredSoulComponent();
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

        private class BrokenClawsComponent : IMewtationsComponent
        {
            public string Id => BrokenClaws;
            public string DisplayName => GetDisplayName(BrokenClaws);
            public string Description => GetDescription(BrokenClaws);
            public void Initialize(CombatUnit unit)
            {
                // Managed during ATK processed calculations
            }
        }

        private class CursedMeridiansComponent : IMewtationsComponent
        {
            public string Id => CursedMeridians;
            public string DisplayName => GetDisplayName(CursedMeridians);
            public string Description => GetDescription(CursedMeridians);
            public void Initialize(CombatUnit unit)
            {
                // Managed in Ultimate Skill lock pipeline
            }
        }

        private class BrokenFireVeinComponent : IMewtationsComponent
        {
            public string Id => BrokenFireVein;
            public string DisplayName => GetDisplayName(BrokenFireVein);
            public string Description => GetDescription(BrokenFireVein);
            public void Initialize(CombatUnit unit)
            {
                // Managed in after-attack backfire pipeline
            }
        }

        private class HeartDemonPossessedComponent : IMewtationsComponent
        {
            public string Id => HeartDemonPossessed;
            public string DisplayName => GetDisplayName(HeartDemonPossessed);
            public string Description => GetDescription(HeartDemonPossessed);
            public void Initialize(CombatUnit unit)
            {
                // Managed in hit chance processed calculations
            }
        }

        private class ShatteredSoulComponent : IMewtationsComponent
        {
            public string Id => ShatteredSoul;
            public string DisplayName => GetDisplayName(ShatteredSoul);
            public string Description => GetDescription(ShatteredSoul);
            public void Initialize(CombatUnit unit)
            {
                // Managed in experience gain pipeline
            }
        }
    }
}
