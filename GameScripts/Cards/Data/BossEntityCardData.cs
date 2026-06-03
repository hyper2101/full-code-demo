using System.Collections.Generic;
using UnityEngine;
using Mewtations.Dialogue;

public class BossEntityCardData : Mob
{
    [Header("Boss Settings")]
    public string BossSpawnDialogueKey;
    public string BossDeathDialogueKey;

    public override void OnInitialCreate()
    {
        base.OnInitialCreate();
        TriggerSpawnDialogue();
    }

    public override void Die()
    {
        TriggerDeathDialogue();
        base.Die();
    }

    private void TriggerSpawnDialogue()
    {
        if (string.IsNullOrEmpty(BossSpawnDialogueKey)) return;

        if (DialogueSystem.Instance != null)
        {
            string title = MewtationsLoc.Translate(this.NameTerm, this.Name);
            string text = MewtationsLoc.Translate(BossSpawnDialogueKey, "...");
            
            DialogueSystem.Instance.StartDialogue(
                title, 
                text, 
                new List<string> { MewtationsLoc.Translate("btn_boss_spawn_ack", "Chuẩn bị chiến đấu!") }, 
                (idx) => {}
            );
        }
    }

    private void TriggerDeathDialogue()
    {
        if (string.IsNullOrEmpty(BossDeathDialogueKey)) return;

        if (DialogueSystem.Instance != null)
        {
            string title = MewtationsLoc.Translate(this.NameTerm, this.Name);
            string text = MewtationsLoc.Translate(BossDeathDialogueKey, "...");
            
            DialogueSystem.Instance.StartDialogue(
                title, 
                text, 
                new List<string> { MewtationsLoc.Translate("btn_boss_death_ack", "Chiến thắng!") }, 
                (idx) => {}
            );
        }
    }
}
