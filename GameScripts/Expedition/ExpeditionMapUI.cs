using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Expedition
{
    public class ExpeditionMapUI : MonoBehaviour
    {
        public static ExpeditionMapUI Instance { get; private set; }

        private bool _isVisible = false;

        // Visual Styling
        private GUIStyle _panelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _labelStyle;
        
        // Node styles
        private GUIStyle _nodeLockedStyle;
        private GUIStyle _nodeAvailableStyle;
        private GUIStyle _nodeVisitedStyle;
        
        private GUIStyle _backpackPanelStyle;
        private GUIStyle _buttonStyle;

        private void Awake()
        {
            Instance = this;
        }

        public void ShowWindow()
        {
            _isVisible = true;
        }

        public void HideWindow()
        {
            _isVisible = false;
        }

        private void InitializeStyles()
        {
            if (_panelStyle != null) return;

            Texture2D darkTranslucent = CreateColorTexture(new Color(0.08f, 0.08f, 0.12f, 0.95f));
            Texture2D lighterTranslucent = CreateColorTexture(new Color(0.16f, 0.16f, 0.22f, 0.90f));
            
            Texture2D nodeLockedBg = CreateColorTexture(new Color(0.12f, 0.12f, 0.15f, 0.50f));
            Texture2D nodeAvailableBg = CreateColorTexture(new Color(0.75f, 0.60f, 0.20f, 0.90f)); // Bright Gold
            Texture2D nodeVisitedBg = CreateColorTexture(new Color(0.25f, 0.25f, 0.30f, 0.70f)); // Greyish

            Texture2D btnBg = CreateColorTexture(new Color(0.6f, 0.2f, 0.2f, 0.9f));

            _panelStyle = new GUIStyle();
            _panelStyle.normal.background = darkTranslucent;
            _panelStyle.padding = new RectOffset(25, 25, 25, 25);

            _backpackPanelStyle = new GUIStyle();
            _backpackPanelStyle.normal.background = lighterTranslucent;
            _backpackPanelStyle.padding = new RectOffset(15, 15, 15, 15);

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 24;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = new Color(0.95f, 0.85f, 0.60f); // Warm Gold
            _headerStyle.alignment = TextAnchor.MiddleCenter;

            _subHeaderStyle = new GUIStyle();
            _subHeaderStyle.fontSize = 16;
            _subHeaderStyle.fontStyle = FontStyle.Bold;
            _subHeaderStyle.normal.textColor = new Color(0.80f, 0.80f, 0.85f);
            _subHeaderStyle.alignment = TextAnchor.MiddleLeft;

            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 13;
            _labelStyle.normal.textColor = new Color(0.90f, 0.90f, 0.95f);
            _labelStyle.wordWrap = true;
            _labelStyle.richText = true;

            // Nodes
            _nodeLockedStyle = new GUIStyle();
            _nodeLockedStyle.normal.background = nodeLockedBg;
            _nodeLockedStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            _nodeLockedStyle.alignment = TextAnchor.MiddleCenter;
            _nodeLockedStyle.fontSize = 13;
            _nodeLockedStyle.margin = new RectOffset(5, 5, 5, 5);

            _nodeAvailableStyle = new GUIStyle();
            _nodeAvailableStyle.normal.background = nodeAvailableBg;
            _nodeAvailableStyle.normal.textColor = Color.white;
            _nodeAvailableStyle.alignment = TextAnchor.MiddleCenter;
            _nodeAvailableStyle.fontSize = 13;
            _nodeAvailableStyle.fontStyle = FontStyle.Bold;
            _nodeAvailableStyle.margin = new RectOffset(5, 5, 5, 5);

            _nodeVisitedStyle = new GUIStyle();
            _nodeVisitedStyle.normal.background = nodeVisitedBg;
            _nodeVisitedStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _nodeVisitedStyle.alignment = TextAnchor.MiddleCenter;
            _nodeVisitedStyle.fontSize = 13;
            _nodeVisitedStyle.margin = new RectOffset(5, 5, 5, 5);

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = btnBg;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 14;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;
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
            if (!_isVisible || !ExpeditionManager.Instance.IsExpeditionActive) return;

            InitializeStyles();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Rect overlayRect = new Rect(0, 0, screenWidth, screenHeight);
            GUILayout.BeginArea(overlayRect, _panelStyle);

            // 1. Header
            GUILayout.Label("CHIẾN LỘ VIỄN CHINH — EXPEDITION MAP", _headerStyle);
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            // ================== LEFT COLUMN: MAP NODES TREE ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.55f));
            GUILayout.Label("<color=#ffaa00>BẢN ĐỒ VIỄN CHINH (SLAY THE SPIRE)</color>", _subHeaderStyle);
            GUILayout.Space(10);

            // Group nodes by Layer descending (Boss on top, layer 0 at bottom)
            var nodes = ExpeditionManager.Instance.MapNodes;
            int maxLayer = nodes.Max(n => n.Layer);

            for (int layer = maxLayer; layer >= 0; layer--)
            {
                var layerNodes = nodes.Where(n => n.Layer == layer).OrderBy(n => n.Position).ToList();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>TẦNG {layer}:</b>", _labelStyle, GUILayout.Width(75));
                GUILayout.FlexibleSpace();

                foreach (var node in layerNodes)
                {
                    string nodeText = GetNodeLabel(node);
                    
                    // Decide Style
                    GUIStyle style = _nodeLockedStyle;
                    if (node.State == NodeState.Visited) style = _nodeVisitedStyle;
                    else if (node.State == NodeState.Available) style = _nodeAvailableStyle;

                    bool isClickable = node.State == NodeState.Available;
                    
                    if (isClickable)
                    {
                        if (GUILayout.Button(nodeText, style, GUILayout.Width(150), GUILayout.Height(50)))
                        {
                            ExpeditionManager.Instance.EnterNode(node);
                        }
                    }
                    else
                    {
                        GUILayout.Box(nodeText, style, GUILayout.Width(150), GUILayout.Height(50));
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // ================== RIGHT COLUMN: SQUAD STATUS & BACKPACK ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.38f));

            // SQUAD STATUS PANEL
            GUILayout.BeginVertical(_backpackPanelStyle);
            GUILayout.Label("<color=#7cc>ĐỘI NGŨ THẦN MIÊU VIỄN CHINH</color>", _subHeaderStyle);
            GUILayout.Space(10);

            foreach (var cat in ExpeditionManager.Instance.ActiveCats)
            {
                if (cat == null) continue;
                string roleText = $"<color=#eeaa22>[{cat.Role}]</color>";
                string elementText = cat.Element != CatElement.None ? $"<color=#88ccff>[{cat.Element}]</color> " : "";
                string speedText = $"Speed: {cat.Speed}";
                
                GUILayout.Label($"<b>{cat.Name}</b> {roleText} {elementText}— HP: {cat.HealthPoints}/{cat.ProcessedCombatStats.MaxHealth} | {speedText}", _labelStyle);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // BACKPACK INVENTORY PANEL
            GUILayout.BeginVertical(_backpackPanelStyle);
            var backpack = ExpeditionManager.Instance.CurrentBackpack;
            string capLabel = $"<color=#ccff88>DUNG TÍCH BALO: {backpack.ContainedCardIds.Count}/{backpack.MaxCapacity}</color>";
            GUILayout.Label(capLabel, _subHeaderStyle);
            GUILayout.Space(10);

            if (backpack.ContainedCardIds.Count == 0)
            {
                GUILayout.Label("<color=#777>[ Balo trống rỗng ]</color>", _labelStyle);
            }
            else
            {
                // Draw 2 column list of items in backpack
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.17f));
                for (int idx = 0; idx < backpack.ContainedCardIds.Count; idx += 2)
                {
                    DrawBackpackItem(backpack, idx);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.17f));
                for (int idx = 1; idx < backpack.ContainedCardIds.Count; idx += 2)
                {
                    DrawBackpackItem(backpack, idx);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(30);

            // ABANDON EXPEDITION BUTTON
            if (GUILayout.Button("🏳 RÚT LUI KHỎI VIỄN CHINH (MẤT LOOT)", _buttonStyle, GUILayout.Height(50)))
            {
                ExpeditionManager.Instance.ReturnToBase(isDefeat: true);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawBackpackItem(Backpack backpack, int index)
        {
            string itemId = backpack.ContainedCardIds[index];
            string displayName = itemId.Replace("resource_", "").Replace("item_", "").Replace("_", " ");
            displayName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(displayName);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"• {displayName}", _labelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20)))
            {
                backpack.RemoveItemAt(index);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private string GetNodeLabel(ExpeditionNode node)
        {
            string prefix = "";
            switch (node.Type)
            {
                case NodeType.Combat:
                    prefix = "⚔️ Chiến Đấu";
                    break;
                case NodeType.Resource:
                    prefix = "💎 Tài Nguyên";
                    break;
                case NodeType.Event:
                    prefix = "📜 Sự Kiện";
                    break;
                case NodeType.Lore:
                    prefix = "🐾 Điển Tích";
                    break;
                case NodeType.Ruins:
                    prefix = "🏚️ Phế Tích";
                    break;
                case NodeType.Boss:
                    prefix = "💀 BOSS TIẾN TRÌNH";
                    break;
            }

            if (node.State == NodeState.Visited) return $"{prefix}\n[Đã Qua]";
            if (node.State == NodeState.Available) return $"{prefix}\n<color=#fff>[Vào Khám Phá]</color>";
            return $"{prefix}\n[Bị Khóa]";
        }
    }
}
