using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE ON PERFORMANCE TECHNICAL DEBT:
// Current scanning is interval-based (every 1.0s) which is perfectly fine for prototype.
// For final Steam release with thousands of cards, this should be refactored into:
// 1. Event-driven triggers only when stacks change (OnStackChanged / OnParentChanged hooks).
// 2. Spatial partitioning / buckets for active cards.

public class ResourceCompressionSystem : MonoBehaviour
{
    public static ResourceCompressionSystem Instance { get; private set; }

    public int CompressionRatio = 10;
    public float CompressionDuration = 2.5f;
    public int MaxCompressionPerTick = 3;

    private readonly HashSet<string> _compressibleBaseIds = new HashSet<string>
    {
        "linh_luc", "linh_thach", "yeu_dan", "di_bien_chat", "linh_thuc", "linh_duoc"
    };

    private bool IsCompressible(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLower();
        
        string baseId = lower;
        if (lower.EndsWith("_pham")) baseId = lower.Substring(0, lower.Length - 5);
        else if (lower.EndsWith("_dia")) baseId = lower.Substring(0, lower.Length - 4);
        else if (lower.EndsWith("_thien")) baseId = lower.Substring(0, lower.Length - 6);
        else return false;

        return _compressibleBaseIds.Contains(baseId) || baseId.Contains("linh_luc") || baseId.Contains("linh_thach");
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(ScanAndCompressRoutine());
    }

    private IEnumerator ScanAndCompressRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f); // Tối ưu hiệu năng: Quét interval định kỳ 1 giây

            if (WorldManager.instance == null || WorldManager.instance.AllCards == null) continue;

            // Quét toàn bộ root card trên bàn chơi hiện tại
            var rootCards = new List<GameCard>();
            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null && !gc.HasParent)
                {
                    rootCards.Add(gc);
                }
            }

            int compressedThisTick = 0;

            foreach (var rootCard in rootCards)
            {
                if (rootCard == null || rootCard.Destroyed) continue;

                CardData cardData = rootCard.CardData;
                string currentId = cardData.Id;
                if (string.IsNullOrEmpty(currentId)) continue;

                // Xác định xem tài nguyên có nằm trong danh sách nén hay không (Whitelisted)
                bool canCompress = IsCompressible(currentId);
                if (!canCompress)
                {
                    // Tự động tắt timer nén nếu trước đó có chạy và nay không còn thỏa mãn
                    if (rootCard.TimerRunning && rootCard.TimerActionId == "resource_compression")
                    {
                        rootCard.CancelTimer("resource_compression");
                    }
                    continue;
                }

                // Đếm số lượng tài nguyên cùng ID trong stack
                List<GameCard> stack = rootCard.GetAllCardsInStack();
                int identicalCount = 0;
                foreach (var gc in stack)
                {
                    if (gc != null && gc.CardData != null && gc.CardData.Id == currentId)
                    {
                        identicalCount++;
                    }
                }

                // Nếu có đủ số lượng nén (CompressionRatio)
                if (identicalCount >= CompressionRatio)
                {
                    if (!rootCard.TimerRunning)
                    {
                        if (compressedThisTick >= MaxCompressionPerTick) continue; // Giới hạn số lượng nén mỗi Tick để tránh spike frame
                        compressedThisTick++;

                        string nextTier = "";
                        string tierName = "";
                        if (currentId.EndsWith("_pham"))
                        {
                            nextTier = currentId.Replace("_pham", "_dia");
                            tierName = "Địa cấp";
                        }
                        else if (currentId.EndsWith("_dia"))
                        {
                            nextTier = currentId.Replace("_dia", "_thien");
                            tierName = "Thiên cấp";
                        }
                        else if (currentId.EndsWith("_thien"))
                        {
                            nextTier = currentId.Replace("_thien", "_tien");
                            tierName = "Tiên cấp";
                        }

                        string targetTier = nextTier;
                        string targetTierName = tierName;

                        rootCard.StartTimer(CompressionDuration, new TimerAction(() => {
                            ExecuteCompression(rootCard, targetTier, targetTierName);
                        }), $"Đang ngưng tụ lên {tierName}...", "resource_compression");
                    }
                }
                else
                {
                    // Hủy nén nếu người chơi kéo bớt thẻ ra làm số lượng không đủ 10
                    if (rootCard.TimerRunning && rootCard.TimerActionId == "resource_compression")
                    {
                        rootCard.CancelTimer("resource_compression");
                    }
                }
            }
        }
    }

    private void ExecuteCompression(GameCard rootCard, string nextTierId, string tierName)
    {
        if (rootCard == null || rootCard.Destroyed || rootCard.CardData == null) return;

        string currentId = rootCard.CardData.Id;
        List<GameCard> stack = rootCard.GetAllCardsInStack();
        List<GameCard> toDestroy = new List<GameCard>();
        int count = 0;

        foreach (var gc in stack)
        {
            if (gc != null && gc.CardData != null && gc.CardData.Id == currentId && count < CompressionRatio)
            {
                toDestroy.Add(gc);
                count++;
            }
        }

        if (count == CompressionRatio)
        {
            Vector3 spawnPos = rootCard.transform.position;

            // Tiêu hủy 10 thẻ nguyên liệu cấp thấp
            foreach (var gc in toDestroy)
            {
                if (gc != null && !gc.Destroyed)
                {
                    gc.DestroyCard(true, true);
                }
            }

            // Sinh tài nguyên mới ngưng tụ ở phẩm cấp cao hơn
            GameCard newCard = WorldManager.instance.CreateCard(spawnPos, nextTierId, true, true, true);
            if (newCard != null)
            {
                string msg = $"✨ [NGƯNG TỤ THÀNH CÔNG] Ngưng tụ {CompressionRatio} tài nguyên Phàm/Địa thành 1 {newCard.CardData.Name} ({tierName})!";
                WorldManager.instance.CreateFloatingText(newCard, true, 0, msg, "", true, 0, 3.5f, true);
            }
        }
    }
}
