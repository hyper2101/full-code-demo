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
            
            // Right-click for Preview
            bool isRightClicked = isHovered && InputController.instance != null && InputController.instance.GetInputDown(1);
            if (isRightClicked)
            {
                OpenPreview();
            }
        }

        private void OpenPreview()
        {
            if (InstanceData == null) return;
            
            var encounter = Mewtations.Combat.Encounters.EncounterManager.Instance?.GetEncounter(InstanceData.EncounterId);
            if (encounter == null) return;
            
            Debug.Log($"[ThreatCard] Opening Preview for {encounter.EncounterName} (Context: {encounter.Context})");
            // TODO: Call standard UI Preview manager if one exists
        }

        private void TriggerEngagement()
        {
            if (InstanceData == null) return;
            
            var encounter = Mewtations.Combat.Encounters.EncounterManager.Instance?.GetEncounter(InstanceData.EncounterId);
            if (encounter == null)
            {
                Debug.LogError($"[ThreatCard] Failed to trigger combat: Could not find Encounter {InstanceData.EncounterId} in EncounterManager!");
                return;
            }
            
            Debug.Log($"[ThreatCard] Kích hoạt PreCombat cho {encounter.EncounterName}");
            
            if (Mewtations.UI.Screens.PreCombatScreen.Instance != null)
            {
                Mewtations.UI.Screens.PreCombatScreen.Instance.Setup(encounter);
            }
            else
            {
                Debug.LogError("[ThreatCard] PreCombatScreen.Instance is null! Cannot launch combat setup.");
            }
        }
    }
}
