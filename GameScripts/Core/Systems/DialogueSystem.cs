using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Dialogue
{
    public class DialogueChoice
    {
        public string Text;
        public string RequirementText;
        public Func<bool> IsAvailable;
        public Action OnSelected;

        public DialogueChoice(string text, Action onSelected, Func<bool> isAvailable = null, string requirementText = "")
        {
            Text = text;
            OnSelected = onSelected;
            IsAvailable = isAvailable;
            RequirementText = requirementText;
        }
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private bool _isVisible = false;
        public bool IsVisible => _isVisible;
        private string _title = "";
        private string _text = "";
        private List<string> _choices = new List<string>();
        private Action<int> _onChoiceSelected;

        private List<DialogueChoice> _branchingChoices = null;

        private bool _isChronicleVisible = false;
        private Vector2 _chronicleScrollPos = Vector2.zero;

        // Visual Styling
        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;

        private void Awake()
        {
            Instance = this;
        }

        public void StartDialogue(string title, string text, List<string> choices, Action<int> onChoiceSelected)
        {
            _title = title;
            _text = text;
            _choices = choices;
            _onChoiceSelected = onChoiceSelected;
            _branchingChoices = null;
            _isVisible = true;

            // Make sure the base game time scale remains frozen
            Time.timeScale = 0f;
        }

        public void StartDialogue(string title, string text, List<DialogueChoice> branchingChoices)
        {
            _title = title;
            _text = text;
            _branchingChoices = branchingChoices;
            _choices = new List<string>();
            _isVisible = true;

            // Make sure the base game time scale remains frozen
            Time.timeScale = 0f;
        }

        public void HideWindow()
        {
            _isVisible = false;
        }

        private void InitializeStyles()
        {
            if (_panelStyle != null) return;

            Texture2D glassBg = CreateColorTexture(new Color(0.10f, 0.10f, 0.14f, 0.95f));
            Texture2D btnBg = CreateColorTexture(new Color(0.24f, 0.24f, 0.32f, 0.90f));

            _panelStyle = new GUIStyle();
            _panelStyle.normal.background = glassBg;
            _panelStyle.padding = new RectOffset(30, 30, 30, 30);

            _titleStyle = new GUIStyle();
            _titleStyle.fontSize = 24;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = new Color(0.95f, 0.85f, 0.60f); // Warm Gold
            _titleStyle.alignment = TextAnchor.MiddleCenter;

            _bodyStyle = new GUIStyle();
            _bodyStyle.fontSize = 15;
            _bodyStyle.normal.textColor = new Color(0.92f, 0.92f, 0.95f);
            _bodyStyle.wordWrap = true;
            _bodyStyle.richText = true;

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = btnBg;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 15;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;
            _buttonStyle.padding = new RectOffset(10, 10, 10, 10);
            _buttonStyle.margin = new RectOffset(0, 0, 10, 10);
        }

        private Texture2D CreateColorTexture(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitializeStyles();

            // 1. Draw "📖 Chronicle of Truth" button when dialogue is not active
            if (!_isVisible)
            {
                Rect btnRect = new Rect(Screen.width - 240, 15, 220, 45);
                string btnText = MewtationsLoc.Translate("btn_chronicle", "📖 Chronicle of Truth");
                
                if (GUI.Button(btnRect, btnText, _buttonStyle))
                {
                    _isChronicleVisible = !_isChronicleVisible;
                    if (_isChronicleVisible)
                    {
                        Time.timeScale = 0f; // Freeze game while viewing Chronicle
                    }
                    else
                    {
                        bool shouldKeepFrozen = (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive) ||
                                               (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null && Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive);
                        if (!shouldKeepFrozen)
                        {
                            Time.timeScale = 1f;
                        }
                    }
                }
            }

            // 2. Draw dialogue popup if active
            if (_isVisible)
            {
                _isChronicleVisible = false; // Auto hide chronicle if dialogue starts
                DrawDialogueWindow();
                return;
            }

            // 3. Draw Chronicle letter vault if active
            if (_isChronicleVisible)
            {
                DrawChronicleWindow();
            }
        }

        private void DrawDialogueWindow()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Center dialog window: 600px width, 450px height
            float winWidth = Mathf.Min(600, screenWidth * 0.8f);
            float winHeight = Mathf.Min(450, screenHeight * 0.7f);
            float posX = (screenWidth - winWidth) / 2;
            float posY = (screenHeight - winHeight) / 2;

            Rect dialogRect = new Rect(posX, posY, winWidth, winHeight);

            GUILayout.BeginArea(dialogRect, _panelStyle);

            // 1. Title
            GUILayout.Label(_title, _titleStyle);
            GUILayout.Space(20);

            // 2. Body Text (Scrollable if too long)
            GUILayout.BeginVertical(GUILayout.Height(winHeight - 160));
            GUILayout.Label(_text, _bodyStyle);
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // 3. Choices
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_branchingChoices != null && _branchingChoices.Count > 0)
            {
                for (int i = 0; i < _branchingChoices.Count; i++)
                {
                    var choice = _branchingChoices[i];
                    bool available = choice.IsAvailable == null || choice.IsAvailable();
                    string buttonText = choice.Text;
                    if (!available && !string.IsNullOrEmpty(choice.RequirementText))
                    {
                        buttonText += $"\n<size=11>({choice.RequirementText})</size>";
                    }

                    Color oldColor = GUI.color;
                    if (!available)
                    {
                        GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Locked/disabled look
                    }

                    if (GUILayout.Button(buttonText, _buttonStyle, GUILayout.Width((winWidth - 80) / _branchingChoices.Count), GUILayout.Height(45)))
                    {
                        if (available)
                        {
                            _isVisible = false;
                            
                            bool shouldKeepFrozen = (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive) ||
                                                   (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null && Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive);
                            if (shouldKeepFrozen)
                            {
                                Time.timeScale = 0f;
                            }
                            else
                            {
                                Time.timeScale = 1f;
                            }
                            
                            choice.OnSelected?.Invoke();
                        }
                    }
                    GUI.color = oldColor;

                    if (i < _branchingChoices.Count - 1)
                    {
                        GUILayout.Space(15);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _choices.Count; i++)
                {
                    if (GUILayout.Button(_choices[i], _buttonStyle, GUILayout.Width((winWidth - 80) / _choices.Count), GUILayout.Height(45)))
                    {
                        _isVisible = false;
                        
                        bool shouldKeepFrozen = (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive) ||
                                               (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null && Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive);
                        if (shouldKeepFrozen)
                        {
                            Time.timeScale = 0f;
                        }
                        else
                        {
                            Time.timeScale = 1f;
                        }
                        
                        _onChoiceSelected?.Invoke(i);
                    }
                    if (i < _choices.Count - 1)
                    {
                        GUILayout.Space(15);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawChronicleWindow()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Centered panel: 800x500
            float winWidth = Mathf.Min(800, screenWidth * 0.9f);
            float winHeight = Mathf.Min(500, screenHeight * 0.8f);
            float posX = (screenWidth - winWidth) / 2;
            float posY = (screenHeight - winHeight) / 2;

            Rect winRect = new Rect(posX, posY, winWidth, winHeight);

            GUILayout.BeginArea(winRect, _panelStyle);

            // Title & Header
            GUILayout.Label(MewtationsLoc.Translate("win_chronicle_title", "📖 CHRONICLE OF TRUTH"), _titleStyle);
            GUILayout.Space(5);
            
            GUIStyle descStyle = new GUIStyle(_bodyStyle);
            descStyle.alignment = TextAnchor.MiddleCenter;
            descStyle.fontSize = 13;
            descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.8f);
            GUILayout.Label(MewtationsLoc.Translate("win_chronicle_desc"), descStyle);
            GUILayout.Space(15);

            // Scrollview of fragments
            _chronicleScrollPos = GUILayout.BeginScrollView(_chronicleScrollPos, GUILayout.Height(winHeight - 160));

            for (int i = 1; i <= 3; i++)
            {
                string hintId = $"item_secret_lore_hint_{i}";
                bool unlocked = ChronicleManager.IsHintUnlocked(hintId);

                GUILayout.BeginVertical(GUI.skin.box);
                
                GUILayout.BeginHorizontal();
                if (unlocked)
                {
                    string hintTitle = MewtationsLoc.Translate($"hint_{i}_title", $"Fragment {i}");
                    GUILayout.Label($"<b><color=#ffcc00>{hintTitle}</color></b>", _bodyStyle);
                    
                    GUILayout.FlexibleSpace();
                    
                    if (GUILayout.Button(MewtationsLoc.Translate("btn_read", "Read"), _buttonStyle, GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        // Open dialogue to read
                        _isChronicleVisible = false;
                        string title = MewtationsLoc.Translate($"hint_{i}_title");
                        string body = MewtationsLoc.Translate($"hint_{i}_body");
                        StartDialogue(title, body, new List<string> { MewtationsLoc.Translate("btn_close", "Close") }, (choiceIdx) => {
                            // Re-open chronicle when closed
                            _isChronicleVisible = true;
                            Time.timeScale = 0f;
                        });
                    }
                }
                else
                {
                    string lockedText = MewtationsLoc.Translate("lbl_lost_fragment");
                    GUILayout.Label($"<color=grey>{lockedText} (Mảnh {i})</color>", _bodyStyle);
                }
                GUILayout.EndHorizontal();

                // Recipe Status
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<size=12>{MewtationsLoc.Translate("lbl_recipe")}</size>", _bodyStyle, GUILayout.Width(140));
                if (unlocked)
                {
                    GUILayout.Label($"<b><color=green>{MewtationsLoc.Translate("lbl_unlocked")}</color></b>", _bodyStyle);
                }
                else
                {
                    GUILayout.Label($"<color=red>{MewtationsLoc.Translate("lbl_locked")}</color>", _bodyStyle);
                }
                GUILayout.EndHorizontal();

                if (unlocked)
                {
                    GUILayout.Space(4);
                    string details = MewtationsLoc.Translate($"recipe_{i}_details");
                    GUIStyle recipeDetailsStyle = new GUIStyle(_bodyStyle);
                    recipeDetailsStyle.fontSize = 13;
                    recipeDetailsStyle.normal.textColor = new Color(0.6f, 0.9f, 0.6f); // Soft green
                    GUILayout.Label(details, recipeDetailsStyle);
                }

                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(10);

            // Close button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(MewtationsLoc.Translate("btn_close", "Close"), _buttonStyle, GUILayout.Width(150), GUILayout.Height(40)))
            {
                _isChronicleVisible = false;
                bool shouldKeepFrozen = (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive) ||
                                       (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null && Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive);
                if (!shouldKeepFrozen)
                {
                    Time.timeScale = 1f;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
