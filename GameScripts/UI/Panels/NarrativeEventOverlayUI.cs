using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Systems.Narrative;

namespace UI.Panels
{
    public class NarrativeEventOverlayUI : MonoBehaviour
    {
        [Header("Overlay & Background")]
        public CanvasGroup overlayCanvasGroup; // Used for Dark Overlay (40% opacity) fade in

        [Header("Portraits")]
        public Image portraitLeft;
        public Image portraitRight;

        [Header("Dialogue Box")]
        public Text speakerNameText;
        public Text dialogueBodyText;

        [Header("Choices")]
        public RectTransform choicesContainer;
        public GameObject choiceButtonPrefab;

        private NarrativeEventData currentEvent;
        private int currentLineIndex = 0;

        public void OpenEvent(NarrativeEventData eventData)
        {
            currentEvent = eventData;
            currentLineIndex = 0;
            gameObject.SetActive(true);
            
            // Fade in overlay
            StartCoroutine(FadeInOverlay());

            // Initialize Portraits (Assuming static images loaded via Resources/Addressables in real impl)
            // portraitLeft.sprite = ...
            // portraitRight.sprite = ...
            
            // Very light scale/fade for portraits
            portraitLeft.transform.localScale = Vector3.one * 0.95f;
            portraitRight.transform.localScale = Vector3.one * 0.95f;

            DisplayNextLine();
        }

        private IEnumerator FadeInOverlay()
        {
            overlayCanvasGroup.alpha = 0;
            float duration = 0.3f;
            float t = 0;
            while(t < duration)
            {
                t += Time.unscaledDeltaTime; // Use unscaled since game might be paused
                overlayCanvasGroup.alpha = Mathf.Lerp(0, 0.4f, t / duration);
                yield return null;
            }
        }

        private void DisplayNextLine()
        {
            if (currentEvent == null || currentLineIndex >= currentEvent.Lines.Count)
            {
                ShowChoices();
                return;
            }

            var line = currentEvent.Lines[currentLineIndex];
            
            // Localization placeholder. Should fetch from TSV via LocManager
            speakerNameText.text = line.SpeakerId; // Or localized speaker name
            dialogueBodyText.text = GetLocalizedText(line.TextKey);

            // Light shake/scale for the active portrait
            AnimateActivePortrait(line.SpeakerId);

            currentLineIndex++;
        }

        private void AnimateActivePortrait(string speakerId)
        {
            // Simple logic: if speaker is player faction -> left, else right
            // For prototype, we just do a tiny pulse
            Image activePortrait = (speakerId == "player" || speakerId == currentEvent.PortraitLeftID) ? portraitLeft : portraitRight;
            StartCoroutine(PulsePortrait(activePortrait.transform));
        }

        private IEnumerator PulsePortrait(Transform t)
        {
            float duration = 0.15f;
            t.localScale = Vector3.one * 1.02f;
            yield return new WaitForSecondsRealtime(duration);
            t.localScale = Vector3.one * 0.95f;
        }

        private void ShowChoices()
        {
            // Clear existing
            foreach(Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach(var choice in currentEvent.Choices)
            {
                var go = Instantiate(choiceButtonPrefab, choicesContainer);
                var textComp = go.GetComponentInChildren<Text>();
                textComp.text = GetLocalizedText(choice.TextKey);
                
                var btn = go.GetComponent<Button>();
                string resultId = choice.ResultActionId;
                btn.onClick.AddListener(() => OnChoiceSelected(resultId));
            }
        }

        private void OnChoiceSelected(string resultActionId)
        {
            // Pass the result back to the system
            NarrativeEventSystem.Instance.OnEventResolved(resultActionId);
            gameObject.SetActive(false); // Close overlay
        }

        // --- Helper for TSV Loc ---
        private string GetLocalizedText(string key)
        {
            // Prototype fallback: In the real codebase, this would call MewtationsTerms or the TSV manager.
            // For now, if it's "dogtax_pay", we'd return the actual translation. We'll just return the key in UI.
            return "TSV_KEY: " + key; 
        }

        // Advance dialogue on click
        public void OnDialogueClicked()
        {
            if (currentLineIndex < currentEvent.Lines.Count)
            {
                DisplayNextLine();
            }
        }
    }
}
