using System;
using System.Collections.Generic;
using UnityEngine;

public class BreakthroughArrayCardData : CardData
{
	public override bool UsesHorizontalSlots
	{
		get
		{
			return true;
		}
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.MyGameCard == null) return false;

		// Đếm số lượng card con hiện tại trong stack
		int childCount = 0;
		GameCard curr = this.MyGameCard.Child;
		bool hasCat = false;
		while (curr != null)
		{
			childCount++;
			if (curr.CardData is CatCardData)
			{
				hasCat = true;
			}
			curr = curr.Child;
		}

		// Đột Phá Trận tối đa 4 slot con: 1 Mèo (trung tâm) và tối đa 3 vệ tinh (hỗ trợ)
		if (childCount >= 4) return false;

		// Ô con trực tiếp đầu tiên (trung tâm) bắt buộc phải là Mèo để kích hoạt trận pháp
		if (!hasCat)
		{
			return otherCard is CatCardData;
		}

		// Các ô vệ tinh tiếp theo chỉ nhận vật phẩm hỗ trợ đột phá hợp lệ
		return otherCard.IsBreakthroughSupport;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();

		if (this.MyGameCard != null)
		{
			// Kiểm tra xem Mèo đột phá còn nằm trong stack không
			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "breakthrough_array")
			{
				if (!HasCatInStack())
				{
					this.MyGameCard.CancelTimer("breakthrough_array");
				}
			}
			else if (!this.MyGameCard.TimerRunning && HasCatInStack())
			{
				CatCardData cat = GetCatInStack();
				if (cat != null)
				{
					// Tốc độ chạy trận pháp phụ thuộc vào cấp độ và tốc độ của Mèo
					float duration = Mathf.Max(5f, (10f + cat.BreakthroughLevel * 3f) - (cat.Speed * 0.03f));
					this.MyGameCard.StartTimer(duration, new TimerAction(this.CompleteBreakthroughProcess), "Trận pháp tụ linh đột phá...", "breakthrough_array", true, false, false);
				}
			}
		}
	}

	private bool HasCatInStack()
	{
		return GetCatInStack() != null;
	}

	private CatCardData GetCatInStack()
	{
		if (this.MyGameCard == null) return null;
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			if (curr.CardData is CatCardData cat)
			{
				return cat;
			}
			curr = curr.Child;
		}
		return null;
	}

	[TimedAction("breakthrough_array")]
	public void CompleteBreakthroughProcess()
	{
		if (this.MyGameCard == null) return;

		CatCardData cat = GetCatInStack();
		if (cat == null) return;

		// Check for True Harmony Ceremony (3 hint cards + Breakthrough 4 cat)
		int hintCount = 0;
		List<GameCard> hintCards = new List<GameCard>();
		GameCard currCard = this.MyGameCard.Child;
		while (currCard != null)
		{
			if (currCard.CardData != null && currCard.CardData.Id != null && currCard.CardData.Id.ToLower().Contains("item_secret_lore_hint"))
			{
				hintCount++;
				hintCards.Add(currCard);
			}
			currCard = currCard.Child;
		}

		if (hintCount >= 3 && cat.BreakthroughLevel >= 4)
		{
			// Destroy the 3 hints
			foreach (var hc in hintCards)
			{
				if (hc != null && !hc.Destroyed)
				{
					hc.DestroyCard(true, true);
				}
			}

			// Unlock True Harmony Covenant!
			cat.ClearMutations();
			cat.PermanentScarsString = ""; // Clears all permanent scars!
			cat.AddTrait("talent_true_harmony");
			
			// Auto heal to full
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

			Debug.Log($"[TrueHarmony] {cat.Name} has achieved absolute Faction Enlightenment and unlocked True Harmony!");
			return;
		}

		// 1. Phân tích các vật phẩm hỗ trợ đột phá trong stack sử dụng thuộc tính generic của CardData
		float damageReduction = 0f;
		int healthBonus = 0;
		bool hasRevivePill = false;

		List<GameCard> cardsToDestroy = new List<GameCard>();
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			CardData data = curr.CardData;
			if (data != null && data.IsBreakthroughSupport)
			{
				cardsToDestroy.Add(curr);

				damageReduction += data.BreakthroughDmgReduction;
				healthBonus += data.BreakthroughHealthBonus;
				if (data.BreakthroughReviveEffect)
				{
					hasRevivePill = true;
				}
			}
			curr = curr.Child;
		}

		// Giới hạn giảm sát thương tối đa là 90%
		damageReduction = Mathf.Min(damageReduction, 0.90f);

		// 2. Tiêu hủy các vật phẩm phụ trợ (dùng 1 lần)
		foreach (GameCard gc in cardsToDestroy)
		{
			if (gc != null && !gc.Destroyed)
			{
				gc.DestroyCard(true, true);
			}
		}

		// 3. Tiến hành kích hoạt lôi kiếp đột phá trên Mèo
		int targetLevel = cat.BreakthroughLevel + 1;
		cat.PerformBreakthroughInArray(targetLevel, damageReduction, healthBonus, hasRevivePill);
	}
}
