using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Narrative
{
    [Serializable]
    public class DialogueLine
    {
        public string SpeakerId; // Example: "dog_mafia_t1"
        public string TextKey; // Example: "dogtax_t1_intro_01"
    }

    [Serializable]
    public class DialogueChoice
    {
        public string TextKey; // Example: "dogtax_pay"
        public string ResultActionId; // Example: "resolve_dogtax_pay", "trigger_dogtax_combat"
    }

    [CreateAssetMenu(fileName = "NarrativeEvent", menuName = "Systems/Narrative/Narrative Event")]
    public class NarrativeEventData : ScriptableObject
    {
        public string EventID;
        
        [Header("Presentation")]
        public string PortraitLeftID;
        public string PortraitRightID;
        
        [Header("Dialogue Content")]
        public List<DialogueLine> Lines = new List<DialogueLine>();
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
    }
}
