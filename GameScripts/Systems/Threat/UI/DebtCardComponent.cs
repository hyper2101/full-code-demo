using UnityEngine;
using System.Collections.Generic;

namespace GameScripts.Systems.Threat.UI
{
    // Thành phần được gắn thêm vào Prefab CardData gốc
    [RequireComponent(typeof(CardData))]
    public class DebtCardComponent : MonoBehaviour
    {
        public ThreatInstance ThreatSource;
        private CardData _cardData;

        [Header("Tribute Progress")]
        public List<string> RequiredItems = new List<string>();
        public List<string> CurrentItems = new List<string>();

        private void Awake()
        {
            _cardData = GetComponent<CardData>();
        }

        public void Initialize(ThreatInstance threatSource)
        {
            ThreatSource = threatSource;
            // TODO: Populate RequiredItems from threatSource.RequiredTributeItems
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

        public void UpdateDebtLogic()
        {
            if (_cardData.MyGameCard.HasChild)
            {
                var childCard = _cardData.MyGameCard.Child;
                if (childCard != null && CanAcceptCard(childCard.CardData))
                {
                    // Hấp thụ card
                    CurrentItems.Add(childCard.CardData.Id);
                    childCard.DestroyCard(true, true);
                    
                    CheckTributeComplete();
                }
            }
        }

        private void CheckTributeComplete()
        {
            if (CurrentItems.Count >= RequiredItems.Count)
            {
                Debug.Log($"Debt Complete! Gửi thông điệp tới ThreatManager");
                if (GameScripts.Systems.Threat.ThreatManager.Instance != null && ThreatSource != null)
                {
                    GameScripts.Systems.Threat.ThreatManager.Instance.ResolveThreat(ThreatSource, true);
                    // Dọn dẹp Card Cống nạp sau khi xong
                    _cardData.MyGameCard.DestroyCard(true, true);
                }
            }
        }
    }
}
