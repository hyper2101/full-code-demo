using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Expedition
{
    public enum NodeType
    {
        Combat,     // Standard fight
        Resource,   // Resource gathering
        Event,      // Dialogue events
        Ruins,      // Random encounters / mini puzzles
        Lore,       // Unlocking lore cards
        Boss,       // Final progressive boss fight
        Altar,      // Cat God's Altar node for sacrifice & purification
        Elite,      // Harder elite fight with special rewards
        Extraction, // Portal node allowing safe retreat and taking all loot home
        SafeRetreat // Rest node allowing escape with no penalty
    }

    public enum ExpeditionBiome
    {
        Forest,     // Cổ Lộ Rừng Thiêng
        Swamp,      // Đầm Lầy Độc Lực
        Peak,       // Đỉnh Lôi Kiếp
        Abyss       // Hầm Ngục Vô Tận
    }

    public enum NodeState
    {
        Locked,
        Available,
        Visited
    }

    public enum ExpeditionState
    {
        Idle,
        PartySelection,
        MapNavigation,
        InEncounter,
        LootReward,
        EndRun
    }

    [Serializable]
    public class ExpeditionRunState
    {
        public int GreedLevel = 0;        // 0 to 100
        public int CorruptionLevel = 0;   // 0 to 100
        public int CurrentLayer = 0;
        public int TotalGoldCollected = 0;
        public List<string> RunActiveMutations = new List<string>();
        
        public int BaseAppeasementGreed = 0;
        public int BaseAppeasementCorruption = 0;
        public string EquippedRelicId = ""; // Lưu trữ ID cổ vật đang trang bị

        public void AddGreed(int val)
        {
            GreedLevel = Mathf.Clamp(GreedLevel + val, 0, 100);
        }

        public void AddCorruption(int val)
        {
            CorruptionLevel = Mathf.Clamp(CorruptionLevel + val, 0, 100);
        }

        public void Clear()
        {
            GreedLevel = 0;
            CorruptionLevel = 0;
            CurrentLayer = 0;
            TotalGoldCollected = 0;
            RunActiveMutations.Clear();
            BaseAppeasementGreed = 0;
            BaseAppeasementCorruption = 0;
            EquippedRelicId = "";
        }
    }

    // Define standard constants for Heavenly Talents (Permanent)
    public static class HeavenlyTalent
    {
        public const string HeavenlyPoisonBody = "HeavenlyPoisonBody";       // Poison status on attack
        public const string DivineShieldProtection = "DivineShieldProtection"; // Start combat with +15 Armor
        public const string RageOvercharger = "RageOvercharger";             // Gain +10 extra Rage on attack
        public const string MartialArtsCleave = "MartialArtsCleave";         // Basic attack patterns become cleaving

        public static string GetDisplayName(string id)
        {
            switch (id)
            {
                case HeavenlyPoisonBody: return "Thần Thể Kịch Độc";
                case DivineShieldProtection: return "Kim Cương Hộ Thể";
                case RageOvercharger: return "Nộ Khí Cuồng Triều";
                case MartialArtsCleave: return "Bá Vương Thương Pháp";
                default: return "Thiên Kiêu Thể";
            }
        }

        public static string GetDescription(string id)
        {
            switch (id)
            {
                case HeavenlyPoisonBody: return "Đòn đánh thường gây trạng thái Trúng Độc dồn cộng thêm.";
                case DivineShieldProtection: return "Bắt đầu mỗi trận chiến nhận ngay 15 Giáp bảo hộ.";
                case RageOvercharger: return "Nhận thêm 10 điểm Nộ khí mỗi lượt hành động.";
                case MartialArtsCleave: return "Biến đổi đòn đánh cơ bản thành tấn công lan (Cleave) hàng ngang.";
                default: return "Sở hữu tố chất tu tiên đặc biệt.";
            }
        }
    }

    // Define standard constants for Dynamic Mutations (Temporary / Unstable)
    public static class UnstableMutation
    {
        public const string UnstableClaws = "UnstableClaws";     // Damage +30% but self HP drain on attack
        public const string LethargicNap = "LethargicNap";       // Speed -15 but HP health recovery on round end
        public const string CursedFur = "CursedFur";             // Armor -5, locks ability to gain shield

        public static string GetDisplayName(string id)
        {
            switch (id)
            {
                case UnstableClaws: return "Trảo Vuốt Bất Ổn";
                case LethargicNap: return "Thần Miêu Ngái Ngủ";
                case CursedFur: return "Nguyền Rủa Lông Tơ";
                case "morale_collapse": return "Đạo Tâm Trì Trệ (Thiếu Upkeep)";
                default: return "Đột Biến Linh Khí";
            }
        }

        public static string GetDescription(string id)
        {
            switch (id)
            {
                case UnstableClaws: return "Tăng 30% sát thương thường nhưng tự tổn hại 2 HP mỗi khi vung vuốt.";
                case LethargicNap: return "Tốc độ hành động bị giảm 15 đơn vị, bù lại hồi 5 HP mỗi khi kết thúc vòng đấu.";
                case CursedFur: return "Giảm 5 Giáp, vô hiệu hóa khả năng nhận lá chắn phòng ngự.";
                case "morale_collapse": return "Tông môn cạn kiệt bổng lộc: Giảm 25% Thần Tốc và Máu tối đa của mèo.";
                default: return "Cơ thể bị biến đổi do nhiễm độc linh lực.";
            }
        }
    }

    [Serializable]
    public class ExpeditionNode
    {
        public int Id;
        public int Layer; // Floor index (0 to MaxLayers)
        public int Position; // Horizontal slot on the layer
        public NodeType Type;
        public NodeState State = NodeState.Locked;
        public List<int> OutgoingConnections = new List<int>(); // List of destination Node IDs
        public RouteTheme Theme = RouteTheme.Standard;
        public ExpeditionBiome Biome = ExpeditionBiome.Forest;

        public ExpeditionNode(int id, int layer, int position, NodeType type)
        {
            Id = id;
            Layer = layer;
            Position = position;
            Type = type;
        }
    }

    [Serializable]
    public class Backpack
    {
        public int MaxCapacity = 10;
        public List<string> ContainedCardIds = new List<string>();

        public Backpack(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
        }

        public bool IsFull => ContainedCardIds.Count >= MaxCapacity;

        public bool AddItem(string cardId)
        {
            if (IsFull) return false;
            ContainedCardIds.Add(cardId);
            return true;
        }

        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < ContainedCardIds.Count)
            {
                ContainedCardIds.RemoveAt(index);
            }
        }

        public void Clear()
        {
            ContainedCardIds.Clear();
        }
    }
}
