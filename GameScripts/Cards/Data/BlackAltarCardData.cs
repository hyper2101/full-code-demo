using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;
using Mewtations.Combat.Core;

public enum OfferingAffinityType
{
    None,
    Fire,
    Poison,
    Lightning
}

public enum RitualState
{
    Idle,
    Charging,
    BossSpawned,
    Destroyed
}

public class BlackAltarCardData : CardData
{
    [Header("Black Altar State")]
    [ExtraData("ritual_state")]
    public RitualState State = RitualState.Idle;

    [ExtraData("offering_affinity")]
    public OfferingAffinityType CurrentAffinity = OfferingAffinityType.None;

    [ExtraData("ritual_start_time")]
    public float RitualStartTime = 0f;

    public float RitualDuration = 10f; // Tùy chỉnh 10 giây nạp tiến trình

    // Cần 3 thức ăn hiến tế để bắt đầu
    public int RequiredOfferings = 3;

    public override bool DetermineCanHaveCardsWhenIsRoot => true;

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Khóa đồ khi đang Charging hoặc đã gọi Boss
        if (State != RitualState.Idle) return false;

        // Giả sử tế phẩm có Id hoặc tag đặc biệt.
        // Tạm thời dựa vào CardData extension hoặc ID prefix để demo:
        return IsRitualOffering(otherCard);
    }

    private bool IsRitualOffering(CardData card)
    {
        if (card == null || string.IsNullOrEmpty(card.Id)) return false;
        
        string id = card.Id.ToLower();
        // Giả sử tế phẩm hiến tế có tag item_ritual_offering_...
        return id.StartsWith("item_ritual_offering_");
    }

    private OfferingAffinityType GetAffinityFromOffering(CardData card)
    {
        if (card == null || string.IsNullOrEmpty(card.Id)) return OfferingAffinityType.None;
        
        string id = card.Id.ToLower();
        if (id.Contains("fire")) return OfferingAffinityType.Fire;
        if (id.Contains("poison")) return OfferingAffinityType.Poison;
        if (id.Contains("lightning")) return OfferingAffinityType.Lightning;
        return OfferingAffinityType.None;
    }

    public override void UpdateCard()
    {
        base.UpdateCard();

        if (this.MyGameCard == null) return;

        if (State == RitualState.Idle)
        {
            // Kiểm tra số lượng đồ trên bàn thờ
            int offeringCount = 0;
            OfferingAffinityType lastAffinity = OfferingAffinityType.None;
            GameCard curr = this.MyGameCard.Child;

            while (curr != null)
            {
                if (IsRitualOffering(curr.CardData))
                {
                    offeringCount++;
                    lastAffinity = GetAffinityFromOffering(curr.CardData);
                }
                curr = curr.Child;
            }

            if (offeringCount >= RequiredOfferings)
            {
                // Bắt đầu hiến tế
                StartRitual(lastAffinity);
            }
        }
        else if (State == RitualState.Charging)
        {
            // Khôi phục thanh tiến trình nếu nó không chạy do load game
            if (!this.MyGameCard.TimerRunning)
            {
                RebuildProgress();
            }
        }
    }

    private void StartRitual(OfferingAffinityType affinity)
    {
        State = RitualState.Charging;
        CurrentAffinity = affinity;
        RitualStartTime = WorldManager.instance.CurrentMonth; // Hoặc Time.time nếu dùng system time
        
        StartTimerProgress();
    }

    private void StartTimerProgress()
    {
        this.MyGameCard.StartTimer(RitualDuration, new TimerAction(CompleteRitual), "Đang nạp Hắc Đàn...", "black_altar_ritual", true, false, false);
    }

    private void RebuildProgress()
    {
        // Phục hồi lại quá trình sau khi load
        StartTimerProgress();
    }

    [TimedAction("black_altar_ritual")]
    public void CompleteRitual()
    {
        if (this.MyGameCard == null) return;

        // Tiêu hủy vật phẩm
        GameCard curr = this.MyGameCard.Child;
        List<GameCard> toDestroy = new List<GameCard>();
        while (curr != null)
        {
            toDestroy.Add(curr);
            curr = curr.Child;
        }

        foreach (var gc in toDestroy)
        {
            if (gc != null && !gc.Destroyed) gc.DestroyCard(true, true);
        }

        State = RitualState.BossSpawned;

        // Triệu hồi Boss dựa trên Affinity
        SpawnBossFromAffinity();
    }

    private void SpawnBossFromAffinity()
    {
        string encounterName = "Black Altar Boss (Fire)"; // Default fallback
        if (CurrentAffinity == OfferingAffinityType.Poison) encounterName = "Black Altar Boss (Poison)";
        else if (CurrentAffinity == OfferingAffinityType.Lightning) encounterName = "Black Altar Boss (Lightning)";

        Debug.Log($"[BlackAltar] Spawning Threat Card for: {encounterName}");

        // 1. Prepare temporary Encounter Data
        EncounterData newEncounter = new EncounterData
        {
            EncounterName = encounterName,
            Context = EncounterContext.BlackAltar,
            TurnLimit = 50
        };
        // TODO: Populate temporary enemies. The user will carefully edit this later.
        
        int encounterId = 999;
        if (EncounterManager.Instance != null)
        {
            encounterId = EncounterManager.Instance.RegisterEncounter(newEncounter);
        }

        // 2. Create Threat Instance
        var bossThreat = new GameScripts.Systems.Threat.ThreatInstance(null, GameScripts.Systems.Threat.ThreatSourceType.Event)
        {
            CurrentSeverity = Mewtations.Core.Severity.Critical,
            ThreatExpiryMonth = WorldManager.instance.CurrentMonth + 7, // 7 months to fight the boss before consequence
            EncounterId = encounterId
        };

        // 3. Spawn Physical Threat Card
        Vector3 spawnPos = this.MyGameCard.transform.position + new Vector3(2f, 0f, 0f); // Spawn right next to the altar
        GameCard spawnedCard = WorldManager.instance.CreateCard(spawnPos, "blackaltar_threat", true, true, true);
        
        if (spawnedCard != null)
        {
            var threatComp = spawnedCard.GetComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
            if (threatComp == null)
            {
                threatComp = spawnedCard.gameObject.AddComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
            }
            threatComp.Initialize(bossThreat);
            
            // Focus camera on the new threat
            if (GameScripts.Visuals.GameCamera.instance != null)
            {
                GameScripts.Visuals.GameCamera.instance.FocusOn(spawnedCard.CardData);
            }
        }
        else
        {
            Debug.LogError("[BlackAltar] Failed to spawn 'blackaltar_threat' card! Please ensure the prefab or card ID exists.");
        }
    }

    public override void UpdateCardText()
    {
        base.UpdateCardText();
        this.descriptionOverride = MewtationsLoc.Translate(this.DescriptionTerm) + $"\n\n<b>State:</b> {State}";
        if (State == RitualState.Charging)
        {
            this.descriptionOverride += $"\n<b>Affinity:</b> <color=#e74c3c>{CurrentAffinity}</color>";
        }
    }
}
