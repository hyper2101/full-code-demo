using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Legacy.Stacklands;

namespace Mewtations.Expedition
{
    public class ExpeditionRewardUI : MonoBehaviour
    {
        public static ExpeditionRewardUI Instance { get; private set; }

        private bool _isVisible = false;
        private bool _isInventoryOpen = false;
        private bool _confirmSkip = false;
        private List<string> _availableRewards = new List<string>();

        private GUIStyle _panelStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _warningStyle;

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
            _warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.red }
            };
        }

        public void ShowRewards(List<string> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                ExpeditionManager.Instance.CompleteNodeResolution();
                return;
            }
            _availableRewards = new List<string>(rewards);
            _isVisible = true;
            _isInventoryOpen = false;
            _confirmSkip = false;
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
            float height = 550;
            Rect panelRect = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);

            GUILayout.BeginArea(panelRect, _panelStyle);
            GUILayout.Label(Mewtations.Core.MewtationsLoc.Translate("exp_reward_title", "PHẦN THƯỞNG (REWARDS)"), _headerStyle);
            GUILayout.Space(10);

            // Fetch Ordering context for capacity checks
            OrderingCardData ordering = null;
            int currentSlots = 0;
            int maxSlots = 0;
            if (ExpeditionManager.Instance.Context != null && ExpeditionManager.Instance.Context.Ordering != null)
            {
                ordering = ExpeditionManager.Instance.Context.Ordering;
                if (ordering.MyGameCard != null && ordering.MyGameCard.InventoryContainer != null)
                {
                    currentSlots = ordering.MyGameCard.InventoryContainer.GetChildren().Count;
                    maxSlots = ordering.StorageCapacity;
                }
            }

            if (ordering != null)
            {
                string capColor = currentSlots >= maxSlots ? "#ff3333" : "#33cc33";
                GUILayout.Label($"<color={capColor}>Túi Đồ Ordering: {currentSlots} / {maxSlots}</color>", new GUIStyle(_headerStyle) { fontSize = 16, richText = true });
            }
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
                            if (ordering != null)
                            {
                                if (currentSlots < maxSlots)
                                {
                                    // Instantiate the loot card and insert directly into the container
                                    GameCard newCard = WorldManager.instance.CreateCard(new Vector3(-9999, -9999, 0), _availableRewards[i], false, false, true);
                                    if (newCard != null)
                                    {
                                        var context = new ContainerInsertContext { SourceCard = ordering.MyGameCard, ContextSource = "RewardScreen" };
                                        var result = ContainerTransactionSystem.Instance.RequestInsert(newCard, ordering.MyGameCard.InventoryContainer, context);
                                        if (result.Success)
                                        {
                                            newCard.gameObject.SetActive(false);
                                            _availableRewards.RemoveAt(i);
                                            i--; // Adjust index
                                            currentSlots++;
                                            _confirmSkip = false; // Reset confirmation if they took something
                                        }
                                        else
                                        {
                                            newCard.DestroyCard(true, true);
                                        }
                                    }
                                }
                                else
                                {
                                    _isInventoryOpen = true; // Open UI to allow them to discard
                                }
                            }
                            else
                            {
                                // Legacy fallback (for saves loaded without Ordering)
                                var backpack = ExpeditionManager.Instance.CurrentBackpack;
                                if (backpack != null && !backpack.IsFull)
                                {
                                    backpack.AddItem(_availableRewards[i]);
                                    _availableRewards.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            if (_availableRewards.Count > 0)
            {
                if (!_confirmSkip)
                {
                    if (GUILayout.Button(Mewtations.Core.MewtationsLoc.Translate("exp_reward_continue", "TIẾP TỤC (BỎ QUA CÁC PHẦN THƯỞNG CÒN LẠI)"), _buttonStyle, GUILayout.Height(50)))
                    {
                        _confirmSkip = true;
                    }
                }
                else
                {
                    GUILayout.Label("Bạn chắc chắn muốn bỏ lại phần thưởng chưa nhận chứ?", _warningStyle);
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Vâng, kết thúc", _buttonStyle, GUILayout.Height(50)))
                    {
                        _availableRewards.Clear();
                        RewardScreenClosed();
                    }
                    if (GUILayout.Button("Hủy (Quay lại nhận)", _buttonStyle, GUILayout.Height(50)))
                    {
                        _confirmSkip = false;
                    }
                    GUILayout.EndHorizontal();
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