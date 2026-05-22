using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private bool _isVisible = false;
        private string _title = "";
        private string _text = "";
        private List<string> _choices = new List<string>();
        private Action<int> _onChoiceSelected;

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
            if (!_isVisible) return;

            InitializeStyles();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Center dialog window: 600px width, 400px height
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
            for (int i = 0; i < _choices.Count; i++)
            {
                if (GUILayout.Button(_choices[i], _buttonStyle, GUILayout.Width((winWidth - 80) / _choices.Count), GUILayout.Height(45)))
                {
                    _isVisible = false;
                    
                    bool shouldKeepFrozen = (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive) ||
                                           (Mewtations.Combat.TurnBasedCombatManager.Instance != null && Mewtations.Combat.TurnBasedCombatManager.Instance.IsCombatActive);
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
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
