using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;

namespace Mewtations.UI.Screens
{
    public class PreCombatScreen : MewtationsScreen
    {
        [Header("Panels")]
        public EnemyPreviewPanel EnemyPreview;
        public PlayerFormationPanel PlayerFormation;
        public CatReservePanel CatReserve;
        public OrderingPanel OrderingInventory;
        public DetailPanel DetailView;

        private EncounterData currentEncounter;

        public void Setup(EncounterData encounterData)
        {
            currentEncounter = encounterData;
            
            EnemyPreview.Setup(encounterData);
            PlayerFormation.Initialize();
            CatReserve.RefreshAvailableCats();
            OrderingInventory.Initialize();
            
            gameObject.SetActive(true);
        }

        public void OnStartCombatClicked()
        {
            if (!PlayerFormation.HasAnyCatAssigned())
            {
                Debug.LogWarning("Cannot start combat: No cats assigned to formation.");
                return;
            }

            // Create Snapshot
            EncounterSetupSnapshot snapshot = new EncounterSetupSnapshot
            {
                Encounter = currentEncounter,
                PlayerTeam = PlayerFormation.GetPlayerSnapshots()
            };

            // Transition to combat
            GameScripts.Combat.Core.TurnBasedCombatManager.Instance.StartCombat(snapshot);

            gameObject.SetActive(false);
        }

        public void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
