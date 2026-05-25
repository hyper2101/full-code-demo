using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat
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
            if (!_isVisible || !TurnBasedCombatManager.Instance.IsCombatActive) return;

            InitializeStyles();
            _pulseValue = 0.4f + 0.4f * Mathf.Sin(Time.time * 6.0f);

            // Set up full screen layout
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Rect overlayRect = new Rect(0, 0, screenWidth, screenHeight);
            GUILayout.BeginArea(overlayRect, _panelStyle);

            // Title Header
            GUILayout.Label("MEWTATIONS: DOGMA — COMBAT ARENA", _headerStyle);
            GUILayout.Space(15);

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

            // Retreat Button Disabled for epic deathmatches
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
            GUILayout.EndArea();
        }

        private void DrawTeamGrid(List<CombatUnit> units)
        {
            // Grid layout showing units
            // Display front row (0-2) and back row (3-5)
            GUILayout.BeginVertical();

            // Row 1: Back Row
            GUILayout.BeginHorizontal();
            for (int col = 0; col < 3; col++)
            {
                DrawUnitSlot(units, col + 3);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Row 2: Front Row
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
                // Drawing unit info
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

                // Speed display
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

                // Shield display if active
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

                // Exhaustion and Hồi Quang Phản Chiếu indicators
                if (unit.IsExhausted)
                {
                    string penaltyText = $"-{20 + (unit.ExhaustionDuration / 3) * 10}%";
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
            }

            GUILayout.EndVertical();
        }

        private Rect GUILayoutGetLastRect()
        {
            return GUILayoutUtility.GetLastRect();
        }
    }
}
