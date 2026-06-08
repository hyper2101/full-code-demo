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

    [ExtraData("has_warned_blasphemy")]
    public bool HasWarnedBlasphemy = false;
    
    [ExtraData("has_triggered_threat")]
    public bool HasTriggeredThreat = false;

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
            HasWarnedBlasphemy = false;
            HasTriggeredThreat = false;
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

        if (percent >= 20 && !HasWarnedBlasphemy)
        {
            HasWarnedBlasphemy = true;
            WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, MewtationsLoc.Translate("catgod_warn_blasphemy", "Ngươi dâng thứ ô uế cho ta?"), "", false, 0, 2f, true);
        }

        bool threatTriggeredNow = false;
        if (percent >= 40 && !HasTriggeredThreat)
        {
            HasTriggeredThreat = true;
            threatTriggeredNow = true;
            
            if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.5f;

            // Spawn quái vật lệch sang bên trái để tránh đè lên rương
            Vector3 threatSpawnPos = this.transform.position + Vector3.left * 1.5f + Vector3.back * 1.5f;
            
            // Tích hợp hệ thống Encounter/Threat (dùng tạm pool của Dog Tax)
            int encounterId = -1;
            if (Mewtations.Combat.Core.EncounterManager.Instance != null)
            {
                var template = UnityEngine.Resources.Load<Mewtations.Combat.Encounters.EncounterTemplateSO>("Encounters/DogTaxEncounter");
                Mewtations.Combat.Encounters.EncounterData newEncounter;
                if (template != null)
                {
                    newEncounter = Mewtations.Combat.Encounters.EncounterGenerator.Generate(template, UnityEngine.Random.Range(0, 99999), 1);
                    newEncounter.EncounterName = "Sự nổi giận của Mèo Thần";
                    newEncounter.TurnLimit = 30;
                }
                else
                {
                    newEncounter = new Mewtations.Combat.Encounters.EncounterData
                    {
                        EncounterName = "Sự nổi giận của Mèo Thần",
                        Context = Mewtations.Combat.Encounters.EncounterContext.DogTax,
                        TurnLimit = 30
                    };
                }
                encounterId = Mewtations.Combat.Core.EncounterManager.Instance.RegisterEncounter(newEncounter);
            }

            if (encounterId != -1)
            {
                int currentMonth = WorldManager.instance != null ? WorldManager.instance.CurrentMonth : 0;
                var threatInstance = new GameScripts.Systems.Threat.ThreatInstance(null, GameScripts.Systems.Threat.ThreatSourceType.Event)
                {
                    CurrentSeverity = GameScripts.Systems.Threat.Severity.Normal,
                    ThreatExpiryMonth = currentMonth + 5,
                    EncounterId = encounterId
                };
                
                // ThreatInstance doesn't have State setter in constructor, setting it to Active so component can work
                threatInstance.State = GameScripts.Systems.Threat.ThreatState.Active;

                GameCard spawnedCard = WorldManager.instance.CreateCard(threatSpawnPos, "dogtax_threat", true, true, true);
                if (spawnedCard != null)
                {
                    var threatComp = spawnedCard.gameObject.GetComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                    if (threatComp == null) threatComp = spawnedCard.gameObject.AddComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                    threatComp.Initialize(threatInstance);
                }
            }
        }

        if (OfferingProgress >= ritualCard.RequiredDevotion)
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

            // Spawn phần thưởng lệch sang bên phải để tránh đè lên quái vật
            Vector3 rewardSpawnPos = this.transform.position + Vector3.right * 1.5f + Vector3.back * 1.5f;
            WorldManager.instance.CreateCard(rewardSpawnPos, ritualCard.RewardPackId, true, true, true);

            ritualCard.MyGameCard.DestroyCard(true, true);

            OfferingProgress = 0;
            TotalBlasphemy = 0;
            HasWarnedBlasphemy = false;
            HasTriggeredThreat = false;

            if (threatTriggeredNow)
            {
                string title = MewtationsLoc.Translate("catgod_ritual_threat_complete_title", "TÀ THẦN NỔI GIẬN NHƯNG VẪN BAN PHƯỚC");
                string text = MewtationsLoc.Translate("catgod_ritual_threat_complete_desc", "Thần Mèo đã tiếp nhận đủ lễ vật nhưng nổi giận vì ô uế! Ngài ném lại phần thưởng nhưng gọi thêm quái vật trừng phạt!");
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.QueueDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_combat_accept", "Tiếp nhận hình phạt") }, (idx) => {});
                }
            }
            else
            {
                string title = MewtationsLoc.Translate("catgod_ritual_complete_title", "NGHI LỄ HOÀN THÀNH");
                string text = MewtationsLoc.Translate("catgod_ritual_complete_desc", "Thần Mèo đã tiếp nhận đủ lễ vật và ban phát phần thưởng!");
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.QueueDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_accept", "Tiếp nhận") }, (idx) => {});
                }
            }
        }
        else if (threatTriggeredNow)
        {
            string title = MewtationsLoc.Translate("catgod_threat_title", "TÀ THẦN PHẪN NỘ!");
            string text = MewtationsLoc.Translate("catgod_threat_desc", "Mùi xác thối và lòng thành giả dối đã làm bẩn nghi lễ! Một kẻ thù Hư Không đã xuất hiện để trừng phạt ngươi!");

            if (Mewtations.Dialogue.DialogueSystem.Instance != null)
            {
                Mewtations.Dialogue.DialogueSystem.Instance.QueueDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_combat", "Chiến đấu!") }, (idx) => {});
            }
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        if (otherCard is RitualCardData && this.MyGameCard.Child == null) 
            return true;
        return false;
    }
}
