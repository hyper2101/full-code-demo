using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MewtationsLoc
{
    public enum Language { English, Vietnamese, Chinese, Japanese, Korean }

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
                if (lang.Contains("chinese") || lang.Contains("zh") || lang.Contains("cn"))
                {
                    return Language.Chinese;
                }
                if (lang.Contains("japanese") || lang.Contains("ja") || lang.Contains("jp"))
                {
                    return Language.Japanese;
                }
                if (lang.Contains("korean") || lang.Contains("ko") || lang.Contains("kr"))
                {
                    return Language.Korean;
                }
            }
            return Language.English;
        }
    }

    private static readonly Dictionary<string, Dictionary<Language, string>> _dict = new Dictionary<string, Dictionary<Language, string>>();

    static MewtationsLoc()
    {
        // ----------------- STATIC FALLBACKS (In case TSV is missing) -----------------
        // UI - Chronicle Button & Window
        Add("btn_chronicle", "📖 Chronicle of Truth", "📖 Kí Sự Sự Thật", "📖 真相纪事", "📖 真実の年代記", "📖 진실의 연대기");
        Add("win_chronicle_title", "📖 CHRONICLE OF TRUTH", "📖 KÍ SỰ SỰ THẬT TÔNG MÔN", "📖 真相纪事宗门", "📖 真実の年代記宗門", "📖 진실의 연대기 문파");
        Add("win_chronicle_desc", "Contains ancient scrolls and letter archives that resolve the tragic Cat-Dog conflict.", "Kho lưu trữ thư tịch cổ khơi thông sự thật về đại kiếp mâu thuẫn phe phái Mèo - Chó.", "包含解开猫狗悲剧冲突的古老卷轴和信件档案。", "猫と犬の悲劇的な衝突を解決する古代の巻物と手紙のアーカイブが含まれています。", "고양이와 개의 비극적인 갈등을 해결하는 고대 두루마리와 편지 아카이브가 포함되어 있습니다.");
        Add("btn_close", "Close", "Đóng", "关闭", "閉じる", "닫기");
        Add("btn_read", "Read Fragment", "Đọc Bản Thư", "阅读残卷", "残片を読む", "잔해 읽기");
        Add("lbl_recipe", "Recipe Status:", "Trạng Thái Công Thức:", "配方状态：", "レシピステータス：", "제작법 상태:");
        Add("lbl_unlocked", "✓ Unlocked", "✓ Đã Mở Khóa", "✓ 已解锁", "✓ 解放済み", "✓ 해제됨");
        Add("lbl_locked", "✗ Locked (Find in Expedition to open)", "✗ Khóa (Tìm manh mối cổ bản trong Viễn Chinh)", "✗ 已锁定（在远征中寻找线索解锁）", "✗ ロック（遠征で手がかりを見つけて解放）", "✗ 잠김 (원정에서 단서를 찾아 잠금 해제)");
        Add("lbl_lost_fragment", "🔒 Lost Scroll Fragment", "🔒 Mảnh Cổ Bản Bị Thất Lạc", "🔒 遗失的残卷碎片", "🔒 失われた巻物の断片", "🔒 잃어버린 두루마리 조각");

        // Recipe Details
        Add("recipe_1_details", "💡 Talisman Fusion: Breakthrough Qi Refining + Divine Stone + Any Equipment", 
                               "💡 Dung Hợp Bùa Chú: Đột Phá Luyện Khí Trận + Hóa Thần Thạch + Trang Bị Bất Kỳ",
                               "💡 护符融合：突破炼气阵 + 化神石 + 任意装备",
                               "💡 護符融合：突破練気陣 + 化神石 + 任意の装備",
                               "💡 부적 융합: 돌파 연기진 + 화신석 + 임의의 장비");
        Add("recipe_2_details", "💡 Advanced Breakthrough Pill: Stove + 2x Rare Food Items", 
                               "💡 Linh Dược Đột Phá: Bếp + 2x Thức Ăn Quý Hiếm",
                               "💡 高级突破丹：炉灶 + 2x 稀有食物",
                               "💡 高級突破丹：炉灶 + 2x レアフード",
                               "💡 상급 돌파단: 화로 + 2x 희귀 음식");
        Add("recipe_3_details", "💡 True Harmony Covenant: Breakthrough Array + Level 4 Cat + 3 Clue Fragments", 
                               "💡 Nghi Thức Thái Hòa: Đột Phá Trận + Mèo Cảnh Giới 4 + 3 Mảnh Cổ Bản Kí Sự",
                               "💡 大和谐契约：突破阵 + 4级猫 + 3个线索残卷",
                               "💡 大調和の盟約：突破陣 + レベル4의貓 + 3개의 단서 조각",
                               "💡 대조화의 계약: 돌파진 + 4레벨 고양이 + 3개의 단서 조각");

        // Hints & Lore Cards
        Add("hint_1_title", "Ancient Chronicle - Fragment I", "Cổ Bản Kí Sự - Mảnh I", "古代纪事 - 残卷 I", "古代の年代記 - 断片 I", "고대 연대기 - 조각 I");
        Add("hint_1_desc", "Double-click to read the ancient records of Faction Order.", "Nhấp đúp chuột để đọc sử sách xưa kia của Đế chế Trật tự Chó.", "双击阅读犬帝国的古老秩序记录。", "ダブルクリックして犬の帝国の古代の秩序の記録を読みます。", "더블 클릭하여 개 제국의 고대 질서 기록을 읽습니다.");
        
        // Talents
        Add("talent_true_harmony_name", "True Harmony Covenant", "Bản Mệnh Thái Hòa", "大和谐契约", "大調和の盟約", "대조화의 계약");
        Add("talent_true_harmony_desc", "Attained supreme enlightenment: +30% Max HP, +30% Speed, removes all scars/mutations, and immune to future scars.", "Đạt tới ngộ đạo tối thượng: Tăng vĩnh viễn 30% HP, 30% Tốc độ, loại bỏ hoàn toàn vết sẹo/dị biến và miễn nhiễm thiên lôi kiếp số.", "获得无上顿悟：最大生命值+30%，速度+30%，移除所有伤疤/异变，并对未来的伤疤免疫。", "至高の悟りに達しました：最大HP+30％、速度+30％、すべての傷跡/異変を取り除き、将来の傷跡を免疫します。", "지고의 깨달음에 도달했습니다: 최대 체력 +30%, 속도 +30%, 모든 흉터/이변 제거 및 미래의 흉터에 면역됩니다.");

        // Dialog Weary Dog Guard Encounter
        Add("dog_patrol_title", "🐕 THE WEARY DOG PATROL OFFICER", "🐕 LÍNH GÁC CHÓ TRĨU NẶNG ĐẠO TÂM", "🐕 疲惫的的犬卫兵", "🐕 疲れ果てた犬の衛兵", "🐕 피로에 지친 개 경비병");

        // Shrine System & Cat God Fallbacks
        Add("shrine_desc_format", 
            "Cat God Shrine Formation. Attune ancient Relics here to awaken passive automation power and optimize your build.\n\n• <b>Max Relic Slots:</b> <color=#ffdd22>{0}</color>\n• Place a <b>Resonance Trophy</b> here to expand slots permanently.", 
            "Trận Pháp Điện Thờ Thần Mèo. Nơi an vị Cổ Vật cổ đại để kích hoạt Trận Pháp Tự Động Hóa và cộng hưởng Đạo Pháp.\n\n• <b>Số vị trí an vị Cổ Vật tối đa:</b> <color=#ffdd22>{0}</color>\n• Đặt <b>Linh Bảo Cộng Hưởng</b> vào để khai mở vị trí hiển thị Cổ Vật vĩnh viễn.");
        Add("shrine_upgrading", "Harmonizing Shrine energy...", "Đang cộng hưởng năng lượng Điện Thờ...");
        Add("shrine_upgraded_title", "☯️ FORMATION EXPANDED!", "☯️ TRẬN PHÁP KHAI MỞ!");
        Add("shrine_upgraded_desc", 
            "The Resonance Trophy has successfully attuned! Pure spiritual energy surges, expanding the Shrine's formation.\n🌟 <b>Max Relic display slots increased to:</b> <color=#ffdd22>{0} slots</color>!", 
            "Linh Bảo khai quang thành công! Luồng linh khí tinh khiết bùng phát từ Điện Thờ, mở rộng phạm vi Trận Pháp.\n🌟 <b>Số vị trí an vị Cổ Vật tăng lên:</b> <color=#ffdd22>{0} ô</color>!");
        Add("catgod_desc_format",
            "Feed cards into the gluttonous Cat God's Mouth to consume unwanted items, discard trash, and gamble for chaotic cosmic rewards.\n\n<b>Items Swallowed:</b> <color=#ffdd22>{0}</color>\n<b>Sacrificial Appetite:</b>\n• Appeased Greed: {1} points\n• Appeased Pollution: {2} points",
            "Ném vật phẩm vào Miệng Thần Mèo háu đói để xả rác, giải phóng vật phẩm thừa và đánh cược nhận báu vật hư không ngẫu nhiên.\n\n<b>Lượng vật phẩm đã nuốt:</b> <color=#ffdd22>{0}</color>\n<b>Mức độ nuốt chửng hiện tại:</b>\n• Xoa dịu Tham Lam: {1} điểm\n• Xoa dịu Ô Nhiễm: {2} điểm");

        // Stamina, Exhausted & Attrition System Fallbacks
        Add("stamina_name", "Stamina", "Thể Lực", "体力", "スタミナ", "스태미나");
        Add("exhausted_name", "Exhausted", "Kiệt Sức", "力竭", "疲弊状態", "탈진");
        Add("hoi_quang_burst", "🔥 HỒI QUANG PHẢN CHIẾU!", "🔥 HỒI QUANG PHẢN CHIẾU!", "🔥 回光返照！", "🔥 回光返照！", "🔥 회광반조!");

        // Load external CSV/TSV table if available to support dynamically updated translations
        LoadExternalLocTable();
    }

    private static void Add(string key, string en, string vi, string zh = "", string ja = "", string ko = "")
    {
        string k = key.ToLower();
        _dict[k] = new Dictionary<Language, string> {
            { Language.English, en },
            { Language.Vietnamese, vi },
            { Language.Chinese, string.IsNullOrEmpty(zh) ? en : zh },
            { Language.Japanese, string.IsNullOrEmpty(ja) ? en : ja },
            { Language.Korean, string.IsNullOrEmpty(ko) ? en : ko }
        };
    }

    private static void LoadExternalLocTable()
    {
        try
        {
            // Search locations: StreamingAssets, GameScripts dir, or current directory
            string path = Path.Combine(Application.streamingAssetsPath, "MewtationsLocTable.tsv");
            if (!File.Exists(path))
            {
                path = Path.Combine(Application.dataPath, "Core/Systems/MewtationsLocTable.tsv");
            }
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameScripts/Core/Systems/MewtationsLocTable.tsv");
            }
            if (!File.Exists(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), "GameScripts/Core/Systems/MewtationsLocTable.tsv");
            }

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length > 1)
                {
                    // Only clear dynamic keys to preserve hardcoded structures
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] cols = line.Split('\t');
                        if (cols.Length >= 3)
                        {
                            string key = cols[0].Trim().ToLower();
                            string en = cols[1].Replace("\\n", "\n").Trim();
                            string vi = cols[2].Replace("\\n", "\n").Trim();
                            string zh = cols.Length >= 4 ? cols[3].Replace("\\n", "\n").Trim() : en;
                            string ja = cols.Length >= 5 ? cols[4].Replace("\\n", "\n").Trim() : en;
                            string ko = cols.Length >= 6 ? cols[5].Replace("\\n", "\n").Trim() : en;

                            _dict[key] = new Dictionary<Language, string> {
                                { Language.English, en },
                                { Language.Vietnamese, vi },
                                { Language.Chinese, zh },
                                { Language.Japanese, ja },
                                { Language.Korean, ko }
                            };
                        }
                    }
                    Debug.Log($"[MewtationsLoc] Successfully loaded {lines.Length - 1} translation keys from: {path}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MewtationsLoc] Error loading external TSV translation table: {ex}");
        }
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

    // Support parameterized formatting directly in translation lookup
    public static string TranslateFormat(string key, string defaultText, params object[] args)
    {
        string text = Translate(key, defaultText);
        try
        {
            return string.Format(text, args);
        }
        catch
        {
            return text;
        }
    }
}
