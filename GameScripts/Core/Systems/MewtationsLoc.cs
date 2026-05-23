using System;
using System.Collections.Generic;
using UnityEngine;

public static class MewtationsLoc
{
    public enum Language { English, Vietnamese }

    public static Language CurrentLang
    {
        get
        {
            if (SokLoc.instance != null && !string.IsNullOrEmpty(SokLoc.instance.CurrentLanguage))
            {
                string lang = SokLoc.instance.CurrentLanguage.ToLower();
                if (lang.Contains("vietnamese") || lang.Contains("vi"))
                {
                    return Language.Vietnamese;
                }
            }
            return Language.English;
        }
    }

    private static readonly Dictionary<string, Dictionary<Language, string>> _dict = new Dictionary<string, Dictionary<Language, string>>();

    static MewtationsLoc()
    {
        // UI - Chronicle Button & Window
        Add("btn_chronicle", "📖 Chronicle of Truth", "📖 Kí Sự Sự Thật");
        Add("win_chronicle_title", "📖 CHRONICLE OF TRUTH", "📖 KÍ SỰ SỰ THẬT TÔNG MÔN");
        Add("win_chronicle_desc", "Contains ancient scrolls and letter archives that resolve the tragic Cat-Dog conflict.", "Kho lưu trữ thư tịch cổ khơi thông sự thật về đại kiếp mâu thuẫn phe phái Mèo - Chó.");
        Add("btn_close", "Close", "Đóng");
        Add("btn_read", "Read Fragment", "Đọc Bản Thư");
        Add("lbl_recipe", "Recipe Status:", "Trạng Thái Công Thức:");
        Add("lbl_unlocked", "✓ Unlocked", "✓ Đã Mở Khóa");
        Add("lbl_locked", "✗ Locked (Find in Expedition to open)", "✗ Khóa (Tìm manh mối cổ bản trong Viễn Chinh)");
        Add("lbl_lost_fragment", "🔒 Lost Scroll Fragment", "🔒 Mảnh Cổ Bản Bị Thất Lạc");

        // Recipe Details
        Add("recipe_1_details", "💡 Talisman Fusion: Breakthrough Qi Refining + Divine Stone + Any Equipment", 
                               "💡 Dung Hợp Bùa Chú: Đột Phá Luyện Khí Trận + Hóa Thần Thạch + Trang Bị Bất Kỳ");
        Add("recipe_2_details", "💡 Advanced Breakthrough Pill: Stove + 2x Rare Food Items", 
                               "💡 Linh Dược Đột Phá: Bếp + 2x Thức Ăn Quý Hiếm");
        Add("recipe_3_details", "💡 True Harmony Covenant: Breakthrough Array + Level 4 Cat + 3 Clue Fragments", 
                               "💡 Nghi Thức Thái Hòa: Đột Phá Trận + Mèo Cảnh Giới 4 + 3 Mảnh Cổ Bản Kí Sự");

        // Hints & Lore Cards
        Add("hint_1_title", "Ancient Chronicle - Fragment I", "Cổ Bản Kí Sự - Mảnh I");
        Add("hint_1_desc", "Double-click to read the ancient records of Faction Order.", "Nhấp đúp chuột để đọc sử sách xưa kia của Đế chế Trật tự Chó.");
        Add("hint_1_body", "In the beginning, Dogs and Cats walked together under the spiritual sky. Dogs forged tools and rules; Cats opened their spirits for harmony. But as spiritual energy waned, Greed rose, and the Dogs established a rigid Iron Order, pushing Cats to the bottom of society to scavenge trash...\n\n💡 [Secret Recipe]: Breakthrough Qi Refining can fuse Talismans using Divine Stones.", 
                          "Thuở hồng hoang, loài Chó và Mèo cùng bước đi dưới vòm trời linh khí. Loài Chó chế tạo công cụ và giữ luật lệ, loài Mèo mở ra linh căn cảm ngộ thái hòa. Nhưng khi linh khí cạn kiệt, lòng Tham Lam trỗi dậy, loài Chó đã thiết lập Đế chế Trật tự sắt đá, đày đọa loài Mèo xuống đáy xã hội làm nô dịch bới rác...\n\n💡 [Công Thức Bí Truyền]: Đột Phá Luyện Khí có thể dung hợp Talisman bằng cách đặt Hóa Thần Thạch.");

        Add("hint_2_title", "Cat God's Memoir - Fragment II", "Kí Sự Thần Mèo - Mảnh II");
        Add("hint_2_desc", "Double-click to read the origin of the Cat God's sacrifice.", "Nhấp đúp chuột để đọc về nguồn gốc sự hiến tế của Thần Mèo.");
        Add("hint_2_body", "The Cat God is not an evil cosmic horror, but the Will of Chaos born to balance the suffocating Order of the Dogs. With each sacrifice, the Cat God absorbs spiritual energy and grants destiny. But if the player grows too greedy without appeasing, the heavens will strike...\n\n💡 [Secret Recipe]: Brew High Breakthrough Pills by cooking 2 rare Food items.", 
                          "Thần Mèo thực ra không phải là thực thể tà ác, ngài chính là Ý Chí Hỗn Loạn được sinh ra để cân bằng lại Trật Tự ngột ngạt của loài Chó. Mỗi lần dâng hiến hiến tế, Thần Mèo sẽ hấp thụ linh lực tích tụ và phản hồi cơ duyên. Nhưng nếu người chơi quá tham lam nhận phần thưởng cao mà không xoa dịu, thiên đạo sẽ phẫn nộ giáng họa...\n\n💡 [Công Thức Bí Truyền]: Luyện chế Linh Dược Đột Phá cấp cao bằng cách đun 2 Thức ăn quý hiếm.");

        Add("hint_3_title", "True Harmony Covenant - Fragment III", "Hiệp Ước Thái Hòa - Mảnh III");
        Add("hint_3_desc", "Double-click to read the ultimate pathway to peace.", "Nhấp đúp chuột để đọc về con đường Đạo Pháp Thái Hòa.");
        Add("hint_3_body", "Freedom and Order cannot destroy each other; the world always oscillates between both. Only when this is understood, can Cats and Dogs break the cycle of hatred. Place these 3 fragments with a Nascent Soul Cat (Breakthrough 4) in the Breakthrough Array to achieve eternal True Harmony.\n\n💡 [Covenant Ritual]: Place 3 Fragments and a Nascent Soul Cat in the Breakthrough Array to unlock True Harmony.", 
                          "Tự Do và Trật Tự không thể triệt tiêu lẫn nhau, thế giới luôn dao động giữa hai thái cực. Chỉ khi hiểu được điều này, Mèo và Chó mới thoát khỏi vòng lặp thù hận. Hãy đặt 3 mảnh Cổ Bản này cùng một Mèo Nguyên Anh Cảnh (Breakthrough 4) tại Đột Phá Trận để khai mở Bản Mệnh Thái Hòa vĩnh cửu.\n\n💡 [Nghi Thức Bản Mệnh]: Đặt 3 Cổ Bản và Mèo Nguyên Anh Cảnh vào Đột Phá Trận để đạt Thiên Đạo Thái Hòa.");

        // Talents
        Add("talent_true_harmony_name", "True Harmony Covenant", "Bản Mệnh Thái Hòa");
        Add("talent_true_harmony_desc", "Attained supreme enlightenment: +30% Max HP, +30% Speed, removes all scars/mutations, and immune to future scars.", "Đạt tới ngộ đạo tối thượng: Tăng vĩnh viễn 30% HP, 30% Tốc độ, loại bỏ hoàn toàn vết sẹo/dị biến và miễn nhiễm thiên lôi kiếp số.");

        // Dialog Weary Dog Guard Encounter
        Add("dog_patrol_title", "🐕 THE WEARY DOG PATROL OFFICER", "🐕 LÍNH GÁC CHÓ TRĨU NẶNG ĐẠO TÂM");
        Add("dog_patrol_desc", "A heavily armored Dog patrol officer sits under a street lamp, looking incredibly exhausted from enforcing the rigid laws of the Dog Empire. He blocks your path to the ancient ruins.\n\nHe sighs heavily: 'Why must we fight? Why must we enforce order on those who just want to live? I am so tired...'", 
                               "Một lính gác Chó tuần tra bọc giáp sắt đang ngồi dưới đèn đường, trông cực kỳ kiệt quệ và mệt mỏi vì phải thực thi những đạo luật gò bó cứng nhắc của Đế chế Chó. Chú gác cửa chặn lối vào cổ cung phế tích.\n\nChú thở dài trĩu nặng: 'Tại sao chúng ta phải chiến đấu? Tại sao phải cưỡng ép trật tự lên những kẻ chỉ muốn sinh tồn? Ta mệt mỏi quá rồi...'");
        Add("opt_fight", "⚔️ Force breakthrough (+20 Corruption)", "⚔️ Quyết chiến đột phá (+20 Corruption)");
        Add("opt_stealth", "🏃 Sneak past silently (Requires Speed > 115)", "🏃 Lén lút lẻn qua (Yêu cầu Tốc độ > 115)");
        Add("opt_comfort", "☯️ [Zen Dao Comfort] Teach human philosophy & soothe his soul", "☯️ [Thiền Đạo Cảm Hóa] Thuyết giảng Đạo lý Nhân sinh & An ủi");
        Add("opt_comfort_req", "Requires Zen Cat", "Cần có Mèo Thiền Đạo");

        Add("dog_fight_res", "Bloody Skirmish!", "Huyết Chiến Đẫm Máu!");
        Add("dog_fight_res_desc", "You fought and defeated the guard. The path is clear, but at a bloody cost (+20 Corruption).", "Toàn đội tuốt kiếm huyết chiến đánh bại lính gác. Lối đi đã mở, nhưng sát nghiệp tích tụ cực nặng (+20 Corruption)!");

        Add("dog_stealth_success", "Stealth Success!", "Lẻn Qua Thành Công!");
        Add("dog_stealth_success_desc", "Your agile cats slipped by in the shadows without alerting the guard.", "Bằng bước di chuyển thần tốc, toàn đội đã lướt qua trạm canh gác trót lọt mà lính chó không hề hay biết!");

        Add("dog_stealth_fail", "Stealth Failed!", "Lẻn Qua Thất Bại!");
        Add("dog_stealth_fail_desc", "The weary guard noticed you. You had to force your way through and suffered minor injuries (-5 HP).", "Bị phát hiện! Do tốc độ quá chậm, lính gác giật mình báo động buộc phải đánh giáp lá cà, toàn đội bị thương nhẹ (-5 HP)!");

        Add("dog_comfort_success", "A Soul Redeemed!", "Giác Ngộ Đạo Tâm!");
        Add("dog_comfort_success_desc", "The officer wept upon hearing your Zen words, realizing both Cats and Dogs are victims of the system. He abandons his post, giving you an Ancient Scroll and purging your sins (-25 Corruption)!", 
                                  "Lính tuần tra Chó cảm kích rơi lệ khi nghe lời thuyết pháp thiền đạo đầy triết lý, giác ngộ rằng cả Chó và Mèo đều là nạn nhân bị hệ thống trật tự nghiền nát. Chú cởi bỏ giáp sắt từ chức, tặng bạn một mảnh Cổ Bản Kí Sự cực hiếm và giải thoát tà khí cho toàn đội (-25 Corruption)!");
    }

    private static void Add(string key, string en, string vi)
    {
        _dict[key.ToLower()] = new Dictionary<Language, string> {
            { Language.English, en },
            { Language.Vietnamese, vi }
        };
    }

    public static string Translate(string key, string defaultText = "")
    {
        if (string.IsNullOrEmpty(key)) return defaultText;
        string k = key.ToLower();
        if (_dict.TryGetValue(k, out var langs))
        {
            return langs[CurrentLang];
        }
        return defaultText;
    }
}
