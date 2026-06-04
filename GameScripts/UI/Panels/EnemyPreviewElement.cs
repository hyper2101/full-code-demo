using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Mewtations.UI.Panels
{
    public class EnemyPreviewElement : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;

        public void Init(GameScripts.Systems.Enemies.DogEnemyInstance enemyInst)
        {
            if (enemyInst == null || enemyInst.Definition == null) return;

            if (portraitImage != null)
                portraitImage.sprite = enemyInst.Definition.Portrait;

            if (nameText != null)
                nameText.text = enemyInst.Definition.Id; // Or use Localization for NameKey

            if (levelText != null)
                levelText.text = $"Lv.{enemyInst.Level}";
        }
    }
}
