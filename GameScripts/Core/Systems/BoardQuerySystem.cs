using System;
using System.Collections.Generic;
using System.Linq;

public class BoardQuerySystem
{
    private WorldManager _world;

    public BoardQuerySystem(WorldManager world)
    {
        _world = world;
    }

    /// <summary>
    /// Lấy tất cả thẻ đang thực sự nằm trên board (Bao gồm thẻ tự do và thẻ trên Stack).
    /// </summary>
    public List<GameCard> GetVisibleBoardCards(GameBoard board = null)
    {
        board = board ?? _world.CurrentBoard;
        List<GameCard> result = new List<GameCard>();
        foreach (GameCard card in _world.AllCards)
        {
            if (card != null && !card.Destroyed && card.MyBoard == board)
            {
                // Khi AttachmentSystem hoàn thiện, sẽ filter thêm những card bị "ẩn" trong rương.
                result.Add(card);
            }
        }
        return result;
    }

    /// <summary>
    /// Lấy tất cả thẻ mà người chơi có thể thao tác được.
    /// </summary>
    public List<GameCard> GetInteractableCards(GameBoard board = null)
    {
        board = board ?? _world.CurrentBoard;
        return GetVisibleBoardCards(board).Where(c => 
            // Tương lai: Kiểm tra InteractionLockState != HardLocked
            true
        ).ToList();
    }

    /// <summary>
    /// Lấy thẻ rớt tự do (Không cha, không con, không bị kéo).
    /// </summary>
    public List<GameCard> GetLooseCards(GameBoard board = null)
    {
        board = board ?? _world.CurrentBoard;
        return GetVisibleBoardCards(board).Where(c => 
            c.Parent == null && c.Child == null && !c.BeingDragged && !c.HasStructureParent()
        ).ToList();
    }

    /// <summary>
    /// Lấy tất cả các thẻ đang được cắm trong một Structure.
    /// </summary>
    public List<GameCard> GetCardsInStructure(GameCard structureCard)
    {
        List<GameCard> result = new List<GameCard>();
        if (structureCard == null || structureCard.Destroyed) return result;

        if (structureCard.CardData is IStructureContainer container)
        {
            foreach (var slot in container.GetAllSlots())
            {
                result.AddRange(slot.SlotOccupants);
            }
        }
        return result;
    }

    /// <summary>
    /// Lấy tất cả các thẻ nằm trong một SlotId cụ thể của một Structure.
    /// </summary>
    public List<GameCard> GetCardsInSlot(GameCard structureCard, string slotId)
    {
        List<GameCard> result = new List<GameCard>();
        if (structureCard == null || structureCard.Destroyed) return result;

        if (structureCard.CardData is IStructureContainer container)
        {
            StructureSlotData slot = container.GetSlotById(slotId);
            if (slot != null)
            {
                result.AddRange(slot.SlotOccupants);
            }
        }
        return result;
    }
}
