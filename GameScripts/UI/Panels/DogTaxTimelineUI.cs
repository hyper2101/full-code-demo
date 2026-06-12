using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UI.Panels
{
    public class DogTaxTimelineUI : MonoBehaviour
    {
        [Header("Timeline Nodes")]
        public Transform[] dayNodes; // Positions or UI elements for ○
        public RectTransform dogIcon; // 🐶 Icon

        [Header("Moon Progress")]
        public Image moonFillImage;

        [Header("Escalation Effects")]
        public Color normalColor = Color.white;
        public Color warningColor = new Color(1f, 0.5f, 0f);
        public Color dangerColor = Color.red;

        private Coroutine pulseCoroutine;
        private Coroutine shakeCoroutine;

        private void Start()
        {
            // Initial setup
            if (dogIcon != null)
                dogIcon.localScale = Vector3.one;
        }

        public void UpdateMoonProgress(float progress)
        {
            if (moonFillImage != null)
            {
                moonFillImage.fillAmount = progress; // 0 to 1
            }
        }

        public void AdvanceDay(int currentDayIndex, int daysUntilTax)
        {
            // 1. Moon complete animation (flash or reset)
            UpdateMoonProgress(1f);
            StartCoroutine(MoonResetSequence());

            // 2. Move Dog Icon to the new node
            if (dayNodes != null && currentDayIndex >= 0 && currentDayIndex < dayNodes.Length)
            {
                StartCoroutine(MoveDogIcon(dayNodes[currentDayIndex].position));
            }

            // 3. Escalation
            ApplyEscalation(daysUntilTax);
        }

        private IEnumerator MoonResetSequence()
        {
            // Pause lightly at full moon
            yield return new WaitForSeconds(0.2f);
            UpdateMoonProgress(0f);
        }

        private IEnumerator MoveDogIcon(Vector3 targetPosition)
        {
            if (dogIcon == null) yield break;

            // Scale up slightly
            dogIcon.localScale = Vector3.one * 1.15f;
            
            // Move
            float duration = 0.35f;
            float t = 0;
            Vector3 startPos = dogIcon.position;
            
            while(t < duration)
            {
                t += Time.deltaTime;
                dogIcon.position = Vector3.Lerp(startPos, targetPosition, t / duration);
                // Slight shake during movement
                dogIcon.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 50f) * 5f);
                yield return null;
            }

            dogIcon.position = targetPosition;
            dogIcon.localRotation = Quaternion.identity;

            // Scale back down
            dogIcon.localScale = Vector3.one;
        }

        private void ApplyEscalation(int daysUntilTax)
        {
            if (dogIcon == null) return;

            StopEscalationEffects();

            if (daysUntilTax == 3)
            {
                // Pulse nhẹ
                pulseCoroutine = StartCoroutine(PulseIcon(1.05f, 1f));
                dogIcon.GetComponent<Image>().color = normalColor;
            }
            else if (daysUntilTax == 2)
            {
                // Đổi màu nhẹ
                dogIcon.GetComponent<Image>().color = warningColor;
            }
            else if (daysUntilTax == 1 || daysUntilTax == 0)
            {
                // Rung + glow đỏ nhẹ
                dogIcon.GetComponent<Image>().color = dangerColor;
                shakeCoroutine = StartCoroutine(ShakeIcon());
                
                // Note: Tooltip "Dog Tax Tomorrow" would be hooked up via standard tooltip system here
            }
            else
            {
                dogIcon.GetComponent<Image>().color = normalColor;
            }
        }

        private void StopEscalationEffects()
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            dogIcon.localScale = Vector3.one;
            dogIcon.localPosition = new Vector3(dogIcon.localPosition.x, dogIcon.localPosition.y, 0);
        }

        private IEnumerator PulseIcon(float scaleMax, float speed)
        {
            while (true)
            {
                float scale = 1f + Mathf.PingPong(Time.time * speed, scaleMax - 1f);
                dogIcon.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        private IEnumerator ShakeIcon()
        {
            Vector3 origPos = dogIcon.localPosition;
            while (true)
            {
                dogIcon.localPosition = origPos + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
