using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Expedition
{
    public class ExpeditionRewardUI : MonoBehaviour
    {
        
        public static ExpeditionRewardUI Instance { get; private set; }

        private bool _isVisible = false;
        private bool _isInventoryOpen = false;
        private List<string> _availableRewards = new List<string>();

        private GUIStyle _panelStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;

        private void Awake()
        {
            Instance = this;
        }

        private void InitializeStyles()
        {
            if (_panelStyle != null) return;
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.whiteTexture },
                padding = new RectOffset(20, 20, 20, 20)
            };
            _cardStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow }
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }

        public void ShowRewards(List<string> rewards)
        {
            _availableRewards = new List<string>(rewards);
            _isVisible = true;
            _isInventoryOpen = false;
        }

        public void RewardScreenClosed()
        {
            _isVisible = false;
            ExpeditionManager.Instance.CompleteNodeResolution();
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            if (_isInventoryOpen)
            {
                GUI.backgroundColor = new Color(0, 0, 0, 0.9f);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                
                if (Mewtations.Combat.UI.CombatOverlayUI.Instance != null)
                {
                    Mewtations.Combat.UI.CombatOverlayUI.Instance.DrawInventoryExternal();
                }

                InitializeStyles();
                if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 50), Mewtations.Core.MewtationsLoc.Translate("exp_close_inventory", "QUAY LẠI (BACK)"), _buttonStyle))
                {
                    _isInventoryOpen = false;
                }
                GUI.backgroundColor = Color.white;
                return;
            }

            InitializeStyles();

            GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
            float width = 800;
            float height = 500;
            Rect panelRect = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);

            GUILayout.BeginArea(panelRect, _panelStyle);
            GUILayout.Label(Mewtations.Core.MewtationsLoc.Translate("exp_reward_title", "PHẦN THƯỞNG (REWARDS)"), _headerStyle);
            GUILayout.Space(20);

            if (_availableRewards.Count == 0)
            {
                GUILayout.Label("Không còn phần thưởng nào!", _headerStyle);
            }
            else
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < _availableRewards.Count; i++)
                {
                    if (GUILayout.Button(_availableRewards[i], _cardStyle, GUILayout.Width(100), GUILayout.Height(150)))
                    {
                        if (Event.current.button == 0) // Left click
                        {
                            var backpack = ExpeditionManager.Instance.CurrentBackpack;
                            if (backpack != null && !backpack.IsFull)
                            {
                                backpack.AddItem(_availableRewards[i]);
                                _availableRewards.RemoveAt(i);
                                i--; // Adjust index after removal
                            }
                            else
                            {
                                _isInventoryOpen = true;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            
            if (_availableRewards.Count > 0)
            {
                if (GUILayout.Button(Mewtations.Core.MewtationsLoc.Translate("exp_reward_skip", "BỎ QUA TẤT CẢ (SKIP REST)"), _buttonStyle, GUILayout.Height(50)))
                {
                    _availableRewards.Clear();
                }
            }
            else
            {
                if (GUILayout.Button(Mewtations.Core.MewtationsLoc.Translate("exp_reward_continue", "TIẾP TỤC (CONTINUE)"), _buttonStyle, GUILayout.Height(50)))
                {
                    RewardScreenClosed();
                }
            }


            GUILayout.EndArea();
            GUI.backgroundColor = Color.white;
        }

    }
}