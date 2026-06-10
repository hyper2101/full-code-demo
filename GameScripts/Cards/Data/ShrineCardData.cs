using System;
using System.Collections.Generic;
using UnityEngine;

public class ShrineCardData : CardData
{
	[ExtraData("max_shrine_slots")]
	public int MaxSlots = 2;

	private string _cachedRelicsHash = "";

    // [PHASE 4: SHRINE MIGRATION]
    // Danh sách các Slot tĩnh. Không dùng Stacklands Child chain nữa.
    public List<StructureSlotData> ShrineSlots = new List<StructureSlotData>();

	public override bool UsesHorizontalSlots
	{
		get { return false; } // Không dùng horizontal layout mặc định nữa
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
        RefreshSlotsList();
	}

    public void RefreshSlotsList()
    {
        // Điều chỉnh số lượng Slot vật lý dựa theo MaxSlots.
        while (ShrineSlots.Count < MaxSlots)
        {
            int index = ShrineSlots.Count;
            // Xếp các slot theo hình tròn hoặc lưới xung quanh Shrine
            float angle = index * (Mathf.PI * 2f / (float)MaxSlots);
            if (MaxSlots == 2) angle = index == 0 ? 0 : Mathf.PI;
            
            float radius = 1.5f;
            Vector3 offset = new Vector3(Mathf.Sin(angle) * radius, 0.1f, Mathf.Cos(angle) * radius);

            ShrineSlots.Add(new StructureSlotData
            {
                SlotId = "shrine_slot_" + index,
                LocalOffset = offset,
                OccupancyPolicy = OccupancyPolicy.Single,
                AcceptedTypes = new List<string>() // Rỗng = code sẽ check IsShrineOffering | IsAncientRelic
            });
        }
        
        // Cập nhật lại vị trí các slot nếu có sự thay đổi
        for (int i = 0; i < ShrineSlots.Count; i++)
        {
            float angle = i * (Mathf.PI * 2f / (float)MaxSlots);
            if (MaxSlots == 2) angle = i == 0 ? 0 : Mathf.PI;
            float radius = 1.5f;
            ShrineSlots[i].LocalOffset = new Vector3(Mathf.Sin(angle) * radius, 0.1f, Mathf.Cos(angle) * radius);
        }
    }

	protected override bool CanHaveCard(CardData otherCard)
	{
        // Để StructureAttachmentSystem bắt
		return false;
	}

    // Kiểm tra xem 1 thẻ có thể vào đền không (Gọi bởi AttachmentSystem)
    public bool IsValidOffering(CardData otherCard)
    {
        return otherCard.IsShrineOffering || otherCard.IsAncientRelic;
    }

	public override void UpdateCard()
	{
		base.UpdateCard();

		if (this.MyGameCard != null)
		{
            // Nam châm giữ các thẻ trong slot
            foreach (var slot in ShrineSlots)
            {
                if (slot.SlotOccupants.Count > 0)
                {
                    GameCard card = slot.SlotOccupants[0];
                    if (card != null && !card.Destroyed)
                    {
                        Vector3 targetPos = transform.position + slot.LocalOffset;
                        card.transform.position = Vector3.Lerp(card.transform.position, targetPos, Time.deltaTime * 10f);
                    }
                }
            }

			// 1. Kiểm tra sự thay đổi của các cổ vật trong Shrine (Event-driven)
			string currentHash = GetRelicsHash();
			if (currentHash != _cachedRelicsHash)
			{
				_cachedRelicsHash = currentHash;
				EventBus.Publish(new OnShrineStackChangedEvent(this));
			}

			// 2. Cập nhật mô tả thẻ theo số slot hiện tại
			this.descriptionOverride = MewtationsLoc.TranslateFormat("shrine_desc_format", 
				"Trận Pháp Điện Thờ Thần Mèo. Nơi an vị Cổ Vật cổ đại để kích hoạt Trận Pháp Tự Động Hóa và cộng hưởng Đạo Pháp.\n\n• <b>Số vị trí an vị Cổ Vật tối đa:</b> <color=#ffdd22>{0}</color>\n• Đặt <b>Linh Bảo Cộng Hưởng</b> vào để khai mở vị trí hiển thị Cổ Vật vĩnh viễn.", 
				MaxSlots);
 
			// 3. Quản lý timer dâng nạp Linh Bảo Cộng Hưởng (Resonance Trophy)
            GameCard trophyToConsume = GetResonanceTrophyInSlots();
			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "upgrade_shrine")
			{
				if (trophyToConsume == null)
				{
					this.MyGameCard.CancelTimer("upgrade_shrine");
				}
			}
			else if (!this.MyGameCard.TimerRunning && trophyToConsume != null)
			{
				this.MyGameCard.StartTimer(15.0f, new TimerAction(this.UpgradeShrineSlots), MewtationsLoc.Translate("shrine_upgrading", "Đang cộng hưởng năng lượng Điện Thờ..."), "upgrade_shrine", true, false, false);
			}
		}
	}

	private string GetRelicsHash()
	{
		if (this.MyGameCard == null) return "";
		string hash = "";
        foreach (var slot in ShrineSlots)
        {
            if (slot.SlotOccupants.Count > 0)
            {
                GameCard curr = slot.SlotOccupants[0];
                if (curr != null && !curr.Destroyed && curr.CardData != null && curr.CardData.IsAncientRelic)
                {
                    hash += curr.CardData.Id + ",";
                }
            }
        }
		return hash;
	}

	private GameCard GetResonanceTrophyInSlots()
	{
		if (this.MyGameCard == null) return null;
        foreach (var slot in ShrineSlots)
        {
            if (slot.SlotOccupants.Count > 0)
            {
                GameCard curr = slot.SlotOccupants[0];
                if (curr != null && !curr.Destroyed && curr.CardData != null && curr.CardData.IsShrineOffering)
                {
                    return curr;
                }
            }
        }
		return null;
	}

	[TimedAction("upgrade_shrine")]
	public void UpgradeShrineSlots()
	{
		if (this.MyGameCard == null) return;
 
		// Tiêu hủy 1 thẻ Linh Bảo Cộng Hưởng trong slot
		GameCard trophy = GetResonanceTrophyInSlots();
        if (trophy != null)
        {
            // Remove from slot
            foreach (var slot in ShrineSlots)
            {
                if (slot.SlotOccupants.Contains(trophy))
                {
                    slot.SlotOccupants.Remove(trophy);
                    break;
                }
            }

            trophy.DestroyCard(true, true);
            
			// Tăng slot đền thờ
			MaxSlots++;
            RefreshSlotsList();
 
			// Bắn sự kiện thay đổi
			EventBus.Publish(new OnShrineStackChangedEvent(this));
 
			string title = MewtationsLoc.Translate("shrine_upgraded_title", "☯️ TRẬN PHÁP KHAI MỞ!");
			string text = MewtationsLoc.TranslateFormat("shrine_upgraded_desc", 
				"Linh Bảo khai quang thành công! Luồng linh khí tinh khiết bùng phát từ Điện Thờ, mở rộng phạm vi Trận Pháp.\n🌟 <b>Số vị trí an vị Cổ Vật tăng lên:</b> <color=#ffdd22>{0} ô</color>!", 
				MaxSlots);
 
			if (Mewtations.Dialogue.DialogueSystem.Instance != null)
			{
				Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_wonderful", "Tuyệt vời!") }, (cIdx) => {});
			}
        }
	}

	public static bool IsRelicActiveInShrine(string relicId)
	{
		if (WorldManager.instance == null) return false;
		var shrines = WorldManager.instance.BoardQuery.GetVisibleBoardCards().FindAll(c => c != null && c.CardData is ShrineCardData && !c.Destroyed);
		foreach (var shrineCard in shrines)
		{
            ShrineCardData shrine = shrineCard.CardData as ShrineCardData;
            if (shrine == null) continue;

            foreach (var slot in shrine.ShrineSlots)
            {
                if (slot.SlotOccupants.Count > 0)
                {
                    GameCard curr = slot.SlotOccupants[0];
                    if (curr != null && !curr.Destroyed && curr.CardData != null && curr.CardData.Id == relicId)
                    {
                        return true;
                    }
                }
            }
		}
		return false;
	}
}

