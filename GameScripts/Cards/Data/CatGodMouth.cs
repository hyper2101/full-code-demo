using System;
using System.Collections.Generic;
using UnityEngine;

public enum CatGodState { Idle, OfferingPull, Consume, RitualComplete, Anger }

public class CatGodMouth : CardData
{
    [Header("Cat God Mouth Settings")]
    public CatGodState CurrentState = CatGodState.Idle;

    [ExtraData("offering_progress")]
    public int OfferingProgress = 0;
    
    [ExtraData("total_blasphemy")]
    public int TotalBlasphemy = 0;

    private bool _hasWarnedBlasphemy = false;
    private bool _hasTriggeredThreat = false;

    public override bool DetermineCanHaveCardsWhenIsRoot => true;
    public override bool UsesHorizontalSlots => true;

    public override void UpdateCard()
    {
        base.UpdateCard();

        if (this.MyGameCard == null) return;

        // Check if there's a RitualCard
        if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is RitualCardData ritualCard)
        {
            ritualCard.IsLockedWhileActive = true;
            this.descriptionOverride = MewtationsLoc.TranslateFormat("catgod_ritual_active",
                "Nghi lễ đang diễn ra...\nTiến độ: {0}/{1}\nBáng bổ: {2}%",
                OfferingProgress, ritualCard.RequiredDevotion, GetBlasphemyPercent(ritualCard));
            
            // Check for offering
            if (ritualCard.MyGameCard.HasChild && !this.MyGameCard.TimerRunning)
            {
                this.MyGameCard.StartTimer(0.25f, new TimerAction(this.ConsumeOffering), MewtationsLoc.Translate("catgod_consume_timer", "Tiếp nhận..."), "offering");
            }
        }
        else
        {
            this.descriptionOverride = MewtationsLoc.Translate("catgod_idle", "Kéo một Thẻ Nghi Lễ (Ritual Card) vào đây để bắt đầu.");
            OfferingProgress = 0;
            TotalBlasphemy = 0;
            _hasWarnedBlasphemy = false;
            _hasTriggeredThreat = false;
        }
    }

    private GameCard GetTopCardInStack(GameCard root)
    {
        GameCard current = root;
        while (current.Child != null)
        {
            current = current.Child;
        }
        return current;
    }

    private void ConsumeOffering()
    {
        if (this.MyGameCard == null || !this.MyGameCard.HasChild) return;
        
        GameCard ritualGameCard = this.MyGameCard.Child;
        if (!(ritualGameCard.CardData is RitualCardData ritualCard)) return;

        if (!ritualGameCard.HasChild) return;

        GameCard topCard = GetTopCardInStack(ritualGameCard);
        if (topCard == ritualGameCard) return;

        CardData offeringData = topCard.CardData;

        // Push invalid cards out
        if (offeringData.MyCardType == CardType.Humans || offeringData is CatCardData || offeringData is RitualCardData)
        {
            topCard.RemoveFromStack();
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            topCard.BounceTarget = topCard.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * 2f;
            return;
        }

        int devotion = offeringData.DevotionValue;
        if (devotion <= 0) devotion = offeringData.Value;
        if (devotion <= 0) devotion = 1;

        int blasphemy = offeringData.BlasphemyValue * devotion;

        OfferingProgress += devotion;
        TotalBlasphemy += blasphemy;

        topCard.DestroyCard(true, true);

        if (AudioManager.me != null && AudioManager.me.Eat != null)
        {
            AudioManager.me.PlaySound2D(AudioManager.me.Eat, UnityEngine.Random.Range(0.85f, 1.15f), 0.5f);
        }

        CheckRitualStatus(ritualCard);
    }

    private int GetBlasphemyPercent(RitualCardData ritualCard)
    {
        if (ritualCard.RequiredDevotion <= 0) return 0;
        return Mathf.RoundToInt(((float)TotalBlasphemy / ritualCard.RequiredDevotion) * 100);
    }

    private void CheckRitualStatus(RitualCardData ritualCard)
    {
        int percent = GetBlasphemyPercent(ritualCard);

        if (percent >= 20 && !_hasWarnedBlasphemy)
        {
            _hasWarnedBlasphemy = true;
            WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, MewtationsLoc.Translate("catgod_warn_blasphemy", "Ngươi dâng thứ ô uế cho ta?"), "", false, 0, 2f, true);
        }

        if (percent >= 40 && !_hasTriggeredThreat)
        {
            _hasTriggeredThreat = true;
            TriggerGodCatThreat(ritualCard);
        }

        if (OfferingProgress >= ritualCard.RequiredDevotion)
        {
            CompleteRitual(ritualCard);
        }
    }

    private void TriggerGodCatThreat(RitualCardData ritualCard)
    {
        if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.5f;

        Vector3 spawnPos = this.transform.position + Vector3.back * 1.5f;
        WorldManager.instance.CreateCard(spawnPos, "mob_void_spirit", true, true, true);

        string title = MewtationsLoc.Translate("catgod_threat_title", "TÀ THẦN PHẪN NỘ!");
        string text = MewtationsLoc.Translate("catgod_threat_desc", "Mùi xác thối và lòng thành giả dối đã làm bẩn nghi lễ! Một kẻ thù Hư Không đã xuất hiện để trừng phạt ngươi!");

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_combat", "Chiến đấu!") }, (idx) => {});
        }
    }

    private void CompleteRitual(RitualCardData ritualCard)
    {
        // Push remaining offerings out of the stack
        GameCard current = ritualCard.MyGameCard.Child;
        while (current != null)
        {
            GameCard next = current.Child;
            current.RemoveFromStack();
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            current.BounceTarget = current.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * 2.5f;
            current = next;
        }

        Vector3 spawnPos = this.transform.position + Vector3.back * 1.5f;
        WorldManager.instance.CreateCard(spawnPos, ritualCard.RewardPackId, true, true, true);

        ritualCard.MyGameCard.DestroyCard(true, true);

        OfferingProgress = 0;
        TotalBlasphemy = 0;
        _hasWarnedBlasphemy = false;
        _hasTriggeredThreat = false;

        string title = MewtationsLoc.Translate("catgod_ritual_complete_title", "NGHI LỄ HOÀN THÀNH");
        string text = MewtationsLoc.Translate("catgod_ritual_complete_desc", "Thần Mèo đã tiếp nhận đủ lễ vật và ban phát phần thưởng!");

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_accept", "Tiếp nhận") }, (idx) => {});
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        if (otherCard is RitualCardData && this.MyGameCard.Child == null) 
            return true;
        return false;
    }
}
