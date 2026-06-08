using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;

namespace Mewtations.UI.Screens
{
    public class PreCombatScreen : MewtationsScreen
    {
        public static PreCombatScreen Instance { get; private set; }

        [Header("Panels")]
        public EnemyPreviewPanel EnemyPreview;
        public PlayerFormationPanel PlayerFormation;
        public CatReservePanel CatReserve;
        public OrderingPanel OrderingInventory;
        public DetailPanel DetailView;

        public PreCombatSession Session { get; private set; } = new PreCombatSession();

        private void Awake()
        {
            Instance = this;
        }

        public void Setup(EncounterData encounterData)
        {
            Session.Clear();
            Session.Encounter = encounterData;
            
            EnemyPreview.Setup(encounterData);
            
            // Populate Session sandbox from WorldManager
            CatReserve.PopulateSessionCats(Session);
            OrderingInventory.PopulateSessionEquipment(Session);
            
            PlayerFormation.Initialize(Session);
            CatReserve.RefreshAvailableCats(Session);
            OrderingInventory.Initialize(Session);
            
            gameObject.SetActive(true);
        }

        public void OnStartCombatClicked()
        {
            if (!ValidateSelectedCats())
            {
                // Refresh reserve if validation fails (e.g., cat died)
                CatReserve.PopulateSessionCats(Session);
                CatReserve.RefreshAvailableCats(Session);
                return;
            }

            var playerTeam = new List<PlayerUnitSnapshot>(Session.Formation.Values);

            // Create Snapshot
            EncounterSetupSnapshot snapshot = new EncounterSetupSnapshot
            {
                Encounter = Session.Encounter,
                PlayerTeam = playerTeam
            };

            // Transition to combat
            GameScripts.Combat.Core.TurnBasedCombatManager.Instance.StartCombat(snapshot);

            OnCloseClicked(); // Clean up session
        }

        private bool ValidateSelectedCats()
        {
            if (Session == null || Session.Formation.Count == 0)
            {
                Debug.LogWarning("Validation Failed: Cannot start combat with 0 cats.");
                return false;
            }

            if (Session.Formation.Count > 5)
            {
                Debug.LogWarning("Validation Failed: Cannot start combat with more than 5 cats.");
                return false;
            }

            foreach (var kvp in Session.Formation)
            {
                var catSnap = kvp.Value;
                var catData = catSnap.CatReference;
                
                if (catData == null) continue;

                if (!Mewtations.Combat.Core.CombatEligibilityValidator.IsEligible(catData, out string reason))
                {
                    Debug.LogWarning($"[PreCombat] Mèo {catData.CardName} không hợp lệ: {reason}");
                    return false;
                }

                // Check Equipment
                if (catSnap.Equipment != null)
                {
                    foreach (var item in catSnap.Equipment.Slots.Values)
                    {
                        if (item == null || string.IsNullOrEmpty(item.EquipmentId))
                        {
                            Debug.LogWarning($"[PreCombat] Trang bị của {catData.CardName} bị lỗi dữ liệu hoặc không còn tồn tại!");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void OnCloseClicked()
        {
            Session.Clear(); // Discard all sandbox changes
            gameObject.SetActive(false);
        }
    }
}
