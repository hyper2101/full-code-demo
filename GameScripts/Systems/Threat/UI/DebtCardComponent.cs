using UnityEngine;
using System.Collections.Generic;

namespace GameScripts.Systems.Threat.UI
{
    // Thành phần được gắn thêm vào Prefab CardData gốc
    [RequireComponent(typeof(CardData))]
    public class DebtCardComponent : MonoBehaviour
    {
        public System.Action OnDebtResolved;
        private CardData _cardData;

        [Header("Tribute Progress")]
        public List<string> RequiredItems = new List<string>();
        public List<string> CurrentItems = new List<string>();

        private void Awake()
        {
            _cardData = GetComponent<CardData>();
        }

        public void Initialize(List<string> requiredItems, System.Action onResolvedCallback)
        {
            RequiredItems.Clear();
            RequiredItems.AddRange(requiredItems);
            OnDebtResolved = onResolvedCallback;
        }

        public bool CanAcceptCard(CardData otherCard)
        {
            // Chỉ nhận đồ nếu trùng với RequiredItems mà chưa có trong CurrentItems
            string id = otherCard.Id;
            if (RequiredItems.Contains(id))
            {
                int requiredCount = RequiredItems.FindAll(x => x == id).Count;
                int currentCount = CurrentItems.FindAll(x => x == id).Count;
                return currentCount < requiredCount;
            }
            return false;
        }

        private bool _isConsuming = false;

        public void UpdateDebtLogic()
        {
            if (_isConsuming) return;
            
            if (_cardData.MyGameCard.HasChild)
            {
                var childCard = _cardData.MyGameCard.Child;
                if (childCard != null && CanAcceptCard(childCard.CardData))
                {
                    _isConsuming = true;
                    // Hấp thụ card
                    CurrentItems.Add(childCard.CardData.Id);
                    childCard.DestroyCard(true, true);
                    
                    CheckTributeComplete();
                    _isConsuming = false;
                }
            }
        }

        private void CheckTributeComplete()
        {
            if (CurrentItems.Count >= RequiredItems.Count)
            {
                Debug.Log($"[DebtCard] Debt Complete!");
                
                if (OnDebtResolved != null)
                {
                    OnDebtResolved.Invoke();
                }
                
                // Dọn dẹp Card Cống nạp sau khi xong
                if (_cardData != null && _cardData.MyGameCard != null)
                {
                    _cardData.MyGameCard.DestroyCard(true, true);
                }
            }
        }
    }
}
