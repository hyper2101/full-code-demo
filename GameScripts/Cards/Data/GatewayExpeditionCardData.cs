using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Mewtations.Expedition;
using Mewtations.Legacy.Stacklands;

public class GatewayExpeditionCardData : CardData
{
    public List<ExpeditionRouteDefinition> Routes { get; private set; }
    public ExpeditionRouteDefinition SelectedRoute { get; private set; }

    private bool _isRouteUiOpen = false;

    public void Awake()
    {
        // Initialize routes (Mock unlock conditions for now)
        Routes = new List<ExpeditionRouteDefinition>
        {
            new ExpeditionRouteDefinition { SlotId = "route_1", DisplayName = "Thám Thính (Scout)", Difficulty = ExpeditionDifficulty.Easy, IsUnlocked = true },
            new ExpeditionRouteDefinition { SlotId = "route_2", DisplayName = "Tiền Đồn (Outpost)", Difficulty = ExpeditionDifficulty.Medium, IsUnlocked = false, UnlockConditionId = "build_barracks" },
            new ExpeditionRouteDefinition { SlotId = "route_3", DisplayName = "Học Viện (Academy)", Difficulty = ExpeditionDifficulty.Hard, IsUnlocked = false, UnlockConditionId = "build_academy" },
            new ExpeditionRouteDefinition { SlotId = "route_4", DisplayName = "Vực Sâu (Abyss)", Difficulty = ExpeditionDifficulty.Elite, IsUnlocked = false, UnlockConditionId = "kill_first_boss" }
        };
    }

    public override bool CanHaveCardsWhileHasStatus()
    {
        return true;
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Gateway 2.0: Only accepts OrderingCardData to initiate expeditions.
        // It is no longer a logistics container for Food/Gold/Backpacks/Relics.
        return otherCard is OrderingCardData;
    }

    public override bool DetermineCanHaveCardsWhenIsRoot => true;

    public override void UpdateCard()
    {
        base.UpdateCard();

        if (this.MyGameCard == null) return;

        // Flow: Ordering -> Gateway -> Route Selection UI
        bool hasOrdering = this.MyGameCard.Child != null && this.MyGameCard.Child.CardData is OrderingCardData;
        
        if (hasOrdering && !this.MyGameCard.TimerRunning && !ExpeditionManager.Instance.IsExpeditionActive && !_isRouteUiOpen)
        {
            // The Ordering card just landed and no timer/expedition is running.
            // Phase 3 Route Selection UI should pop up here.
            OpenRouteSelectionUI();
        }
        else if (!hasOrdering)
        {
            _isRouteUiOpen = false;
        }
    }

    public void SetRouteUiOpen(bool isOpen)
    {
        _isRouteUiOpen = isOpen;
    }

    private void OpenRouteSelectionUI()
    {
        _isRouteUiOpen = true;
        
        var ui = this.MyGameCard.gameObject.GetComponent<GatewayRouteUI>();
        if (ui == null)
        {
            ui = this.MyGameCard.gameObject.AddComponent<GatewayRouteUI>();
        }
        
        ui.Open(this);
    }

    public void BeginPreparingExpedition(ExpeditionRouteDefinition route)
    {
        SelectedRoute = route;
        // Start 2s "Preparing Expedition" timer
        this.MyGameCard.StartTimer(2f, new TimerAction(this.OnPreparationComplete), "Chuẩn bị viễn chinh...", base.GetActionId("PrepareExpedition"));
    }

    [TimedAction("prepare_expedition")]
    private void OnPreparationComplete()
    {
        _isRouteUiOpen = false; // Reset for next time

        if (this.MyGameCard.Child != null && this.MyGameCard.Child.CardData is OrderingCardData ordering)
        {
            var context = new ExpeditionRunContext
            {
                Ordering = ordering,
                Route = SelectedRoute
            };
            
            ExpeditionManager.Instance.StartExpedition(context);
        }
    }
}
