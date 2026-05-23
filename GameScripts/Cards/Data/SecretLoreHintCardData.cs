using System;
using UnityEngine;

public class SecretLoreHintCardData : CardData
{
    private float _lastClickTime = 0f;

    protected override bool CanHaveCard(CardData otherCard)
    {
        return false; // Cannot stack any cards on top of it (unstackable per user instruction)
    }

    public override bool DetermineCanHaveCardsWhenIsRoot
    {
        get
        {
            return false; // Cannot stack as root either
        }
    }

    public override void OnInitialCreate()
    {
        base.OnInitialCreate();
        RegisterUnlock();
    }

    public override void UpdateCard()
    {
        base.UpdateCard();
        RegisterUnlock();

        // Shimmer visual highlight
        if (this.MyGameCard != null)
        {
            this.MyGameCard.HighlightActive = true; 
        }
    }

    private void RegisterUnlock()
    {
        if (!string.IsNullOrEmpty(this.Id))
        {
            ChronicleManager.UnlockHint(this.Id);
        }
    }

    public override void Clicked()
    {
        base.Clicked();

        float timeSinceLastClick = Time.realtimeSinceStartup - _lastClickTime;
        if (timeSinceLastClick < 0.35f && _lastClickTime > 0.01f) // Double-click window (350ms)
        {
            ShowHintDialogue();
            _lastClickTime = 0f; // Reset to prevent consecutive rapid triggers
        }
        else
        {
            _lastClickTime = Time.realtimeSinceStartup;
        }
    }

    private void ShowHintDialogue()
    {
        if (string.IsNullOrEmpty(this.Id)) return;

        string titleTerm = "";
        string bodyTerm = "";

        if (this.Id.Contains("hint_1") || this.Id.EndsWith("_1"))
        {
            titleTerm = "hint_1_title";
            bodyTerm = "hint_1_body";
        }
        else if (this.Id.Contains("hint_2") || this.Id.EndsWith("_2"))
        {
            titleTerm = "hint_2_title";
            bodyTerm = "hint_2_body";
        }
        else if (this.Id.Contains("hint_3") || this.Id.EndsWith("_3"))
        {
            titleTerm = "hint_3_title";
            bodyTerm = "hint_3_body";
        }
        else
        {
            // Fallback generic hint
            titleTerm = "lbl_lost_fragment";
            bodyTerm = "win_chronicle_desc";
        }

        string title = MewtationsLoc.Translate(titleTerm, "Scroll Fragment");
        string body = MewtationsLoc.Translate(bodyTerm, "An ancient writing lies here...");

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(
                title, 
                body, 
                new System.Collections.Generic.List<string> { MewtationsLoc.Translate("btn_close", "Close") }, 
                (choiceIdx) => {}
            );
        }
    }
}
