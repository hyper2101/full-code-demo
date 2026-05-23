using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat
{
    public enum CatConstitution
    {
        None,
        TaMaLaoTo,       // High Corruption Scaling
        HonLoanTrieu,     // Low Stability Genius
        BaoLinhThienKieu, // Fragile Burst Cultivator
        KhoHanhTang       // Cursed Survivor
    }

    public static class MewtationsConstitutionRegistry
    {
        public static string GetDisplayName(CatConstitution cons)
        {
            switch (cons)
            {
                case CatConstitution.TaMaLaoTo: return "Tà Ma Lão Tổ";
                case CatConstitution.HonLoanTrieu: return "Hỗn Loạn Triều";
                case CatConstitution.BaoLinhThienKieu: return "Bạo Linh Thiên Kiêu";
                case CatConstitution.KhoHanhTang: return "Khổ Hạnh Tăng";
                default: return "Không Linh Căn";
            }
        }

        public static string GetDescription(CatConstitution cons)
        {
            switch (cons)
            {
                case CatConstitution.TaMaLaoTo:
                    return "Gây 1.5x sát thương khi Độ Ô Nhiễm (Corruption) >= 50%, nhưng tăng 200% tỷ lệ dính dị biến khi thám hiểm.";
                case CatConstitution.HonLoanTrieu:
                    return "Vừa vào trận nhận +50 Nộ và +30 Speed. Tuy nhiên, đòn đánh cơ bản có 10% cơ hội thất bại và tự phản phệ gây 3 sát thương.";
                case CatConstitution.BaoLinhThienKieu:
                    return "Đạt x2.0 sát thương bạo kích (Crit Damage), nhưng Sinh mệnh cực hạn bị khóa cứng ở tối đa 35 HP.";
                case CatConstitution.KhoHanhTang:
                    return "Tăng +50% Sức mạnh tấn công khi Sinh mệnh dưới 30%, nhưng không tài nào nhận được Khiên bảo hộ.";
                default:
                    return "Thần Miêu bình thường, không có biến dị thể chất đặc thù.";
            }
        }

        public static void ApplyConstitutionStats(CatConstitution cons, ref int maxHP, ref int speed)
        {
            if (cons == CatConstitution.BaoLinhThienKieu)
            {
                maxHP = Mathf.Min(35, maxHP); // Permanently capped at 35 HP
            }
        }
    }
}
