using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardIntegrityValidator
{
    private WorldManager _world;

    public BoardIntegrityValidator(WorldManager world)
    {
        _world = world;
    }

    public void ValidateAndRepair()
    {
        Debug.Log("[BoardIntegrityValidator] Bắt đầu quét kiểm tra tính toàn vẹn của ván bài...");
        
        List<GameCard> toRepair = new List<GameCard>();

        foreach (GameCard card in _world.AllCards)
        {
            if (card == null || card.Destroyed) continue;

            // 1. Check mồ côi (Orphan) - Không thuộc board nào
            if (card.MyBoard == null)
            {
                Debug.LogWarning($"[BoardIntegrityValidator] Tìm thấy thẻ mồ côi (không thuộc board): {card.CardData?.Id}. Đưa về Board chính.");
                card.MyBoard = _world.CurrentBoard; // Tạm thời đưa về board hiện tại
                toRepair.Add(card);
            }

            // 2. Check Circular Parent
            if (HasCircularParent(card))
            {
                Debug.LogWarning($"[BoardIntegrityValidator] Phát hiện vòng lặp cha-con ở thẻ: {card.CardData?.Id}. Phá vỡ vòng lặp.");
                card.SetParent(null); // Gỡ bỏ parent để phá vòng lặp
                toRepair.Add(card);
            }

            // 3. Check StructureSlot sai lệch
            // TODO: Khi GameCard có OwnershipData thật, chúng ta sẽ kiểm tra xem Parent có thực sự tồn tại SlotId này không.
        }

        Debug.Log($"[BoardIntegrityValidator] Đã quét xong. Tổng số thẻ được repair: {toRepair.Count}");
    }

    private bool HasCircularParent(GameCard startCard)
    {
        GameCard current = startCard.Parent;
        int depth = 0;
        int maxDepth = 1000; // Ngăn loop vô tận nếu có lỗi thật

        while (current != null)
        {
            if (current == startCard) return true;
            current = current.Parent;
            
            depth++;
            if (depth > maxDepth)
            {
                Debug.LogError("[BoardIntegrityValidator] Quá giới hạn độ sâu cây phân cấp cha-con! Có thể có lỗi nghiêm trọng.");
                return true; // Giả định là lỗi circular để cắt đứt
            }
        }
        return false;
    }
}
