using System;
using System.Collections.Generic;
using UnityEngine;

public class ShrineCardData : CardData
{
	[ExtraData("max_shrine_slots")]
	public int MaxSlots = 2;

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
		while (curr != null)
		{
			childCount++;
			curr = curr.Child;
		}

		// Giới hạn số slot của Đền Thờ
		if (childCount >= MaxSlots) return false;

		// Đền Thờ chỉ chấp nhận thẻ Cống Phẩm và Cổ Vật thông qua các thuộc tính generic của CardData
		return otherCard.IsShrineOffering || otherCard.IsAncientRelic;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();

		if (this.MyGameCard != null)
		{
			// 1. Cập nhật mô tả thẻ theo số slot hiện tại
			this.descriptionOverride = "Đền Thờ Thần Mèo cổ kính. Nơi trang bị Cổ Vật và hiến tế Cống Phẩm để mở khóa sức mạnh tự động hóa.\n\n" +
			                           $"• <b>Số ô Cổ Vật tối đa:</b> <color=#ffdd22>{MaxSlots}</color>\n" +
			                           $"• Đặt thẻ <b>Cống Phẩm</b> vào ô để mở rộng thêm ô Đền Thờ vĩnh viễn.";

			// 2. Quản lý timer dâng nạp Cống Phẩm
			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "upgrade_shrine")
			{
				if (!HasOfferingInStack())
				{
					this.MyGameCard.CancelTimer("upgrade_shrine");
				}
			}
			else if (!this.MyGameCard.TimerRunning && HasOfferingInStack())
			{
				// Chạy tiến trình tiêu thụ cống phẩm để tăng slot
				this.MyGameCard.StartTimer(15.0f, new TimerAction(this.UpgradeShrineSlots), "Đang dâng nạp Cống Phẩm lên Đền Thờ...", "upgrade_shrine", true, false, false);
			}
		}
	}

	private bool HasOfferingInStack()
	{
		if (this.MyGameCard == null) return false;
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			if (curr.CardData != null && curr.CardData.IsShrineOffering)
			{
				return true;
			}
			curr = curr.Child;
		}
		return false;
	}

	[TimedAction("upgrade_shrine")]
	public void UpgradeShrineSlots()
	{
		if (this.MyGameCard == null) return;

		// Tiêu hủy 1 thẻ cống phẩm trong stack
		GameCard curr = this.MyGameCard.Child;
		bool foundAndDestroyed = false;
		while (curr != null)
		{
			if (curr.CardData != null && curr.CardData.IsShrineOffering && !curr.Destroyed)
			{
				curr.DestroyCard(true, true);
				foundAndDestroyed = true;
				break;
			}
			curr = curr.Child;
		}

		if (foundAndDestroyed)
		{
			// Tăng slot đền thờ
			MaxSlots++;

			string title = "☯️ ĐỀN THỜ THĂNG CẤP!";
			string text = $"Thần Mèo đã tiếp nhận Cống Phẩm thành kính của bạn!\n\n" +
			              $"Linh quang rực rỡ bùng phát từ đền thờ, mở rộng không gian trận pháp.\n" +
			              $"🌟 <b>Số ô Cổ Vật tối đa tăng lên:</b> <color=#ffdd22>{MaxSlots} ô</color>!";

			if (Mewtations.Dialogue.DialogueSystem.Instance != null)
			{
				Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tuyệt vời!" }, (cIdx) => {});
			}
		}
	}
}
