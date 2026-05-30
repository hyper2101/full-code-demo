using UnityEngine;

namespace GameScripts.Systems.Threat.UI
{
    // Thành phần được gắn thêm vào Prefab CardData gốc
    [RequireComponent(typeof(CardData))]
    public class ThreatCardComponent : MonoBehaviour
    {
        public ThreatInstance InstanceData;
        private CardData _cardData;

        public float HoldProgress = 0f;
        public const float MaxHoldTime = 2.0f;

        private void Awake()
        {
            _cardData = GetComponent<CardData>();
        }

        public void Initialize(ThreatInstance instance)
        {
            InstanceData = instance;
            // TODO: Thiết lập Icon, Tên, Mô tả thông qua _cardData ở Phase 3
        }

        private void Update()
        {
            if (InstanceData == null || InstanceData.State != ThreatState.Active) return;

            bool isHovered = (WorldManager.instance != null && _cardData != null && WorldManager.instance.HoveredCard == _cardData.MyGameCard);
            bool isHolding = isHovered && InputController.instance != null && InputController.instance.GetInput(0);

            if (isHolding)
            {
                HoldProgress += Time.deltaTime;
                if (HoldProgress >= MaxHoldTime)
                {
                    HoldProgress = 0f;
                    TriggerEngagement();
                }
                else if (_cardData != null && _cardData.MyGameCard != null)
                {
                    // Visual Feedback: Rung lắc nhẹ
                    float shakeAmount = 0.03f * (HoldProgress / MaxHoldTime);
                    _cardData.MyGameCard.transform.position += new Vector3(UnityEngine.Random.Range(-shakeAmount, shakeAmount), 0f, UnityEngine.Random.Range(-shakeAmount, shakeAmount));
                    _cardData.MyGameCard.RotWobble(0.2f);
                }
            }
            else
            {
                HoldProgress = Mathf.Max(0f, HoldProgress - Time.deltaTime * 2f);
            }
        }

        private void TriggerEngagement()
        {
            Debug.Log($"Bắt đầu Combat với {InstanceData.BaseData.DisplayName}");
            // TODO: Truyền InstanceData.GeneratedEnemyTeam vào Combat System (Mewtations.Combat.Core.TurnBasedCombatManager)
            // Lấy top 5 mèo hoặc toàn bộ mèo trên board để đánh.
            
            // Xử lý sau trận:
            // Thắng -> GameScripts.Systems.Threat.ThreatManager.Instance.ResolveThreat(InstanceData);
            // Thua -> Mèo bị thương, Threat thêm delay hồi phục.
        }
    }
}
