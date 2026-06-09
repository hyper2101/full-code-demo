using UnityEngine;
using System.Collections.Generic;

namespace Mewtations.Core
{
    public abstract class BaseStructureRuntime : MonoBehaviour
    {
        public List<StructureSlot> Slots = new List<StructureSlot>();
        public float BreakDistance = 2.5f;
        public float SnapSpeed = 15f;
        
        protected CardData _cardData;

        protected virtual void Awake()
        {
            _cardData = GetComponent<CardData>();
        }

        public virtual bool TryInsertCard(CardData incomingData)
        {
            // Override in subclasses for specific insert logic
            return false;
        }

        public bool HasActiveSlots()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (!Slots[i].IsEmpty) return true;
            }
            return false;
        }

        public virtual bool IsRuntimeActive => false;

        protected virtual void Update()
        {
            if (_cardData == null || _cardData.MyGameCard == null || _cardData.MyGameCard.IsDemoCard)
                return;

            if (!HasActiveSlots() && !IsRuntimeActive)
                return;

            UpdateMagneticSnap();
            UpdateGameplayLogic();
        }

        protected void UpdateMagneticSnap()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                StructureSlot slot = Slots[i];
                if (slot.CurrentCard != null)
                {
                    GameCard card = slot.CurrentCard;

                    // 1. Player Drag Priority
                    if (card.BeingDragged)
                    {
                        Vector3 desiredPos = transform.position + slot.LocalOffset;
                        float dist = Vector3.Distance(card.transform.position, desiredPos);
                        
                        if (dist > BreakDistance)
                        {
                            OnCardInterrupted(slot, card);
                        }
                        continue;
                    }

                    // 2. Structure Runtime Priority (Soft Magnetic Snap)
                    Vector3 targetPos = transform.position + slot.LocalOffset;
                    targetPos.y = transform.position.y + 0.1f; // Slight elevation
                    
                    card.transform.position = Vector3.Lerp(card.transform.position, targetPos, Time.deltaTime * SnapSpeed);
                }
            }
        }

        protected virtual void OnCardInterrupted(StructureSlot slot, GameCard card)
        {
            card.CurrentLock = CardLockReason.None;
            slot.Clear();
        }

        protected abstract void UpdateGameplayLogic();
    }
}
