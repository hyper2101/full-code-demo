using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Core; // Assuming we use StructureSlotType if needed, or just strings

public class BreakthroughArrayCardData : CardData, IStructureContainer
{
    // [PHASE 4: BREAKTHROUGH ARRAY MIGRATION]
    public StructureSlotData CenterSlot;
    public StructureSlotData CatalystSlot;
    public List<StructureSlotData> SupportSlots = new List<StructureSlotData>();

	public override bool UsesHorizontalSlots
	{
		get { return false; }
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get { return true; }
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

    protected override void Awake()
    {
        base.Awake();
        
        // Define Physical Slots
        CenterSlot = new StructureSlotData
        {
            SlotId = "center_cat",
            LocalOffset = new Vector3(0, 0.1f, 0f),
            OccupancyPolicy = OccupancyPolicy.Single
        };

        CatalystSlot = new StructureSlotData
        {
            SlotId = "catalyst_pill",
            LocalOffset = new Vector3(0, 0.1f, 1.5f), // Ở phía trên Mèo
            OccupancyPolicy = OccupancyPolicy.Single
        };

        // Support slots ở bên trái và phải
        SupportSlots.Add(new StructureSlotData
        {
            SlotId = "support_1",
            LocalOffset = new Vector3(-1.5f, 0.1f, 0f),
            OccupancyPolicy = OccupancyPolicy.Single
        });

        SupportSlots.Add(new StructureSlotData
        {
            SlotId = "support_2",
            LocalOffset = new Vector3(1.5f, 0.1f, 0f),
            OccupancyPolicy = OccupancyPolicy.Single
        });
    }

    // Attachment System check
    public string GetValidSlotFor(CardData otherCard)
    {
        if (otherCard is CatCardData && CenterSlot.SlotOccupants.Count == 0) return CenterSlot.SlotId;
        if (otherCard.IsCultivationPill && CatalystSlot.SlotOccupants.Count == 0) return CatalystSlot.SlotId;
        
        if (otherCard.IsBreakthroughSupport || (otherCard.Id != null && otherCard.Id.ToLower().Contains("item_secret_lore_hint")))
        {
            foreach (var slot in SupportSlots)
            {
                if (slot.SlotOccupants.Count == 0) return slot.SlotId;
            }
        }
        return null;
    }

    public StructureSlotData GetSlotById(string slotId)
    {
        if (CenterSlot.SlotId == slotId) return CenterSlot;
        if (CatalystSlot.SlotId == slotId) return CatalystSlot;
        foreach (var slot in SupportSlots)
        {
            if (slot.SlotId == slotId) return slot;
        }
        return null;
    }

    public IEnumerable<StructureSlotData> GetAllSlots()
    {
        yield return CenterSlot;
        yield return CatalystSlot;
        foreach (var slot in SupportSlots) yield return slot;
    }

    public void OnCardAttached(GameCard childCard, string slotId) { }
    public void OnCardDetached(GameCard childCard, string slotId) { }

	protected override bool CanHaveCard(CardData otherCard)
	{
        // Vô hiệu hóa stack gốc, ép vào Attachment System
		return false;
	}

	private int _lastActiveCount = -1;

	public override void UpdateCard()
	{
		base.UpdateCard();

		if (this.MyGameCard != null)
		{

            int currentOccupied = 0;
            foreach (var slot in GetAllSlots())
            {
                if (slot.SlotOccupants.Count > 0) currentOccupied++;
            }

			CatCardData cat = GetCatInSlot();

			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("breakthrough_array"))
			{
				if (cat == null)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("breakthrough_array"));
					_lastActiveCount = -1;
				}
				else if (currentOccupied != _lastActiveCount)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("breakthrough_array"));
					_lastActiveCount = currentOccupied;
				}
			}
			else if (!this.MyGameCard.TimerRunning && cat != null)
			{
				_lastActiveCount = currentOccupied;
				float duration = Mathf.Max(5f, (10f + cat.BreakthroughLevel * 3f) - (cat.Speed * 0.03f));
				this.MyGameCard.StartTimer(duration, new TimerAction(this.CompleteBreakthroughProcess), "Trận pháp tụ linh đột phá...", base.GetActionId("breakthrough_array"), true, false, false);
			}
		}
	}

	private CatCardData GetCatInSlot()
	{
		if (CenterSlot.SlotOccupants.Count > 0)
        {
            GameCard catCard = CenterSlot.SlotOccupants[0];
            if (catCard != null && !catCard.Destroyed)
            {
                return catCard.CardData as CatCardData;
            }
        }
		return null;
	}

	[TimedAction("breakthrough_array")]
	public void CompleteBreakthroughProcess()
	{
		if (this.MyGameCard == null) return;

		CatCardData cat = GetCatInSlot();
		if (cat == null) return;

		int hintCount = 0;
		List<GameCard> hintCards = new List<GameCard>();

        // Quét các slot support xem có hint không
        foreach (var slot in SupportSlots)
        {
            if (slot.SlotOccupants.Count > 0)
            {
                GameCard c = slot.SlotOccupants[0];
                if (c != null && !c.Destroyed && c.CardData != null && c.CardData.Id != null && c.CardData.Id.ToLower().Contains("item_secret_lore_hint"))
                {
                    hintCount++;
                    hintCards.Add(c);
                }
            }
        }

		if (hintCount >= 3 && cat.BreakthroughLevel >= 4)
		{
			foreach (var hc in hintCards)
			{
                // Dọn slot trước khi destroy
                foreach (var s in SupportSlots) s.SlotOccupants.Remove(hc);
				hc.DestroyCard(true, true);
			}

			cat.ClearMutations();
			cat.PermanentScarsString = ""; 
			cat.AddTrait("talent_true_harmony");
			cat.HealthPoints = cat.ProcessedCombatStats.MaxHealth;

			string title = MewtationsLoc.Translate("talent_true_harmony_name", "True Harmony Covenant");
			string desc = MewtationsLoc.Translate("talent_true_harmony_desc");

			if (Mewtations.Dialogue.DialogueSystem.Instance != null)
			{
				Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(
					"☯️ " + title + " ☯️",
					"<b>" + desc + "</b>\n\n" + MewtationsLoc.Translate("hint_3_body"),
					new List<string> { MewtationsLoc.Translate("btn_close", "Close") },
					(idx) => {}
				);
			}
			return;
		}

		float damageReduction = 0f;
		int healthBonus = 0;
		bool hasRevivePill = false;
		string pillToInsert = null;

		List<GameCard> cardsToDestroy = new List<GameCard>();
        List<StructureSlotData> slotsToClear = new List<StructureSlotData>();

        // Quét Catalyst (Thuốc)
        if (CatalystSlot.SlotOccupants.Count > 0)
        {
            GameCard pillCard = CatalystSlot.SlotOccupants[0];
            if (pillCard != null && !pillCard.Destroyed && pillCard.CardData != null && pillCard.CardData.IsCultivationPill)
            {
                pillToInsert = pillCard.CardData.Id;
                cardsToDestroy.Add(pillCard);
                slotsToClear.Add(CatalystSlot);
            }
        }

        // Quét Support
        foreach (var slot in SupportSlots)
        {
            if (slot.SlotOccupants.Count > 0)
            {
                GameCard supp = slot.SlotOccupants[0];
                if (supp != null && !supp.Destroyed && supp.CardData != null && supp.CardData.IsBreakthroughSupport)
                {
                    cardsToDestroy.Add(supp);
                    slotsToClear.Add(slot);
                    damageReduction += supp.CardData.BreakthroughDmgReduction;
                    healthBonus += supp.CardData.BreakthroughHealthBonus;
                    if (supp.CardData.BreakthroughReviveEffect) hasRevivePill = true;
                }
            }
        }

		damageReduction = Mathf.Min(damageReduction, 0.90f);

        // Clear slots and destroy
        for (int i = 0; i < cardsToDestroy.Count; i++)
        {
            slotsToClear[i].SlotOccupants.Remove(cardsToDestroy[i]);
            cardsToDestroy[i].DestroyCard(true, true);
        }

		if (!string.IsNullOrEmpty(pillToInsert))
		{
			cat.CultivationData.InsertPill(pillToInsert);
			cat.SaveCultivationData();
			cat.UpdateCardText();
		}

		int targetLevel = cat.BreakthroughLevel + 1;
		cat.PerformBreakthroughInArray(targetLevel, damageReduction, healthBonus, hasRevivePill);
	}
}
