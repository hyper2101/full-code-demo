using UnityEngine;
using TMPro;

public class DetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    // Called when right-clicking a cat in Formation or Reserve
    public void ShowCatDetails(string catCardId)
    {
        gameObject.SetActive(true);
        titleText.text = "Cat Stats";
        statsText.text = "Loading stats for " + catCardId + "...";
        descriptionText.text = "Traits and Skills...";
    }

    // Called when right-clicking an enemy in Preview
    public void ShowEnemyDetails(GameScripts.Systems.Enemies.DogEnemyDefinition enemyDef)
    {
        gameObject.SetActive(true);
        titleText.text = enemyDef.Id;
        statsText.text = "Enemy Stats";
        descriptionText.text = "Passives...";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
