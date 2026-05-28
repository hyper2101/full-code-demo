using System;
using System.Collections.Generic;
using UnityEngine;

using Mewtations.Combat.Core;
using Mewtations.Combat.Battlefield;

// TURN-BASED CORE SYSTEM
// DO NOT REMOVE DURING LEGACY COMBAT CLEANUP
namespace Mewtations.Combat.UI
{
    public class CombatOverlayUI : MonoBehaviour
    {
        public static CombatOverlayUI Instance { get; private set; }

        private Vector2 _logScrollPosition = Vector2.zero;
        private float _pulseValue;

        // Custom premium GUI Styles
        private GUIStyle _panelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _logStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _hpBarBackgroundStyle;
        private GUIStyle _hpBarFillStyle;
        private GUIStyle _rageBarFillStyle;
        private GUIStyle _shieldBarFillStyle;
        private GUIStyle _staminaBarFillStyle;
        private GUIStyle _unitCardStyle;

        // Interactive Prep styles
        private GUIStyle _goldButtonStyle;
        private GUIStyle _sidebarCardStyle;
        private GUIStyle _tooltipStyle;

        private bool _isVisible = false;
        private CatCardData _selectedSidebarCat = null;
        private Vector2 _sidebarScrollPos = Vector2.zero;

        // Tooltip hover caching
        private object _hoveredObject = null;
        private GameCard _selectedRingItem = null;
        
        public static GameCard BoardHoveredCat { get; set; }
        public GameCard DraggedRingItem { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowWindow()
        {
            _isVisible = true;
            _selectedSidebarCat = null;
        }

        public void HideWindow()
        {
            _isVisible = false;
            _selectedSidebarCat = null;
        }

        private void InitializeStyles()
        {
            if (_panelStyle != null) return;

            // Translucent Dark Glassmorphic background
            Texture2D darkTranslucent = CreateColorTexture(new Color(0.12f, 0.12f, 0.16f, 0.90f));
            Texture2D lighterTranslucent = CreateColorTexture(new Color(0.20f, 0.20f, 0.25f, 0.95f));
            Texture2D redTranslucent = CreateColorTexture(new Color(0.6f, 0.2f, 0.2f, 0.9f));
            Texture2D hpBgTexture = CreateColorTexture(new Color(0.2f, 0.1f, 0.1f, 1f));
            Texture2D hpFillTexture = CreateColorTexture(new Color(0.2f, 0.7f, 0.3f, 1f));
            Texture2D shieldFillTexture = CreateColorTexture(new Color(0.3f, 0.6f, 0.9f, 1f));
            Texture2D rageFillTexture = CreateColorTexture(new Color(0.9f, 0.6f, 0.1f, 1f));

            _panelStyle = new GUIStyle();
            _panelStyle.normal.background = darkTranslucent;
            _panelStyle.padding = new RectOffset(20, 20, 20, 20);

            _unitCardStyle = new GUIStyle();
            _unitCardStyle.normal.background = lighterTranslucent;
            _unitCardStyle.padding = new RectOffset(10, 10, 10, 10);
            _unitCardStyle.margin = new RectOffset(5, 5, 5, 5);

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 22;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = new Color(0.95f, 0.85f, 0.6f); // Matted Gold
            _headerStyle.alignment = TextAnchor.MiddleCenter;

            _logStyle = new GUIStyle();
            _logStyle.fontSize = 13;
            _logStyle.normal.textColor = new Color(0.9f, 0.9f, 0.95f);
            _logStyle.wordWrap = true;

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = redTranslucent;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 15;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _hpBarBackgroundStyle = new GUIStyle();
            _hpBarBackgroundStyle.normal.background = hpBgTexture;

            _hpBarFillStyle = new GUIStyle();
            _hpBarFillStyle.normal.background = hpFillTexture;

            _shieldBarFillStyle = new GUIStyle();
            _shieldBarFillStyle.normal.background = shieldFillTexture;

            _rageBarFillStyle = new GUIStyle();
            _rageBarFillStyle.normal.background = rageFillTexture;

            Texture2D staminaFillTexture = CreateColorTexture(new Color(0.1f, 0.75f, 0.85f, 1f));
            _staminaBarFillStyle = new GUIStyle();
            _staminaBarFillStyle.normal.background = staminaFillTexture;

            // Matted Gold Fight Button
            Texture2D goldTranslucent = CreateColorTexture(new Color(0.85f, 0.70f, 0.35f, 0.9f));
            _goldButtonStyle = new GUIStyle();
            _goldButtonStyle.normal.background = goldTranslucent;
            _goldButtonStyle.normal.textColor = new Color(0.12f, 0.12f, 0.16f);
            _goldButtonStyle.fontSize = 16;
            _goldButtonStyle.fontStyle = FontStyle.Bold;
            _goldButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Sidebar Background style
            Texture2D sidebarBg = CreateColorTexture(new Color(0.16f, 0.16f, 0.22f, 0.95f));
            _sidebarCardStyle = new GUIStyle();
            _sidebarCardStyle.normal.background = sidebarBg;
            _sidebarCardStyle.normal.textColor = Color.white;
            _sidebarCardStyle.padding = new RectOffset(8, 8, 8, 8);
            _sidebarCardStyle.margin = new RectOffset(3, 3, 3, 3);

            // Floating Tooltip style
            Texture2D tooltipBg = CreateColorTexture(new Color(0.08f, 0.08f, 0.12f, 0.98f));
            _tooltipStyle = new GUIStyle();
            _tooltipStyle.normal.background = tooltipBg;
            _tooltipStyle.padding = new RectOffset(15, 15, 15, 15);
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
            _pulseValue = 0.4f + 0.4f * Mathf.Sin(Time.time * 6.0f);

            bool showCombatOverlay = _isVisible && TurnBasedCombatManager.Instance.IsCombatActive;

            if (showCombatOverlay)
            {
                // Reset hover caching at start of frame
                _hoveredObject = null;

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                Rect overlayRect = new Rect(0, 0, screenWidth, screenHeight);
                GUILayout.BeginArea(overlayRect, _panelStyle);

                // Title Header
                GUILayout.Label("MEWTATIONS: DOGMA — COMBAT ARENA", _headerStyle);
                GUILayout.Space(15);

                if (TurnBasedCombatManager.Instance.State == MewtationsCombatState.Preparation)
                {
                    DrawPreparationUI(screenWidth, screenHeight);
                }
                else
                {
                    DrawBattleUI(screenWidth, screenHeight);
                }

                GUILayout.EndArea();
            }

            // Global mouse up to clear drag if not applied (dropped on empty space)
            if (Event.current.type == EventType.MouseUp && DraggedRingItem != null)
            {
                DraggedRingItem = null;
            }

            // Draw ghost icon
            if (DraggedRingItem != null)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Rect ghostRect = new Rect(mousePos.x - 30, mousePos.y - 30, 60, 60);
                
                GUI.color = new Color(1, 1, 1, 0.6f);
                if (DraggedRingItem.CardData.Icon != null)
                {
                    GUI.DrawTexture(ghostRect, DraggedRingItem.CardData.Icon.texture);
                }
                else
                {
                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                    boxStyle.normal.background = CreateColorTexture(new Color(0.2f, 0.2f, 0.3f, 0.9f));
                    boxStyle.normal.textColor = Color.white;
                    GUI.Box(ghostRect, DraggedRingItem.CardData.Name, boxStyle);
                }
                GUI.color = Color.white;
            }

            // Render tooltip on top of all windows at the end of repaint
            if (Event.current.type == EventType.Repaint)
            {
                DrawFloatingTooltip();
            }
        }

        private void DrawPreparationUI(float screenWidth, float screenHeight)
        {
            GUILayout.BeginHorizontal();

            // ================== COLUMN 1: SIDEBAR & GRID ==================
            GUILayout.BeginHorizontal(GUILayout.Width(screenWidth * 0.45f));
            
            // Sub-column 1A: Sidebar list of Available Cats
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.16f));
            GUILayout.Label("<b>KHO DỰ BỊ</b>", _headerStyle);
            GUILayout.Space(10);

            _sidebarScrollPos = GUILayout.BeginScrollView(_sidebarScrollPos, GUILayout.Height(screenHeight * 0.70f));
            
            var formation = TurnBasedCombatManager.Instance.Formation;
            var availableCats = TurnBasedCombatManager.Instance.AvailableCats;

            foreach (var comb in availableCats)
            {
                var cat = comb as CatCardData;
                if (cat == null) continue;

                // Check if this cat is already in player's grid team
                bool isSlotted = formation.PlayerUnits.Exists(u => u.Source == comb);
                
                GUIStyle sidebarCardStyle = new GUIStyle(_sidebarCardStyle);
                if (_selectedSidebarCat == cat)
                {
                    Texture2D highlightBg = CreateColorTexture(new Color(0.25f, 0.35f, 0.45f, 0.95f));
                    sidebarCardStyle.normal.background = highlightBg;
                }

                GUILayout.BeginVertical(sidebarCardStyle, GUILayout.Height(65));
                
                string nameText = isSlotted ? $"<color=#777>{cat.Name} (Đã Lên Lưới)</color>" : $"<b>{cat.Name}</b>";
                GUILayout.Label(nameText, _logStyle);

                string roleVi = cat.Role switch
                {
                    CatRole.DPS => "DPS 主力",
                    CatRole.Tank => "Đỡ Đòn (Tank)",
                    CatRole.ShieldSupport => "Hỗ Trợ Khiên",
                    CatRole.RageSupport => "Hỗ Trợ Nộ",
                    CatRole.Debuff => "Áp Chế",
                    CatRole.Disruption => "Quấy Nhiễu",
                    CatRole.Attrition => "Hậu Kỳ",
                    _ => cat.Role.ToString()
                };

                GUILayout.Label($"<color=#ff8>{roleVi}</color> | HP: {cat.HealthPoints}/{cat.ProcessedCombatStats.MaxHealth}", _logStyle);

                // Handle drop or selection
                Rect cardRect = GUILayoutUtility.GetLastRect();
                cardRect.y -= 18;
                cardRect.height = 65;
                
                if (cardRect.Contains(Event.current.mousePosition))
                {
                    _hoveredObject = cat;
                    if (Event.current.type == EventType.MouseUp && DraggedRingItem != null)
                    {
                        ApplyRingItemToCat(DraggedRingItem, cat);
                        DraggedRingItem = null;
                        Event.current.Use();
                    }
                }
                
                if (GUI.Button(cardRect, "", GUIStyle.none))
                {
                    if (!isSlotted)
                    {
                        if (_selectedSidebarCat == cat)
                        {
                            _selectedSidebarCat = null;
                        }
                        else
                        {
                            _selectedSidebarCat = cat;
                        }
                    }
                }
                
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // Sub-column 1B: Player 3x3 Grid
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.26f));
            GUILayout.Label("<b>LƯỚI CHIẾN THUẬT (3x3)</b>", _headerStyle);
            int catCountOnGrid = formation.PlayerUnits.Count;
            GUILayout.Label($"<color=#7bc>Xuất kích: {catCountOnGrid}/5 Mèo</color>", _headerStyle);
            GUILayout.Space(15);

            DrawPrepGrid(true);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // ================== COLUMN 2: CENTRAL ACTIONS ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.18f));
            GUILayout.Label("<b>KẾ HOẠCH BÀY TRẬN</b>", _headerStyle);
            GUILayout.Space(30);

            // Large GOLD FIGHT button
            bool canFight = catCountOnGrid >= 1 && catCountOnGrid <= 5;
            GUIStyle fightBtnStyle = new GUIStyle(canFight ? _goldButtonStyle : _buttonStyle);
            if (!canFight)
            {
                fightBtnStyle.normal.background = CreateColorTexture(new Color(0.3f, 0.3f, 0.3f, 0.9f));
                fightBtnStyle.normal.textColor = Color.gray;
            }

            if (GUILayout.Button("⚔️ XUẤT TRẬN (FIGHT)", fightBtnStyle, GUILayout.Height(65)))
            {
                if (canFight)
                {
                    TurnBasedCombatManager.Instance.ConfirmFight();
                }
                else
                {
                    TurnBasedCombatManager.Instance.AddLog("⚠️ Yêu cầu tối thiểu 1 và tối đa 5 Mèo trên lưới chiến thuật!");
                }
            }

            GUILayout.Space(20);

            // Retreat button
            if (GUILayout.Button("🏳 RÚT LUI (RETREAT)", _buttonStyle, GUILayout.Height(45)))
            {
                TurnBasedCombatManager.Instance.Retreat();
            }

            GUILayout.Space(40);
            GUILayout.Label("<color=#ccc>• Click Mèo bên Kho Dự Bị, chọn ô trống trên Lưới 3x3 để sắp đặt.</color>", _logStyle);
            GUILayout.Space(5);
            GUILayout.Label("<color=#ccc>• Click Mèo trên lưới để rút lui khỏi đội hình.</color>", _logStyle);
            GUILayout.Space(5);
            GUILayout.Label("<color=#ccc>• Di chuột (hover) để xem Chi tiết Chỉ số & Kỹ năng.</color>", _logStyle);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // ================== COLUMN 3: ENEMY 3x3 GRID ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.32f));
            GUILayout.Label("<b>QUÂN ĐỊCH</b>", _headerStyle);
            GUILayout.Label("<color=#f55>Đội hình bài trí sẵn</color>", _headerStyle);
            GUILayout.Space(25);

            DrawPrepGrid(false);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            DrawOrderingInventoryUI(screenWidth, screenHeight);
        }

        private void DrawPrepGrid(bool isPlayer)
        {
            GUILayout.BeginVertical();
            if (isPlayer)
            {
                // For Player: Row 2 (Back, 6-8) -> Row 1 (Mid, 3-5) -> Row 0 (Front, 0-2)
                // Row 2: Back Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col + 6, true);
                GUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Row 1: Mid Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col + 3, true);
                GUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Row 0: Front Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col, true);
                GUILayout.EndHorizontal();
            }
            else
            {
                // For Enemy: Row 0 (Front, 0-2) -> Row 1 (Mid, 3-5) -> Row 2 (Back, 6-8)
                // Row 0: Front Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col, false);
                GUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Row 1: Mid Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col + 3, false);
                GUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Row 2: Back Row
                GUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++) DrawPrepSlot(col + 6, false);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawPrepSlot(int slotIndex, bool isPlayer)
        {
            var formation = TurnBasedCombatManager.Instance.Formation;
            var units = isPlayer ? formation.PlayerUnits : formation.EnemyUnits;
            var unit = units.Find(u => u.SlotIndex == slotIndex);

            GUIStyle cardStyle = new GUIStyle(_unitCardStyle);
            if (isPlayer && _selectedSidebarCat != null && unit == null)
            {
                // Highlight empty slots when a cat is selected
                Texture2D greenBorder = CreateColorTexture(new Color(0.15f, 0.45f, 0.25f, 0.9f));
                cardStyle.normal.background = greenBorder;
            }

            float screenWidth = Screen.width;
            
            GUILayout.BeginVertical(cardStyle, GUILayout.Width(screenWidth * 0.08f), GUILayout.Height(105));

            if (unit == null)
            {
                if (isPlayer)
                {
                    string label = _selectedSidebarCat != null ? "<color=#2f3>[ Đặt Mèo ]</color>" : "<color=#555>[ Trống ]</color>";
                    if (GUILayout.Button(label, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (_selectedSidebarCat != null)
                        {
                            if (formation.PlayerUnits.Count >= 5)
                            {
                                TurnBasedCombatManager.Instance.AddLog("⚠️ Đội hình tối đa là 5 Mèo!");
                            }
                            else
                            {
                                var newUnit = new CombatUnit(_selectedSidebarCat, true, slotIndex);
                                formation.PlayerUnits.Add(newUnit);
                                _selectedSidebarCat = null;
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label("<color=#444>[ Trống ]</color>", _logStyle);
                }
            }
            else
            {
                string colorHex = isPlayer ? "#7bc" : "#faa";
                string contentText = $"<b><color={colorHex}>{unit.Name}</color></b>\n";
                
                string roleVi = unit.Role switch
                {
                    CatRole.DPS => "DPS 主",
                    CatRole.Tank => "Tank Đỡ",
                    CatRole.ShieldSupport => "Khiên Hỗ",
                    CatRole.RageSupport => "Nộ Hỗ",
                    CatRole.Debuff => "Áp Chế",
                    CatRole.Disruption => "Quấy Rối",
                    CatRole.Attrition => "Hậu Kỳ",
                    _ => unit.Role.ToString()
                };
                
                contentText += $"<size=11>{roleVi}</size>\n";
                contentText += $"<size=11>HP: {unit.CurrentHP}/{unit.MaxHP}</size>";

                if (isPlayer)
                {
                    Rect slotRect = GUILayoutUtility.GetLastRect();
                    
                    if (slotRect.Contains(Event.current.mousePosition))
                    {
                        _hoveredObject = unit;
                        if (Event.current.type == EventType.MouseUp && DraggedRingItem != null && unit.Source is CatCardData cat)
                        {
                            ApplyRingItemToCat(DraggedRingItem, cat);
                            DraggedRingItem = null;
                            Event.current.Use();
                        }
                    }
                    
                    if (GUILayout.Button(contentText, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        formation.PlayerUnits.Remove(unit);
                    }
                }
                else
                {
                    GUILayout.Label(contentText, _logStyle);
                    
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(Event.current.mousePosition))
                    {
                        _hoveredObject = unit;
                    }
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawBattleUI(float screenWidth, float screenHeight)
        {
            GUILayout.BeginHorizontal();

            // ================== LEFT SIDE: PLAYER TEAM ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.38f));
            GUILayout.Label("<color=#7bc>ĐỘI HÌNH THẦN MIÊU</color>", _headerStyle);
            GUILayout.Space(10);
            DrawTeamGrid(TurnBasedCombatManager.Instance.Formation.PlayerUnits);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // ================== CENTER SIDE: LOG & CONTROL ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.22f));
            GUILayout.Label($"<b>VÒNG ĐẤU: {TurnBasedCombatManager.Instance.CurrentRound}</b>", _headerStyle);
            if (TurnBasedCombatManager.Instance.CurrentRound > 10)
            {
                GUILayout.Label("<color=#ff4444><b>⚠️ LINH KHÍ SUY KIỆT\n(-50% Trị Liệu & Giáp!)</b></color>", _headerStyle);
            }
            GUILayout.Space(10);

            // Scroll view for Combat Logs
            _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition, GUILayout.Height(screenHeight * 0.65f));
            foreach (var log in TurnBasedCombatManager.Instance.CombatLog)
            {
                GUILayout.Label(log, _logStyle);
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(20);

            if (TurnBasedCombatManager.Instance.Result == CombatResult.Ongoing)
            {
                GUILayout.Label("<color=red><b>⚡ ĐÃ BƯỚC VÀO TỬ CHIẾN\nKhông thể rút lui khỏi thiên kiếp!</b></color>", _headerStyle);
            }
            else
            {
                string resultText = TurnBasedCombatManager.Instance.Result == CombatResult.Victory ? "<color=#2f3>CHIẾN THẮNG!</color>" : "<color=#f33>THẤT BẠI!</color>";
                GUILayout.Label($"<b>KẾT QUẢ: {resultText}</b>", _headerStyle);
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // ================== RIGHT SIDE: ENEMY TEAM ==================
            GUILayout.BeginVertical(GUILayout.Width(screenWidth * 0.38f));
            GUILayout.Label("<color=#f55>QUÂN THÙ</color>", _headerStyle);
            GUILayout.Space(10);
            DrawTeamGrid(TurnBasedCombatManager.Instance.Formation.EnemyUnits);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawTeamGrid(List<CombatUnit> units)
        {
            GUILayout.BeginVertical();

            // Row 2: Back Row
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 3; col++)
            {
                DrawUnitSlot(units, col + 6);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Row 1: Mid Row
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 3; col++)
            {
                DrawUnitSlot(units, col + 3);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Row 0: Front Row
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 3; col++)
            {
                DrawUnitSlot(units, col);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawUnitSlot(List<CombatUnit> units, int slotIndex)
        {
            var unit = units.Find(u => u.SlotIndex == slotIndex);

            GUIStyle cardStyle = new GUIStyle(_unitCardStyle);
            if (unit != null && unit.IsAlive && unit.IsPlayer)
            {
                float hpPercent = (float)unit.CurrentHP / unit.MaxHP;
                if (hpPercent < 0.20f)
                {
                    Texture2D pulseRedBg = CreateColorTexture(new Color(0.8f, 0.1f, 0.1f, _pulseValue));
                    cardStyle.normal.background = pulseRedBg;
                }
            }

            GUILayout.BeginVertical(cardStyle, GUILayout.Width(Screen.width * 0.12f), GUILayout.Height(140));

            if (unit == null || !unit.IsAlive)
            {
                GUILayout.Label("<color=#555>[ Trống ]</color>", _logStyle);
            }
            else
            {
                GUILayout.Label($"<b>{unit.Name}</b>", _logStyle);
                
                string roleVi = unit.Role switch
                {
                    CatRole.DPS => "DPS 主力",
                    CatRole.Tank => "Đỡ Đòn (Tank)",
                    CatRole.ShieldSupport => "Hỗ Trợ Khiên",
                    CatRole.RageSupport => "Hỗ Trợ Nộ",
                    CatRole.Debuff => "Áp Chế",
                    CatRole.Disruption => "Quấy Nhiễu",
                    CatRole.Attrition => "Hậu Kỳ",
                    _ => unit.Role.ToString()
                };

                string elementVi = unit.Element switch
                {
                    CatElement.None => "Vô Hệ",
                    CatElement.Fire => "Hỏa 🔥",
                    CatElement.Poison => "Độc ☠️",
                    CatElement.Ice => "Băng ❄️",
                    CatElement.Lightning => "Lôi ⚡",
                    _ => unit.Element.ToString()
                };

                string roleText = unit.IsPlayer ? $"<color=#ff8>{roleVi} | {elementVi}</color>" : $"<color=#faa>Địch ({roleVi})</color>";
                GUILayout.Label(roleText, _logStyle);

                // Speed
                GUILayout.Label($"Speed: {unit.Speed}", _logStyle);

                // Draw HP bar
                float hpPercent = (float)unit.CurrentHP / unit.MaxHP;
                GUILayout.Label($"HP: {unit.CurrentHP}/{unit.MaxHP}", _logStyle);
                Rect hpBarRect = GUILayoutGetLastRect();
                hpBarRect.y += 18;
                hpBarRect.height = 8;
                GUI.Box(hpBarRect, "", _hpBarBackgroundStyle);
                hpBarRect.width *= hpPercent;
                GUI.Box(hpBarRect, "", _hpBarFillStyle);

                GUILayout.Space(12);

                // Shield
                if (unit.Shield > 0)
                {
                    GUILayout.Label($"Giáp: +{unit.Shield}", _logStyle);
                    Rect shieldBarRect = GUILayoutGetLastRect();
                    shieldBarRect.y += 18;
                    shieldBarRect.height = 4;
                    GUI.Box(shieldBarRect, "", _shieldBarFillStyle);
                    GUILayout.Space(6);
                }

                // Draw Rage bar
                float ragePercent = (float)unit.CurrentRage / 145f;
                GUILayout.Label($"Nộ: {unit.CurrentRage}%", _logStyle);
                Rect rageBarRect = GUILayoutGetLastRect();
                rageBarRect.y += 18;
                rageBarRect.height = 6;
                GUI.Box(rageBarRect, "", _hpBarBackgroundStyle);
                rageBarRect.width *= ragePercent;
                GUI.Box(rageBarRect, "", _rageBarFillStyle);

                GUILayout.Space(10);

                // Draw Stamina bar (Player only)
                if (unit.IsPlayer)
                {
                    float staminaPercent = (float)unit.Stamina / unit.MaxStamina;
                    GUILayout.Label($"Thể Lực: {unit.Stamina}/{unit.MaxStamina}", _logStyle);
                    Rect staminaBarRect = GUILayoutGetLastRect();
                    staminaBarRect.y += 18;
                    staminaBarRect.height = 5;
                    GUI.Box(staminaBarRect, "", _hpBarBackgroundStyle);
                    staminaBarRect.width *= staminaPercent;
                    GUI.Box(staminaBarRect, "", _staminaBarFillStyle);
                    GUILayout.Space(8);
                }

                // Exhaustion indicators
                if (unit.IsExhausted)
                {
                    string penaltyText = $"-{20 + (unit.ExhaustionLevel / 3) * 10}%";
                    GUILayout.Label($"<color=#aaaaaa><b>💀 KIỆT SỨC ({penaltyText} chỉ số)</b></color>", _logStyle);
                }

                if (unit.HoiQuangPhanChieuTriggered && unit.Source is CatCardData catData && catData.Constitution == CatConstitution.BaoLinhThienKieu)
                {
                    GUILayout.Label("<color=#ff9900><b>🔥 HỒI QUANG PHẢN CHIẾU</b></color>", _logStyle);
                }

                GUILayout.Space(5);

                // Debuffs
                if (unit.ActiveDebuffs.Count > 0)
                {
                    string debuffsText = "";
                    foreach (var debuff in unit.ActiveDebuffs)
                    {
                        string color = debuff.Type == MewtationsDebuff.Burning ? "#f62" : "#a2f";
                        debuffsText += $"<color={color}>[{debuff.Type} x{debuff.Stacks}]</color> ";
                    }
                    GUILayout.Label(debuffsText, _logStyle);
                }

                // Hover check in battle mode too!
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    _hoveredObject = unit;
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawFloatingTooltip()
        {
            object tooltipTarget = _hoveredObject;
            if (tooltipTarget == null && BoardHoveredCat != null)
            {
                tooltipTarget = BoardHoveredCat;
            }
            
            if (tooltipTarget == null || DraggedRingItem != null) return;

            InitializeStyles();

            Vector2 mousePos = Event.current.mousePosition;
            float width = 350f;
            float height = 330f;
            float x = mousePos.x + 15f;
            float y = mousePos.y + 15f;

            if (x + width > Screen.width) x = mousePos.x - width - 15f;
            if (y + height > Screen.height) y = mousePos.y - height - 15f;

            Rect tooltipRect = new Rect(x, y, width, height);
            
            GUILayout.BeginArea(tooltipRect, _tooltipStyle);

            string title = "";
            string role = "";
            string element = "";
            int maxHp = 0, currentHp = 0, speed = 0, dmg = 0, stamina = 100, maxStamina = 100;
            bool isPlayer = false;
            
            List<string> traits = new List<string>();
            List<string> mutations = new List<string>();
            string weaponName = "Tay Không";
            string attackPatternVi = "Đơn Thể (Tấn công mục tiêu trước mặt)";
            string ultName = "Bí Kỹ Mặc Định";
            string ultDesc = "Bí kỹ căn bản của thần miêu, tấn công mạnh đơn thể.";

            string weaponArchetypeDesc = "Hệ phái tiêu chuẩn";
            string weaponEfficiencyVi = "Bậc C (1.0x sát thương)";
            string weaponResistanceVi = "Giảm 0% sát thương gánh chịu";

            if (tooltipTarget is CombatUnit unit)
            {
                title = unit.Name;
                role = unit.Role.ToString();
                element = unit.Element.ToString();
                maxHp = unit.MaxHP;
                currentHp = unit.CurrentHP;
                speed = unit.Speed;
                dmg = unit.GetAttackDamage();
                stamina = unit.Stamina;
                maxStamina = unit.MaxStamina;
                isPlayer = unit.IsPlayer;

                var weapon = unit.Source.GetEquipableOfEquipableType(EquipableType.Weapon) as Equipable;
                if (weapon != null)
                {
                    weaponName = weapon.Name;
                    var pattern = MewtationsWeaponRegistry.GetAttackPattern(weapon.Id);
                    attackPatternVi = GetWeaponPatternVi(pattern);
                    
                    weaponEfficiencyVi = GetEfficiencyRank(weapon.OutputEfficiency);
                    weaponResistanceVi = $"Giảm {Mathf.RoundToInt(weapon.DamageResistance * 100f)}% sát thương gánh chịu";
                    weaponArchetypeDesc = GetArchetypeDesc(weapon.WeaponArchetype);
                }

                if (unit.Source is CatCardData cat)
                {
                    traits = new List<string>(cat.PermanentTraits);
                    mutations = new List<string>(cat.ActiveMutations);

                    var ultType = MewtationsUltimateRegistry.GetUltimateType(cat);
                    ultName = GetUltimateNameVi(ultType);
                    ultDesc = GetUltimateDescVi(ultType);
                }
            }
            else if (tooltipTarget is GameCard gameCard && gameCard.CardData is CatCardData catCard)
            {
                title = catCard.Name;
                role = catCard.Role.ToString();
                element = catCard.Element.ToString();
                maxHp = catCard.ProcessedCombatStats.MaxHealth;
                currentHp = catCard.HealthPoints;
                speed = catCard.Speed;
                dmg = catCard.ProcessedCombatStats.AttackDamage;
                stamina = catCard.Stamina;
                maxStamina = catCard.MaxStamina;
                isPlayer = true;

                var weapon = catCard.GetEquipableOfEquipableType(EquipableType.Weapon) as Equipable;
                if (weapon != null)
                {
                    weaponName = weapon.Name;
                    var pattern = MewtationsWeaponRegistry.GetAttackPattern(weapon.Id);
                    attackPatternVi = GetWeaponPatternVi(pattern);
                    
                    weaponEfficiencyVi = GetEfficiencyRank(weapon.OutputEfficiency);
                    weaponResistanceVi = $"Giảm {Mathf.RoundToInt(weapon.DamageResistance * 100f)}% sát thương gánh chịu";
                    weaponArchetypeDesc = GetArchetypeDesc(weapon.WeaponArchetype);
                }

                traits = new List<string>(catCard.PermanentTraits);
                mutations = new List<string>(catCard.ActiveMutations);

                var ultType = MewtationsUltimateRegistry.GetUltimateType(catCard);
                ultName = GetUltimateNameVi(ultType);
                ultDesc = GetUltimateDescVi(ultType);
            }

            string roleVi = role switch
            {
                "DPS" => "DPS Chủ Lực",
                "Tank" => "Đỡ Đòn (Tank)",
                "ShieldSupport" => "Hỗ Trợ Giáp",
                "RageSupport" => "Hỗ Trợ Nộ",
                "Debuff" => "Áp Chế Hiệu Ứng",
                "Disruption" => "Quấy Nhiễu Tốc Độ",
                "Attrition" => "Hậu Kỳ Trận Pháp",
                _ => role
            };

            string elementVi = element switch
            {
                "None" => "Không Hệ",
                "Fire" => "Hỏa 🔥",
                "Poison" => "Độc ☠️",
                "Ice" => "Băng ❄️",
                "Lightning" => "Lôi ⚡",
                _ => element
            };

            GUILayout.Label($"<b><size=15>{title}</size></b>", _headerStyle);
            GUILayout.Label($"<color=#ff8>{roleVi}</color> | <color=#8cf>{elementVi}</color>", _logStyle);
            GUILayout.Space(5);

            GUILayout.Label($"HP: <b>{currentHp}/{maxHp}</b>  |  Công: <b>{dmg}</b>  |  Tốc độ: <b>{speed}</b>", _logStyle);
            if (isPlayer)
            {
                GUILayout.Label($"Thể Lực: <b>{stamina}/{maxStamina}</b>", _logStyle);
            }
            GUILayout.Space(5);

            // Dynamic Weapon Details Display
            GUILayout.Label($"<color=#abc><b>Vũ Khí: {weaponName}</b> ({weaponEfficiencyVi})</color>", _logStyle);
            GUILayout.Label($"<size=11>Khu vực đánh: {attackPatternVi}</size>", _logStyle);
            GUILayout.Label($"<size=11>Phòng thủ: {weaponResistanceVi}</size>", _logStyle);
            GUILayout.Label($"<size=11>Hệ phái: <color=#fda>{weaponArchetypeDesc}</color></size>", _logStyle);
            GUILayout.Space(5);

            if (isPlayer)
            {
                GUILayout.Label($"<color=#fda><b>Bí Kỹ: {ultName}</b></color>", _logStyle);
                GUILayout.Label($"<size=11>{ultDesc}</size>", _logStyle);
                GUILayout.Space(5);
            }

            if (traits.Count > 0 || mutations.Count > 0)
            {
                string traitsStr = "";
                foreach (var t in traits)
                {
                    string label = Mewtations.Expedition.HeavenlyTalent.GetDisplayName(t);
                    traitsStr += $"<color=#bf6>[{label}]</color> ";
                }
                foreach (var m in mutations)
                {
                    string label = Mewtations.Expedition.UnstableMutation.GetDisplayName(m);
                    traitsStr += $"<color=#f55>[{label}]</color> ";
                }
                GUILayout.Label($"<b>Kinh Mạch & Thiên Phú:</b>", _logStyle);
                GUILayout.Label($"<size=11>{traitsStr}</size>", _logStyle);
            }

            GUILayout.EndArea();
        }

        private string GetEfficiencyRank(float val)
        {
            if (val >= 2.0f) return "Rank S (2.0x Sát thương)";
            if (val >= 1.5f) return "Rank A (1.5x Sát thương)";
            if (val >= 1.2f) return "Rank B (1.2x Sát thương)";
            if (val >= 1.0f) return "Rank C (1.0x Sát thương)";
            if (val >= 0.8f) return "Rank D (0.8x Sát thương)";
            return "Rank E (0.5x Sát thương)";
        }

        private string GetArchetypeDesc(WeaponArchetype arch)
        {
            return arch switch
            {
                WeaponArchetype.Rally => "📣 Cổ Vũ (Không gây sát thương, nạp nộ khí cho đồng đội)",
                WeaponArchetype.Stun => "❄️ Choáng Bảo (35% tỉ lệ gây hiệu ứng Đóng Băng mục tiêu)",
                WeaponArchetype.Vulnerability => "⚡ Trọng Thương (Đánh dấu khiến mục tiêu nhận +30% sát thương)",
                WeaponArchetype.RagePierce => "📉 Xuyên Nộ (Tấn công hàng dọc, tiêu hao 20 Nộ khí kẻ địch)",
                WeaponArchetype.HeavyPierce => "🔥 Trọng Thương Thương (Công hàng dọc cực mạnh, Kháng sát thương thấp)",
                WeaponArchetype.HeavySweep => "🔥 Trọng Kích Quét (Công hàng ngang cực mạnh, Kháng sát thương thấp)",
                WeaponArchetype.Fortress => "🛡️ Trấn Thủ (Tạo Giáp & Nộ Khí khi đánh, sát thương rất thấp, kháng cao)",
                _ => "Hệ phái tiêu chuẩn"
            };
        }

        private string GetWeaponPatternVi(WeaponAttackPattern pattern)
        {
            return pattern switch
            {
                WeaponAttackPattern.Single => "Đơn Thể (Tấn công mục tiêu trước mặt)",
                WeaponAttackPattern.ColumnAttack => "Đâm Thủng (Tấn công xuyên thấu toàn hàng dọc)",
                WeaponAttackPattern.Cleave => "Chém Lan (Quẹt rộng lan sang 2 bên mục tiêu)",
                WeaponAttackPattern.RageDrain => "Hút Nộ (Gây sát thương, tiêu hao nộ địch và nạp nộ ta)",
                WeaponAttackPattern.RageGain => "Kích Nộ (Đòn đánh hồi phục lượng lớn nộ khí)",
                WeaponAttackPattern.Row => "Quét Hàng (Gọi lôi điện tấn công toàn bộ hàng ngang)",
                _ => pattern.ToString()
            };
        }

        private string GetUltimateNameVi(UltimateType type)
        {
            return type switch
            {
                UltimateType.DefaultBasicBoost => "Bí Kỹ: Thần Miêu Kích",
                UltimateType.HealLowest => "Bí Kỹ: Thần Dược Linh Súp",
                UltimateType.AoeFireBurn => "Bí Kỹ: Bát Hoang Hỏa Diệm",
                UltimateType.ShieldTeam => "Bí Kỹ: Kim Chung Hộ Thể",
                UltimateType.DisruptStun => "Bí Kỹ: Băng Cực Phong Ấn",
                _ => type.ToString()
            };
        }

        private string GetUltimateDescVi(UltimateType type)
        {
            return type switch
            {
                UltimateType.DefaultBasicBoost => "Bùng phát linh lực, đập nện gây 2.0x sát thương đơn cực lớn.",
                UltimateType.HealLowest => "Dùng Linh Súp nạp đầy sức sống, hồi sinh HP cực mạnh cho đồng đội yếu nhất.",
                UltimateType.AoeFireBurn => "Kích hoạt hỏa trận thiêu đốt tất cả địch, tích lũy 2 tầng Thiêu Đốt.",
                UltimateType.ShieldTeam => "Dựng lên hào quang linh lực, tạo Khiên hấp thụ sát thương cực lớn cho toàn đội.",
                UltimateType.DisruptStun => "Nhảy đập gây sát thương lớn và Đóng Băng (đứng hình 1 lượt) kẻ địch chính.",
                _ => type.ToString()
            };
        }

        private void ApplyRingItemToCat(GameCard ringItem, CatCardData cat)
        {
            if (ringItem == null || cat == null) return;

            var ringCard = WorldManager.instance.AllCards
                .FirstOrDefault(c => c != null && c.CardData is Mewtations.Legacy.Stacklands.StorageRingCardData && !c.Destroyed);
            if (ringCard == null) return;

            if (ringItem.CardData is Equipable eq)
            {
                var oldEquip = cat.GetEquipableOfEquipableType(eq.EquipableType);
                if (oldEquip != null)
                {
                    cat.UnequipItem(oldEquip);
                    ringCard.InventoryContainer.Insert(oldEquip.MyGameCard, new ContainerInsertContext { SourceCard = cat.MyGameCard, ContextSource = "OrderingSwap" });
                }
                
                ringCard.InventoryContainer.Remove(ringItem);
                cat.MyGameCard.Equip(eq);
                TurnBasedCombatManager.Instance.AddLog($"⚔️ Trang bị thành công '{eq.Name}' cho {cat.Name}!");
            }
            else if (ringItem.CardData.Id.Contains("potion") || ringItem.CardData.Id.Contains("pill") || ringItem.CardData.Id.Contains("food") || ringItem.CardData is Food)
            {
                cat.HealthPoints = Mathf.Min(cat.ProcessedCombatStats.MaxHealth, cat.HealthPoints + 15);
                
                ringCard.InventoryContainer.Remove(ringItem);
                ringItem.DestroyCard(true, true);
                
                TurnBasedCombatManager.Instance.AddLog($"🧪 Sử dụng '{ringItem.CardData.Name}' cho {cat.Name}. Phục hồi +15 HP!");
            }
            else
            {
                TurnBasedCombatManager.Instance.AddLog("⚠️ Vật phẩm này không thể trang bị hoặc sử dụng lên Mèo!");
            }

            _selectedRingItem = null;
        }

        private void DrawOrderingInventoryUI(float screenWidth, float screenHeight)
        {
            GUILayout.BeginVertical(_sidebarCardStyle, GUILayout.ExpandWidth(true));
            
            var ringCard = WorldManager.instance.AllCards
                .FirstOrDefault(c => c != null && c.CardData is Mewtations.Legacy.Stacklands.StorageRingCardData && !c.Destroyed);

            string title = "<b>NHẪN TRỮ VẬT (EXPEDITION STORAGE RING) - LƯỚI 6x5 TIỂU THẾ GIỚI</b>";
            if (ringCard == null)
            {
                GUILayout.Label($"{title} <color=red>(Không tìm thấy Nhẫn Trữ Vật trên Board!)</color>", _headerStyle);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label(title, _headerStyle);
            GUILayout.Space(10);

            var container = ringCard.InventoryContainer;
            var items = container != null ? container.GetChildren() : new List<GameCard>();
            bool hasRelic = ShrineCardData.IsRelicActiveInShrine("item_ancient_relic_insurance");

            GUILayout.BeginVertical();
            for (int r = 0; r < 5; r++)
            {
                GUILayout.BeginHorizontal();
                for (int c = 0; c < 6; c++)
                {
                    int index = r * 6 + c;
                    bool isProtected = hasRelic && index < 5;
                    
                    GUIStyle slotStyle = new GUIStyle(_unitCardStyle);
                    if (isProtected)
                    {
                        Texture2D protectedBg = CreateColorTexture(new Color(0.1f, 0.4f, 0.45f, 0.85f));
                        slotStyle.normal.background = protectedBg;
                    }

                    GameCard itemCard = index < items.Count ? items[index] : null;

                    string buttonText = "[ Trống ]";
                    if (isProtected && itemCard == null)
                    {
                        buttonText = "<color=#00ffcc>[🛡️ Trống]</color>";
                    }
                    else if (itemCard != null)
                    {
                        string prefix = isProtected ? "<color=#00ffcc>🛡️</color> " : "";
                        buttonText = $"{prefix}<b>{itemCard.CardData.Name}</b>";
                    }

                    if (itemCard != null && DraggedRingItem == itemCard)
                    {
                        Texture2D selBg = CreateColorTexture(new Color(0.85f, 0.70f, 0.35f, 0.5f)); // Half transparent ghost style
                        slotStyle.normal.background = selBg;
                    }

                    GUILayout.Box(buttonText, slotStyle, GUILayout.Width(screenWidth * 0.15f), GUILayout.Height(50));
                    Rect slotRect = GUILayoutUtility.GetLastRect();
                    
                    if (itemCard != null && slotRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            DraggedRingItem = itemCard;
                            Event.current.Use();
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);
            
            // TRASH SLOT
            GUIStyle trashStyle = new GUIStyle(GUI.skin.box);
            trashStyle.normal.background = CreateColorTexture(new Color(0.4f, 0.1f, 0.1f, 0.9f));
            trashStyle.normal.textColor = Color.red;
            trashStyle.fontSize = 16;
            trashStyle.fontStyle = FontStyle.Bold;
            trashStyle.alignment = TextAnchor.MiddleCenter;
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("🗑️ HỦY TÀI NGUYÊN (Kéo thả vào đây để xóa vĩnh viễn)", trashStyle, GUILayout.Width(screenWidth * 0.4f), GUILayout.Height(60));
            Rect trashRect = GUILayoutUtility.GetLastRect();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (DraggedRingItem != null && trashRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    TurnBasedCombatManager.Instance.AddLog($"🗑️ Đã vứt bỏ vĩnh viễn: {DraggedRingItem.CardData.Name}");
                    DraggedRingItem.DestroyCard(true, true);
                    DraggedRingItem = null;
                    Event.current.Use();
                }
            }

            GUILayout.EndVertical();
        }

        private Rect GUILayoutGetLastRect()
        {
            return GUILayoutUtility.GetLastRect();
        }
    }
}
