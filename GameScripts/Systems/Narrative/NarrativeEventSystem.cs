using UnityEngine;
using System;
using UI.Panels;

namespace Systems.Narrative
{
    public class NarrativeEventSystem : MonoBehaviour
    {
        public static NarrativeEventSystem Instance { get; private set; }

        public NarrativeEventOverlayUI overlayUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void TriggerEvent(NarrativeEventData eventData)
        {
            // Pause the game lightly
            Time.timeScale = 0f;
            
            // Open the Narrative Event Overlay
            if (overlayUI != null)
            {
                overlayUI.OpenEvent(eventData);
            }
            else
            {
                Debug.LogError("NarrativeEventOverlayUI is not assigned!");
            }
        }

        public void OnEventResolved(string resultActionId)
        {
            // Unpause
            Time.timeScale = 1f;

            Debug.Log($"[NarrativeEventSystem] Event Resolved with Action: {resultActionId}");

            // Dispatch to other systems (Combat Encounter, Resources, etc)
            // Example flow based on result:
            // "resolve_dogtax_pay" -> deduct resources
            // "trigger_dogtax_combat" -> hand off to combat system
            if (resultActionId == "trigger_dogtax_combat")
            {
                Debug.Log("Player refused. Triggering Combat Encounter System...");
                // e.g. CombatEncounterSystem.Instance.StartEncounter("dog_mafia_wave_1");
            }
            else if (resultActionId == "resolve_dogtax_pay")
            {
                Debug.Log("Player paid. Deducting resources...");
                // e.g. ResourceManager.Instance.Deduct("Gold", 50);
            }
        }
    }
}
