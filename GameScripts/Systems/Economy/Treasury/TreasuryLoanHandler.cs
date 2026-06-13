using UnityEngine;

namespace Mewtations.UI.Treasury
{
    public class TreasuryLoanHandler : MonoBehaviour
    {
        private bool _hasActiveLoan = false;
        private int _loanCooldownDays = 0;

        public void UpdateCooldown()
        {
            if (_loanCooldownDays > 0)
                _loanCooldownDays--;
        }

        public void OnDebtRepaid()
        {
            _hasActiveLoan = false;
            _loanCooldownDays = Random.Range(5, 11); // 5-10 days cooldown
        }

        public void ProcessLoan(int currentDay)
        {
            if (_hasActiveLoan)
            {
                Debug.LogWarning("Only one active loan is allowed.");
                return;
            }

            if (_loanCooldownDays > 0)
            {
                Debug.LogWarning($"Loan system is on cooldown for {_loanCooldownDays} days.");
                return;
            }

            // Fixed Progression Loan calculation
            int loanRawValue = 0;
            if (currentDay <= 10) loanRawValue = 5; // 1 Spirit Stone
            else if (currentDay <= 20) loanRawValue = 20; // 1 Refined
            else if (currentDay <= 35) loanRawValue = 40; // 2 Refined
            else loanRawValue = 50; // 1 Pure

            // 120% Repayment
            int repaymentValue = Mathf.CeilToInt(loanRawValue * 1.2f);

            _hasActiveLoan = true;
            // Spawn currency equivalent to loanRawValue
            // Spawn DebtNotice configured with repaymentValue
            Debug.Log($"Loan granted: {loanRawValue}. Owe: {repaymentValue}");
        }
    }
}
