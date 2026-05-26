using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat.Battlefield;

public class SaveSystem
{
    private WorldManager _world;

    public bool IsLoadingSaveRound { get; set; }

    public SaveSystem(WorldManager world)
    {
        _world = world;
    }

    public void LoadSaveRound(SaveRound saveRound)
    {
        GameState.IsLoadingSave = true;
        IsLoadingSaveRound = true;
        _world.ClearRound();
        _world.AllCards.Clear();
        _world.UniqueIdToCard.Clear();
        if (RuntimeCardRegistry.Instance != null)
        {
            RuntimeCardRegistry.Instance.ClearAll();
        }

        if (Application.isEditor)
        {
            Debug.Log(string.Format("Loading Run with {0} moon length and peaceful mode: {1}", saveRound.RunOptions.MoonLength, saveRound.RunOptions.IsPeacefulMode));
        }
        _world.MonthTimer = saveRound.MonthTimer;
        _world.OldCurrentMonth = saveRound.CurrentMonth;
        _world.BoardMonths = new BoardMonths(saveRound.BoardMonths);
        _world.GivenCards = saveRound.GivenCards;
        _world.BoughtBoosterIds = saveRound.BoughtBoosterIds;
        _world.CurrentBoard = _world.GetBoardWithId(saveRound.CurrentBoardId);
        _world.CurrentRunOptions = saveRound.RunOptions;
        _world.CurrentRunVariables = saveRound.RunVariables;
        _world.RoundExtraKeyValues = saveRound.ExtraKeyValues ?? new List<SerializedKeyValuePair>();
        if (CitiesManager.instance == null)
        {
            new Exception("CitiesManager should be active before loading the saveRound");
        }
        CitiesManager.instance.Wellbeing = saveRound.CitiesWellbeing;
        CitiesManager.instance.NextConflictMonth = saveRound.CitiesConflictMonth;
        CitiesManager.instance.ActiveEvent = saveRound.CitiesDisaster;
        if (_world.CurrentRunVariables.ActiveDemand != null && string.IsNullOrEmpty(_world.CurrentRunVariables.ActiveDemand.DemandId))
        {
            _world.CurrentRunVariables.ActiveDemand = null;
        }

        // ==========================================
        // PHASE A: CARD INSTANTIATION & REGISTRY
        // ==========================================
        List<CardData> loadedCards = new List<CardData>();
        Dictionary<string, SavedCard> idToSavedCard = new Dictionary<string, SavedCard>();

        foreach (SavedCard savedCard in saveRound.SavedCards)
        {
            CardData cardData = _world.CreateCard(savedCard.CardPosition, savedCard.CardPrefabId, savedCard.FaceUp, false, false);
            if (cardData != null)
            {
                cardData.MyGameCard.MyBoard = _world.GetBoardWithId(savedCard.BoardId);
                
                string oldId = cardData.UniqueId;
                string newId = savedCard.UniqueId;
                cardData.UniqueId = newId;

                // Sync persistent UniqueId with the global RuntimeCardRegistry
                if (RuntimeCardRegistry.Instance != null)
                {
                    CardRuntimeState state = RuntimeCardRegistry.Instance.GetState(oldId);
                    if (state != null)
                    {
                        RuntimeCardRegistry.Instance.UnregisterCard(oldId);
                        state.RuntimeId = newId;
                        RuntimeCardRegistry.Instance.RegisterCard(state, cardData.MyGameCard);
                    }
                    else
                    {
                        state = new CardRuntimeState();
                        state.RuntimeId = newId;
                        state.InitFromCardData(cardData);
                        RuntimeCardRegistry.Instance.RegisterCard(state, cardData.MyGameCard);
                    }
                }

                _world.UniqueIdToCard[newId] = cardData.MyGameCard;
                cardData.SetExtraCardData(savedCard.ExtraCardData);
                if (savedCard.IsFoil)
                {
                    cardData.SetFoil();
                }
                cardData.IsDamaged = savedCard.IsDamaged;
                cardData.DamageType = savedCard.DamageType;

                // Temporarily cache relationship links to avoid resolving before all cards are instantiated
                cardData.ParentUniqueId = savedCard.ParentUniqueId;
                cardData.EquipmentHolderUniqueId = savedCard.EquipmentHolderUniqueId;
                cardData.WorkerHolderUniqueId = savedCard.WorkerHolderUniqueId;
                cardData.WorkerIndex = savedCard.WorkerIndex;

                loadedCards.Add(cardData);
                idToSavedCard[newId] = savedCard;
            }
        }

        // ==========================================
        // PHASE B: RECONNECTING RELATIONSHIPS & STATES
        // ==========================================

        // 1. Restore Connectors configurations
        foreach (CardData cardData in loadedCards)
        {
            if (!idToSavedCard.TryGetValue(cardData.UniqueId, out SavedCard savedCard)) continue;

            if (savedCard.StatusEffects != null && savedCard.StatusEffects.Count > 0)
            {
                List<StatusEffect> list = savedCard.StatusEffects.Select<SavedStatusEffect, StatusEffect>((SavedStatusEffect x) => StatusEffect.FromSavedStatusEffect(x)).ToList<StatusEffect>();
                list.RemoveAll((StatusEffect x) => x == null);
                foreach (StatusEffect statusEffect in list)
                {
                    statusEffect.ParentCard = cardData;
                }
                cardData.StatusEffects = list;
            }
            else
            {
                cardData.StatusEffects = new List<StatusEffect>();
            }

            if (savedCard.CardConnectors != null && savedCard.CardConnectors.Count > 0)
            {
                List<SavedCardConnector> cardConnectors = savedCard.CardConnectors;
                cardConnectors.RemoveAll((SavedCardConnector x) => x == null || string.IsNullOrEmpty(x.ConnectedNodeUniqueId));
                for (int i = 0; i < cardData.MyGameCard.CardConnectorChildren.Count; i++)
                {
                    CardConnector cardConnector = cardData.MyGameCard.CardConnectorChildren[i];
                    string myUniqueId = cardConnector.GetConnectorUniqueId();
                    SavedCardConnector savedCardConnector = cardConnectors.Find((SavedCardConnector x) => x.UniqueId == myUniqueId);
                    if (savedCardConnector != null)
                    {
                        cardConnector.UniqueId = savedCardConnector.UniqueId;
                        cardConnector.ConnectedNodeUniqueId = savedCardConnector.ConnectedNodeUniqueId;
                    }
                }
            }
        }

        // 2. Reconnect physical connector links
        foreach (GameCard gameCard in _world.AllCards)
        {
            foreach (CardConnector connector in gameCard.CardConnectorChildren)
            {
                if (!string.IsNullOrEmpty(connector.ConnectedNodeUniqueId))
                {
                    CardConnector cardConnector2 = (from x in _world.AllCards.SelectMany<GameCard, CardConnector>((GameCard x) => x.CardConnectorChildren)
                        where x.UniqueId == connector.ConnectedNodeUniqueId
                        select x).FirstOrDefault<CardConnector>();
                    if (cardConnector2 != null)
                    {
                        connector.ConnectedNode = cardConnector2;
                    }
                }
            }
        }

        // 3. Reconnect parents (preventing circular parenting loops and deep hierarchies)
        foreach (GameCard gameCard2 in _world.AllCards)
        {
            if (!string.IsNullOrEmpty(gameCard2.CardData.ParentUniqueId))
            {
                if (gameCard2.CardData.ParentUniqueId == gameCard2.UniqueId)
                {
                    Debug.LogWarning($"Circular parenting reference skipped: Card {gameCard2.UniqueId} referenced itself.");
                    gameCard2.CardData.ParentUniqueId = null;
                    continue;
                }

                GameCard cardWithUniqueId = _world.GetCardWithUniqueId(gameCard2.CardData.ParentUniqueId);
                if (cardWithUniqueId != null)
                {
                    // Recursively verify if loading this parent introduces a circular reference
                    GameCard currentParent = cardWithUniqueId;
                    bool isCircular = false;
                    while (currentParent != null)
                    {
                        if (currentParent == gameCard2)
                        {
                            isCircular = true;
                            break;
                        }
                        currentParent = currentParent.Parent;
                    }

                    if (!isCircular)
                    {
                        gameCard2.SetParent(cardWithUniqueId);
                    }
                    else
                    {
                        Debug.LogWarning($"Circular parenting chain detected for card {gameCard2.UniqueId}. Safe cleanup performed.");
                        gameCard2.CardData.ParentUniqueId = null;
                    }
                }
            }
        }

        // 4. Reconnect equipment
        foreach (GameCard gameCard3 in _world.AllCards)
        {
            if (!string.IsNullOrEmpty(gameCard3.CardData.EquipmentHolderUniqueId))
            {
                GameCard cardWithUniqueId2 = _world.GetCardWithUniqueId(gameCard3.CardData.EquipmentHolderUniqueId);
                if (cardWithUniqueId2 != null)
                {
                    cardWithUniqueId2.EquipmentChildren.Add(gameCard3);
                    gameCard3.EquipmentHolder = cardWithUniqueId2;
                    gameCard3.IsEquipped = true;
                }
            }
        }

        // 5. Reconnect workers
        foreach (GameCard gameCard4 in _world.AllCards)
        {
            if (gameCard4.CardData.WorkerAmount > 0)
            {
                gameCard4.WorkerTransformHolder.UpdateWorkerAmount(gameCard4.CardData.WorkerAmount);
            }
            if (!string.IsNullOrEmpty(gameCard4.CardData.WorkerHolderUniqueId))
            {
                GameCard cardWithUniqueId3 = _world.GetCardWithUniqueId(gameCard4.CardData.WorkerHolderUniqueId);
                if (cardWithUniqueId3 != null)
                {
                    if (cardWithUniqueId3.WorkerChildren.Count < cardWithUniqueId3.CardData.WorkerAmount)
                    {
                        cardWithUniqueId3.WorkerChildren.Add(gameCard4);
                        gameCard4.WorkerHolder = cardWithUniqueId3;
                        gameCard4.IsWorking = true;
                    }
                    else
                    {
                        gameCard4.CardData.WorkerHolderUniqueId = null;
                        gameCard4.IsWorking = false;
                    }
                }
            }
        }

        // 6. Notify status effects changed safely
        foreach (GameCard gameCard5 in _world.AllCards)
        {
            gameCard5.StatusEffectsChanged();
        }

        // 7. Reconnect active combat conflicts
        foreach (SavedConflict savedConflict in saveRound.SavedConflicts)
        {
            BattlefieldContext.CreateFromSavedConflict(savedConflict);
        }

        // 8. Reconnect boosters
        foreach (SavedBooster savedBooster2 in saveRound.SavedBoosters)
        {
            Boosterpack boosterpack = _world.CreateBoosterpack(savedBooster2.Position, savedBooster2.BoosterId);
            if (boosterpack != null)
            {
                boosterpack.MyBoard = _world.GetBoardWithId(savedBooster2.BoardId);
                int num = savedBooster2.TimesOpened;
                boosterpack.TimesOpened = savedBooster2.TimesOpened;
                for (int j = 0; j < boosterpack.CardBags.Count; j++)
                {
                    CardBag cardBag = boosterpack.CardBags[j];
                    int num2 = Mathf.Min(num, cardBag.CardsInPack);
                    cardBag.CardsInPack -= num2;
                    num -= num2;
                    if (num <= 0)
                    {
                        break;
                    }
                }
            }
        }

        // 9. Reconnect booster boxes
        foreach (SavedBoosterBox savedBooster in saveRound.SavedBoosterBoxes)
        {
            BuyBoosterBox buyBoosterBox = _world.AllBoosterBoxes.Find((BuyBoosterBox x) => x.BoosterId == savedBooster.BoosterId);
            if (buyBoosterBox != null)
            {
                buyBoosterBox.StoredCostAmount = savedBooster.StoredCostAmount;
            }
        }

        // 10. Start running timers in Phase B where environment details are fully reconnected
        foreach (CardData cardData in loadedCards)
        {
            if (!idToSavedCard.TryGetValue(cardData.UniqueId, out SavedCard savedCard)) continue;

            if (savedCard.TimerRunning)
            {
                TimerAction delegateForActionId = cardData.GetDelegateForActionId(savedCard.TimerActionId);
                if (delegateForActionId != null)
                {
                    cardData.MyGameCard.StartTimer(savedCard.TargetTimerTime, delegateForActionId, savedCard.Status, savedCard.TimerActionId, savedCard.WithStatusBar, true, false);
                    cardData.MyGameCard.CurrentTimerTime = savedCard.CurrentTimerTime;
                    cardData.MyGameCard.TimerBlueprintId = savedCard.TimerBlueprintId;
                    cardData.MyGameCard.TimerSubprintIndex = savedCard.SubprintIndex;
                    cardData.MyGameCard.SkipCitiesChecks = savedCard.SkipCitiesChecks;
                }
            }
        }

        if (saveRound.SaveVersion != 3)
        {
            PerformSaveRoundMigration(saveRound.SaveVersion, 3);
        }
        if (Mewtations.Expedition.ExpeditionManager.Instance != null)
        {
            Mewtations.Expedition.ExpeditionManager.Instance.LoadFromExtraKeyValues(_world.RoundExtraKeyValues);
        }

        GameState.IsLoadingSave = false;
        IsLoadingSaveRound = false;
    }

    public void PerformSaveRoundMigration(int oldVersion, int newVersion)
    {
        if (oldVersion == 0 && newVersion == 1)
        {
            Debug.Log(string.Format("Performing save round migration from v{0} to v{1}", oldVersion, newVersion));
            foreach (GameCard gameCard in _world.AllCards)
            {
                BaseVillager baseVillager = gameCard.CardData as BaseVillager;
                if (baseVillager != null)
                {
                    baseVillager.HealthPoints = Mathf.Min(baseVillager.ProcessedCombatStats.MaxHealth, baseVillager.HealthPoints * 3);
                }
            }
            for (int i = _world.AllCards.Count - 1; i >= 0; i--)
            {
                Combatable combatable = _world.AllCards[i].CardData as Combatable;
                if (combatable != null)
                {
                    if (combatable.Id == "swordsman")
                    {
                        combatable.CreateAndEquipCard("sword", true);
                    }
                    if (combatable.Id == "explorer")
                    {
                        combatable.CreateAndEquipCard("map", true);
                    }
                    if (combatable.Id == "militia")
                    {
                        combatable.CreateAndEquipCard("spear", true);
                    }
                    if (combatable.Id == "fisher")
                    {
                        combatable.CreateAndEquipCard("fishing_rod", true);
                    }
                }
            }
        }
        if (oldVersion == 1 && newVersion == 2 && _world.BoardMonths.IsEmpty && _world.MonthTimer > 0f)
        {
            _world.BoardMonths = new BoardMonths();
            _world.BoardMonths.MainMonth = _world.OldCurrentMonth - _world.CurrentRunVariables.IslandMonths;
            _world.BoardMonths.IslandMonth = _world.CurrentRunVariables.IslandMonths;
            _world.BoardMonths.DeathMonth = Mathf.Max(1, _world.CurrentRunVariables.DeathMonths);
        }
        if (oldVersion == 2 && newVersion == 3)
        {
            List<GameCard> list = _world.AllCards.Where<GameCard>((GameCard x) => x.CardData.Id == "strange_portal").ToList<GameCard>();
            for (int j = 0; j < list.Count - 1; j++)
            {
                list[j].DestroyCard(false, true);
            }
        }
    }

    public SaveRound GetSaveRound()
    {
        SaveRound saveRound = new SaveRound();
        saveRound.SaveVersion = 3;
        saveRound.SavedCards = new List<SavedCard>();
        saveRound.SavedBoosters = new List<SavedBooster>();
        saveRound.SavedBoosterBoxes = new List<SavedBoosterBox>();
        saveRound.SavedConflicts = new List<SavedConflict>();
        saveRound.RunVariables = _world.CurrentRunVariables;
        saveRound.RunOptions = _world.CurrentRunOptions;
        saveRound.BoughtBoosterIds = _world.BoughtBoosterIds;
        saveRound.CurrentBoardId = _world.CurrentBoard.Id;
        if (_world.RoundExtraKeyValues == null)
        {
            _world.RoundExtraKeyValues = new List<SerializedKeyValuePair>();
        }
        saveRound.ExtraKeyValues = _world.RoundExtraKeyValues;
        if (Mewtations.Expedition.ExpeditionManager.Instance != null)
        {
            Mewtations.Expedition.ExpeditionManager.Instance.SaveToExtraKeyValues(_world.RoundExtraKeyValues);
        }
        foreach (GameCard gameCard in _world.AllCards)
        {
            saveRound.SavedCards.Add(gameCard.ToSavedCard());
        }
        foreach (Boosterpack boosterpack in _world.AllBoosters)
        {
            saveRound.SavedBoosters.Add(new SavedBooster
            {
                BoosterId = boosterpack.BoosterId,
                TimesOpened = boosterpack.TimesOpened,
                BoardId = boosterpack.MyBoard.Id,
                Position = boosterpack.TargetPosition
            });
        }
        foreach (BuyBoosterBox buyBoosterBox in _world.AllBoosterBoxes)
        {
            saveRound.SavedBoosterBoxes.Add(new SavedBoosterBox
            {
                BoosterId = buyBoosterBox.BoosterId,
                StoredCostAmount = buyBoosterBox.StoredCostAmount
            });
        }
        saveRound.MonthTimer = _world.MonthTimer;
        saveRound.CurrentMonth = _world.CurrentMonth;
        saveRound.OldCurrentMonth = _world.OldCurrentMonth;
        saveRound.BoardMonths = _world.BoardMonths.ToSavedMonth();
        saveRound.NewCardsFound = _world.NewCardsFound;
        saveRound.QuestsCompleted = _world.QuestsCompleted;
        saveRound.CitiesWellbeing = CitiesManager.instance.Wellbeing;
        saveRound.CitiesConflictMonth = CitiesManager.instance.NextConflictMonth;
        saveRound.CitiesDisaster = CitiesManager.instance.ActiveEvent;
        foreach (BattlefieldContext BattlefieldContext in _world.GetAllConflicts())
        {
            List<SavedConflict> savedConflicts = saveRound.SavedConflicts;
            SavedConflict savedConflict = new SavedConflict();
            savedConflict.Id = BattlefieldContext.Id;
            savedConflict.InitiatorCardId = BattlefieldContext.Initiator.UniqueId;
            savedConflict.InvolvedCards = BattlefieldContext.Participants.Select<Combatable, string>((Combatable x) => x.UniqueId).ToList<string>();
            savedConflict.StartPosition = BattlefieldContext.ConflictStartPosition;
            savedConflicts.Add(savedConflict);
        }
        return saveRound;
    }
}

