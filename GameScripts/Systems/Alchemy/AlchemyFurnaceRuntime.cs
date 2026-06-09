using System.Collections.Generic;
using UnityEngine;
using Mewtations.Core;

namespace Mewtations.Systems.Alchemy
{
    public class AlchemyFurnaceRuntime : BaseStructureRuntime
    {
        public List<AlchemyRecipe> AvailableRecipes = new List<AlchemyRecipe>(); // Typically loaded from a database

        [Header("Furnace State")]
        public AlchemyRecipe CurrentValidRecipe;
        public float FurnaceProgress;
        public bool IsRefining;

        // Action events for UI
        public System.Action<AlchemyRecipe> OnRecipeValid;
        public System.Action<float> OnProgressUpdated;
        public System.Action OnRefiningStarted;
        public System.Action OnRefiningStopped;

        public void InitializeSlots()
        {
            Slots.Clear();
            
            // Example configuration: 1 Cat Slot, 4 Ingredient Slots
            Slots.Add(new StructureSlot(StructureSlotType.Cat, new Vector3(0, 0, 0.5f)));
            
            Slots.Add(new StructureSlot(StructureSlotType.Ingredient, new Vector3(-0.7f, 0, -0.5f)));
            Slots.Add(new StructureSlot(StructureSlotType.Ingredient, new Vector3(-0.3f, 0, -0.5f)));
            Slots.Add(new StructureSlot(StructureSlotType.Ingredient, new Vector3(0.3f, 0, -0.5f)));
            Slots.Add(new StructureSlot(StructureSlotType.Ingredient, new Vector3(0.7f, 0, -0.5f)));
        }

        protected override void Awake()
        {
            base.Awake();
            if (Slots.Count == 0)
            {
                InitializeSlots();
            }
        }

        public override bool IsRuntimeActive => IsRefining;

        public override bool TryInsertCard(CardData incomingData)
        {
            // If currently refining, reject new inserts
            if (IsRefining) return false;

            StructureSlotType targetType = incomingData is CatCardData ? StructureSlotType.Cat : StructureSlotType.Ingredient;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].SlotType == targetType && Slots[i].IsEmpty)
                {
                    Slots[i].CurrentCard = incomingData.MyGameCard;
                    incomingData.MyGameCard.CurrentLock = CardLockReason.ControlledByStructure;
                    
                    CheckRecipe();
                    return true;
                }
            }
            return false;
        }

        protected override void OnCardInterrupted(StructureSlot slot, GameCard card)
        {
            base.OnCardInterrupted(slot, card);
            
            if (IsRefining)
            {
                StopRefining();
            }
            else
            {
                CheckRecipe();
            }
        }

        private void CheckRecipe()
        {
            Dictionary<string, int> currentIngredients = new Dictionary<string, int>();
            bool hasCat = false;

            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.CurrentCard != null)
                {
                    if (slot.SlotType == StructureSlotType.Ingredient)
                    {
                        string id = slot.CurrentCard.CardData.Id;
                        if (!currentIngredients.ContainsKey(id)) currentIngredients[id] = 0;
                        currentIngredients[id]++;
                    }
                    else if (slot.SlotType == StructureSlotType.Cat)
                    {
                        hasCat = true;
                    }
                }
            }

            CurrentValidRecipe = null;
            foreach (var recipe in AvailableRecipes)
            {
                if (recipe.IsExactMatch(currentIngredients))
                {
                    CurrentValidRecipe = recipe;
                    break;
                }
            }

            if (CurrentValidRecipe != null)
            {
                OnRecipeValid?.Invoke(CurrentValidRecipe);

                // Auto start if cat is present
                if (hasCat)
                {
                    StartRefining();
                }
            }
            else
            {
                OnRecipeValid?.Invoke(null);
            }
        }

        private void StartRefining()
        {
            IsRefining = true;
            FurnaceProgress = 0f;
            
            // Lock all cards to refining state
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.CurrentCard != null)
                {
                    slot.CurrentCard.CurrentLock = CardLockReason.Refining;
                }
            }

            OnRefiningStarted?.Invoke();
        }

        private void StopRefining()
        {
            IsRefining = false;
            FurnaceProgress = 0f;
            CurrentValidRecipe = null;

            // Release locks back to soft control for remaining cards
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.CurrentCard != null)
                {
                    slot.CurrentCard.CurrentLock = CardLockReason.ControlledByStructure;
                }
            }

            OnRefiningStopped?.Invoke();
            CheckRecipe(); // Re-evaluate state
        }

        protected override void UpdateGameplayLogic()
        {
            if (!IsRefining || CurrentValidRecipe == null) return;

            // Base progression rate (Cat buffs can be applied here)
            float speedMultiplier = 1f; 
            FurnaceProgress += Time.deltaTime * speedMultiplier;

            OnProgressUpdated?.Invoke(FurnaceProgress / CurrentValidRecipe.RefiningDuration);

            if (FurnaceProgress >= CurrentValidRecipe.RefiningDuration)
            {
                CompleteRefining();
            }
        }

        private void CompleteRefining()
        {
            IsRefining = false;
            
            GameCard root = _cardData.MyGameCard.GetRootCard();
            Vector3 spawnPos = root.transform.position;

            // Destroy ingredients, unlock cat
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.CurrentCard != null)
                {
                    if (slot.SlotType == StructureSlotType.Ingredient)
                    {
                        slot.CurrentCard.DestroyCard(true, true);
                        slot.Clear();
                    }
                    else if (slot.SlotType == StructureSlotType.Cat)
                    {
                        slot.CurrentCard.CurrentLock = CardLockReason.None;
                        slot.Clear();
                    }
                }
            }

            // Spawn output
            CardData newCard = WorldManager.instance.CreateCard(spawnPos, CurrentValidRecipe.ResultCardId, true, false, true);
            if (newCard != null)
            {
                Vector2 randDir = UnityEngine.Random.insideUnitCircle.normalized;
                newCard.MyGameCard.BounceTarget = spawnPos + new Vector3(randDir.x, 0, randDir.y) * 1.5f;
            }

            OnRefiningStopped?.Invoke();
            CheckRecipe();
        }
    }
}
