using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Core; // For StructureSlotData

public enum CatGodRitualState { Idle, Consuming, Completing, Invalid }

public class CatGodMouth : CardData
{
    [Header("Cat God Mouth Settings")]
    [ExtraData("ritual_state")]
    public CatGodRitualState State = CatGodRitualState.Idle;

    [ExtraData("active_ritual_id")]
    public string ActiveRitualId = "";

    [ExtraData("offering_progress")]
    public int OfferingProgress = 0;
    
    [ExtraData("total_blasphemy")]
    public int TotalBlasphemy = 0;

    [ExtraData("has_warned_blasphemy")]
    public bool HasWarnedBlasphemy = false;
    
    [ExtraData("has_triggered_threat")]
    public bool HasTriggeredThreat = false;

    // [PHASE 4: GOD CAT MIGRATION]
    public StructureSlotData RitualSlot;
    public StructureSlotData OfferingSlot;

    private RitualCardData CurrentRitualDef;
    private bool DirtyUI = false;
    private bool IsConsumingItem = false;

    public override bool DetermineCanHaveCardsWhenIsRoot => true;

    protected override void Awake()
    {
        base.Awake();
        
        RitualSlot = new StructureSlotData
        {
            SlotId = "catgod_ritual",
            LocalOffset = new Vector3(0, 0.1f, 1.5f),
            OccupancyPolicy = OccupancyPolicy.Single
        };

        OfferingSlot = new StructureSlotData
        {
            SlotId = "catgod_offering",
            LocalOffset = new Vector3(0, 0.1f, 0f),
            OccupancyPolicy = OccupancyPolicy.Single
        };
    }

    public string GetValidSlotFor(CardData otherCard)
    {
        if (State == CatGodRitualState.Idle && otherCard is RitualCardData && RitualSlot.SlotOccupants.Count == 0)
        {
            return RitualSlot.SlotId;
        }
        else if (State == CatGodRitualState.Consuming && otherCard.CanBeConsumedByRitual && OfferingSlot.SlotOccupants.Count == 0)
        {
            return OfferingSlot.SlotId;
        }
        return null;
    }

    public StructureSlotData GetSlotById(string slotId)
    {
        if (RitualSlot.SlotId == slotId) return RitualSlot;
        if (OfferingSlot.SlotId == slotId) return OfferingSlot;
        return null;
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Vô hiệu hóa stack gốc
        return false;
    }

    public override void UpdateCard()
    {
        base.UpdateCard();

        if (this.MyGameCard == null) return;

        // Xử lý nam châm từ Attachment System
        if (RitualSlot.SlotOccupants.Count > 0)
        {
            GameCard r = RitualSlot.SlotOccupants[0];
            if (r != null && !r.Destroyed) r.transform.position = Vector3.Lerp(r.transform.position, transform.position + RitualSlot.LocalOffset, Time.deltaTime * 10f);
        }
        if (OfferingSlot.SlotOccupants.Count > 0)
        {
            GameCard o = OfferingSlot.SlotOccupants[0];
            if (o != null && !o.Destroyed) o.transform.position = Vector3.Lerp(o.transform.position, transform.position + OfferingSlot.LocalOffset, Time.deltaTime * 10f);
        }

        // Self-heal and cache rebuild
        if (State != CatGodRitualState.Idle && State != CatGodRitualState.Invalid)
        {
            if (CurrentRitualDef == null)
            {
                RebuildRitualCache();
                if (CurrentRitualDef == null)
                {
                    EnterInvalidState("Missing ritual definition for ID: " + ActiveRitualId);
                    return;
                }
            }
        }

        if (DirtyUI)
        {
            UpdateUI();
            DirtyUI = false;
        }

        if (State == CatGodRitualState.Idle)
        {
            if (RitualSlot.SlotOccupants.Count > 0)
            {
                GameCard ritualCard = RitualSlot.SlotOccupants[0];
                if (ritualCard != null && !ritualCard.Destroyed && ritualCard.CardData is RitualCardData rData)
                {
                    RitualSlot.SlotOccupants.Clear();
                    ActivateRitual(rData);
                }
            }
        }
        else if (State == CatGodRitualState.Consuming)
        {
            // Self-Healing Loop
            if (!IsConsumingItem && !this.MyGameCard.TimerRunning && OfferingSlot.SlotOccupants.Count > 0)
            {
                GameCard topCard = OfferingSlot.SlotOccupants[0];
                if (topCard != null && !topCard.Destroyed && topCard.CardData.CanBeConsumedByRitual)
                {
                    this.MyGameCard.StartTimer(0.25f, new TimerAction(this.ConsumeOffering), MewtationsLoc.Translate("catgod_consume_timer", "Tiếp nhận..."), "offering");
                }
            }
        }
    }

    private void RebuildRitualCache()
    {
        if (string.IsNullOrEmpty(ActiveRitualId)) return;
        CurrentRitualDef = WorldManager.instance.GameDataLoader.GetCardFromId(ActiveRitualId, false) as RitualCardData;
        if (CurrentRitualDef != null)
        {
            MarkUIDirty();
        }
    }

    private void MarkUIDirty()
    {
        DirtyUI = true;
    }

    private void UpdateUI()
    {
        if (State == CatGodRitualState.Idle)
        {
            this.descriptionOverride = MewtationsLoc.Translate("catgod_idle", "Kéo một Thẻ Nghi Lễ (Ritual Card) vào đây để bắt đầu.");
        }
        else if (State == CatGodRitualState.Consuming && CurrentRitualDef != null)
        {
            this.descriptionOverride = MewtationsLoc.TranslateFormat("catgod_ritual_active",
                "Nghi lễ đang diễn ra...\nTiến độ: {0}/{1}\nBáng bổ: {2}%",
                OfferingProgress, CurrentRitualDef.RequiredDevotion, GetBlasphemyPercent(CurrentRitualDef));
        }
        else if (State == CatGodRitualState.Completing)
        {
            this.descriptionOverride = MewtationsLoc.Translate("catgod_completing", "Thần Mèo đang ban phước...");
        }
        else if (State == CatGodRitualState.Invalid)
        {
            this.descriptionOverride = MewtationsLoc.Translate("catgod_invalid", "Nghi lễ bị gián đoạn hoặc lỗi hệ thống.");
        }
    }

    private void ActivateRitual(RitualCardData ritualCard)
    {
        ActiveRitualId = ritualCard.Id;
        State = CatGodRitualState.Consuming;
        
        if (WorldManager.instance != null)
        {
            WorldManager.instance.CreateSmoke(ritualCard.transform.position);
        }
        if (AudioManager.me != null)
        {
            AudioManager.me.PlaySound2D(AudioManager.me.FireStruggle, 1f, 0.2f);
        }

        // Delay destroy by 0.1s to allow effects to play
        ritualCard.MyGameCard.StartCoroutine(DelayDestroyRitual(ritualCard.MyGameCard));

        RebuildRitualCache();
    }

    private IEnumerator DelayDestroyRitual(GameCard card)
    {
        yield return new WaitForSeconds(0.1f);
        if (card != null)
        {
            card.DestroyCard(true, true);
        }
    }

    private void EnterInvalidState(string reason)
    {
        Debug.LogWarning("[CatGodMouth] Entering Invalid State. Reason: " + reason);
        this.MyGameCard.CancelAnyTimer();
        EjectAllChildren();
        State = CatGodRitualState.Idle;
        ActiveRitualId = "";
        OfferingProgress = 0;
        TotalBlasphemy = 0;
        HasWarnedBlasphemy = false;
        HasTriggeredThreat = false;
        CurrentRitualDef = null;
        IsConsumingItem = false;
        MarkUIDirty();
    }

    private void ConsumeOffering()
    {
        if (State != CatGodRitualState.Consuming || CurrentRitualDef == null) return;

        try
        {
            IsConsumingItem = true;
            
            if (OfferingSlot.SlotOccupants.Count == 0) return;

            GameCard topCard = OfferingSlot.SlotOccupants[0];
            OfferingSlot.SlotOccupants.Remove(topCard);

            if (topCard == null || topCard.Destroyed) return;

            CardData offeringData = topCard.CardData;

            // Push invalid cards out just in case they slipped in
            if (!offeringData.CanBeConsumedByRitual)
            {
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

            MarkUIDirty();

            if (OfferingProgress >= CurrentRitualDef.RequiredDevotion)
            {
                State = CatGodRitualState.Completing;
                MarkUIDirty();
                this.MyGameCard.StartTimer(0.5f, new TimerAction(this.FinishRitual), MewtationsLoc.Translate("catgod_finishing", "Hoàn tất..."), "ritual_finish");
            }
        }
        finally
        {
            IsConsumingItem = false;
        }
    }

    private void FinishRitual()
    {
        // 1. Play FX
        if (WorldManager.instance != null)
        {
            WorldManager.instance.CreateSmoke(this.transform.position);
        }
        if (GameCamera.instance != null)
        {
            GameCamera.instance.Screenshake = 0.5f;
        }
        if (AudioManager.me != null)
        {
            AudioManager.me.PlaySound2D(AudioManager.me.CardPackOpen, 1f, 0.5f);
        }

        // 2. Detach leftover stack
        EjectAllChildren();

        // 3. Spawn reward
        if (CurrentRitualDef != null)
        {
            CheckRitualThreatsAndSpawnReward(CurrentRitualDef);
        }

        // 4 & 5. Clear refs & cache
        CurrentRitualDef = null;

        // 6. Reset state
        ActiveRitualId = "";
        OfferingProgress = 0;
        TotalBlasphemy = 0;
        HasWarnedBlasphemy = false;
        HasTriggeredThreat = false;
        State = CatGodRitualState.Idle;

        // 7. Dirty UI
        MarkUIDirty();
    }

    private void EjectAllChildren()
    {
        if (OfferingSlot.SlotOccupants.Count > 0)
        {
            foreach (var card in OfferingSlot.SlotOccupants)
            {
                if (card != null && !card.Destroyed)
                {
                    Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                    card.BounceTarget = card.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * 2.5f;
                }
            }
            OfferingSlot.SlotOccupants.Clear();
        }
        if (RitualSlot.SlotOccupants.Count > 0)
        {
            foreach (var card in RitualSlot.SlotOccupants)
            {
                if (card != null && !card.Destroyed)
                {
                    Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                    card.BounceTarget = card.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * 2.5f;
                }
            }
            RitualSlot.SlotOccupants.Clear();
        }
    }

    private int GetBlasphemyPercent(RitualCardData ritualCard)
    {
        if (ritualCard.RequiredDevotion <= 0) return 0;
        return Mathf.RoundToInt(((float)TotalBlasphemy / ritualCard.RequiredDevotion) * 100);
    }

    private void CheckRitualThreatsAndSpawnReward(RitualCardData ritualCard)
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
            
            Vector3 threatSpawnPos = this.transform.position + Vector3.left * 1.5f + Vector3.back * 1.5f;
            
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

        Vector3 rewardSpawnPos = this.transform.position + Vector3.right * 1.5f + Vector3.back * 1.5f;
        WorldManager.instance.CreateCard(rewardSpawnPos, ritualCard.RewardPackId, true, true, true);

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
}
