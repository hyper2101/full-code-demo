using UnityEngine;
using TMPro;

public class DetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    // Called when right-clicking a cat in Formation or Reserve
    public void ShowCatDetails(CatCardData catData, PreCombatSession session)
    {
        gameObject.SetActive(true);
        titleText.text = catData.CardName; // Name
        
        string level = $"Level: {catData.CombatLevel}";
        string hp = $"HP: {catData.HealthPoints}/{catData.MaxHealthPoints}";
        string atk = $"ATK: {catData.Damage}";
        string def = $"DEF: {catData.Defense}"; // Assuming CatCardData has Defense
        string spd = $"SPD: {catData.AttackSpeed}"; // Assuming CatCardData has Speed/AttackSpeed
        string skills = $"Skills: {(catData.SkillId != null ? catData.SkillId : "None")}";
        
        // Lookup sandbox equipment
        string equipmentStr = "None";
        if (session != null)
        {
            var pSnap = session.Formation.Values.FirstOrDefault(f => f.CatReference == catData);
            if (pSnap != null && pSnap.Equipment != null && pSnap.Equipment.Slots.Count > 0)
            {
                equipmentStr = string.Join(", ", pSnap.Equipment.Slots.Values.Select(v => v.CardName));
            }
        }

        string equipment = $"Equipment: {equipmentStr}";

        statsText.text = $"{level}\n{hp}\n{atk}\n{def}\n{spd}";
        descriptionText.text = $"{skills}\n{equipment}";
    }

    // Called when right-clicking an enemy in Preview
    public void ShowEnemyDetails(GameScripts.Systems.Enemies.DogEnemyInstance enemyInst)
    {
        gameObject.SetActive(true);
        titleText.text = enemyInst.Definition.Id; // Could use loc key
        
        string lvl = $"Level: {enemyInst.Level}";
        string hp = $"HP: {enemyInst.HP}/{enemyInst.MaxHP}";
        string atk = $"ATK: {enemyInst.ATK}";
        string def = $"DEF: {enemyInst.DEF}";
        string spd = $"SPD: {enemyInst.SPD}";
        string skills = $"Skills: {(enemyInst.Definition.ActiveCombatSkill != null ? enemyInst.Definition.ActiveCombatSkill.Id : "None")}";
        string equipment = $"Equipment: None";

        statsText.text = $"{lvl}\n{hp}\n{atk}\n{def}\n{spd}";
        descriptionText.text = $"{skills}\n{equipment}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
