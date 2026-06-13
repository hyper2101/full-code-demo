using UnityEngine;
using Mewtations.Systems.Economy;
using System.Collections.Generic;

namespace Mewtations.Cards.Contracts
{
    public class DebtNotice : CardData
    {
        [Header("Debt Details")]
        public int TotalDebt;
        public int RepaidAmount;
        public int DaysRemaining;

        private bool _isCollectionState = false;

        public override bool CanBeSold => false;

        // Normal state: player manually drops currency here
        public void ProcessManualRepayment(ICurrency droppedCurrency)
        {
            if (_isCollectionState) return; // Collection state handles absorption automatically

            // In normal state, denomination rules apply.
            // For simplicity, we assume the system verifies droppedCurrency validity before calling this.
            RepaidAmount += droppedCurrency.RawValue;
            CheckDebtCleared();
        }

        public void OnDayPassed()
        {
            if (_isCollectionState) return;

            DaysRemaining--;
            if (DaysRemaining <= 0)
            {
                EnterFailurePhase();
            }
        }

        private void EnterFailurePhase()
        {
            // Spawn Enforcement Threat (Phase 1)
            Debug.Log("Enforcement Threat spawned!");
            // If player loses threat combat, we call EnterCollectionState()
        }

        public void EnterCollectionState()
        {
            if (_isCollectionState) return;

            _isCollectionState = true;
            int remainingDebt = TotalDebt - RepaidAmount;
            
            // 150% penalty on remaining debt
            int penaltyDebt = Mathf.CeilToInt(remainingDebt * 1.5f);
            TotalDebt = RepaidAmount + penaltyDebt;

            // Change visual state
            Debug.Log("DebtNotice transformed into Active Collection State! 150% penalty applied.");
            
            // Hook into currency spawn event
            // WorldManager.OnCurrencySpawned += HandleForcedLiquidation;
        }

        // Hooked to OnCurrencySpawned
        public void HandleForcedLiquidation(ICurrency newlySpawnedCurrency)
        {
            if (!_isCollectionState) return;

            // Forced Liquidation: Ignored denomination hierarchy, convert by raw value
            RepaidAmount += newlySpawnedCurrency.RawValue;
            Debug.Log($"Absorbed {newlySpawnedCurrency.RawValue} value. Remaining: {TotalDebt - RepaidAmount}");

            // Over-absorb without refund: We destroy the currency object.
            // Destroy(newlySpawnedCurrency.gameObject);

            CheckDebtCleared();
        }

        private void CheckDebtCleared()
        {
            if (RepaidAmount >= TotalDebt)
            {
                // Debt cleared
                // Unhook events
                // WorldManager.OnCurrencySpawned -= HandleForcedLiquidation;
                
                // Notify TreasuryLoanHandler to start cooldown
                // Destroy(gameObject);
                Debug.Log("Debt Cleared!");
            }
        }
    }
}
