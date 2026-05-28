using UnityEngine;
using System.Collections.Generic;
using Mewtations.Expedition;
using Mewtations.Combat.UI;

namespace Mewtations.UI
{
    public static class CharacterPanelUIInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            var go = new GameObject("CharacterPanelUI");
            go.AddComponent<CharacterPanelUI>();
            GameObject.DontDestroyOnLoad(go);
        }
    }

    public class CharacterPanelUI : MonoBehaviour
    {
        public static CharacterPanelUI Instance { get; private set; }

        public CatCardData TargetCat { get; private set; }

        private bool _isOpen = false;
        private float _fadeAlpha = 0f;
        private float _fadeSpeed = 8f;

        private GUIStyle _panelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _slotStyle;
        private GUIStyle _textStyle;
        private Texture2D _panelBg;

        private Vector2 _scrollPos;

        // Ghost drag
        private EquipmentSlotData _draggedSlotData;
        private CatSlotType _draggedSlotType;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void InitStyles()
        {
            if (_panelStyle != null) return;

            _panelBg = CreateColorTexture(new Color(0.1f, 0.12f, 0.15f, 0.95f));

            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.background = _panelBg;
            _panelStyle.padding = new RectOffset(20, 20, 20, 20);

            _headerStyle = new GUIStyle(GUI.skin.label);
            _headerStyle.fontSize = 20;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = new Color(0.9f, 0.8f, 0.4f);

            _textStyle = new GUIStyle(GUI.skin.label);
            _textStyle.fontSize = 14;
            _textStyle.richText = true;
            _textStyle.normal.textColor = Color.white;

            _slotStyle = new GUIStyle(GUI.skin.box);
            _slotStyle.normal.background = CreateColorTexture(new Color(0.15f, 0.18f, 0.22f, 0.9f));
            _slotStyle.normal.textColor = Color.white;
            _slotStyle.alignment = TextAnchor.MiddleCenter;
            _slotStyle.fontSize = 14;
        }

        private Texture2D CreateColorTexture(Color c)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        public void OpenPanel(CatCardData cat)
        {
            TargetCat = cat;
            _isOpen = true;
            // Clear any dragging
            _draggedSlotData = null;
        }

        public void ClosePanel()
        {
            _isOpen = false;
            if (_draggedSlotData != null && TargetCat != null)
            {
                // If we were dragging an item out and closed the panel, unequip it
                TargetCat.UnequipFromSlot(_draggedSlotType, true);
                _draggedSlotData = null;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                if (WorldManager.instance != null && WorldManager.instance.HoveredCard != null)
                {
                    if (WorldManager.instance.HoveredCard.CardData is CatCardData cat)
                    {
                        OpenPanel(cat);
                    }
                }
            }

            if (_isOpen && TargetCat != null)
            {
                if (TargetCat.MyGameCard == null || TargetCat.MyGameCard.IsDragging || TargetCat.MyGameCard.HasParent || TargetCat.HealthPoints <= 0 || (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.IsCombatActive))
                {
                    ClosePanel();
                }
            }

            if (_isOpen && TargetCat == null)
            {
                ClosePanel();
            }

            // Global Mouse Up for clearing dragged slot
            if (_draggedSlotData != null && Input.GetMouseButtonUp(0))
            {
                // Handled in OnGUI, but if missed we drop to world
                if (TargetCat != null)
                {
                    TargetCat.UnequipFromSlot(_draggedSlotType, true);
                }
                _draggedSlotData = null;
            }

            if (_isOpen)
                _fadeAlpha = Mathf.Lerp(_fadeAlpha, 1f, Time.deltaTime * _fadeSpeed);
            else
                _fadeAlpha = Mathf.Lerp(_fadeAlpha, 0f, Time.deltaTime * _fadeSpeed);
        }

        private void OnGUI()
        {
            if (_fadeAlpha < 0.01f) return;
            if (TargetCat == null) return;

            InitStyles();

            GUI.color = new Color(1, 1, 1, _fadeAlpha);
            
            float width = 450;
            float height = Screen.height * 0.8f;
            float x = Screen.width - width - 20;
            float y = (Screen.height - height) / 2f;

            Rect panelRect = new Rect(x, y, width, height);

            // Matrix scale animation
            Vector2 pivot = new Vector2(x + width / 2, y + height / 2);
            float scale = Mathf.Lerp(0.9f, 1f, _fadeAlpha);
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), pivot);

            GUILayout.BeginArea(panelRect, _panelStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(TargetCat.Name, _headerStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(30)))
            {
                ClosePanel();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label($"Vai trò: <color=#5dade2>{TargetCat.Role}</color> | HP: {TargetCat.HealthPoints}/{TargetCat.ProcessedCombatStats.MaxHealth} | Tốc độ: {TargetCat.Speed}", _textStyle);
            GUILayout.Space(20);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            DrawEquipmentCategory(CatSlotType.Weapon);
            DrawEquipmentCategory(CatSlotType.Torso);
            DrawEquipmentCategory(CatSlotType.Head);
            DrawEquipmentCategory(CatSlotType.Pill);
            DrawEquipmentCategory(CatSlotType.Skill);
            DrawEquipmentCategory(CatSlotType.Passive1);
            DrawEquipmentCategory(CatSlotType.Passive2);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.matrix = oldMatrix;

            // Draw ghost drag outside the layout
            if (_draggedSlotData != null && _draggedSlotData.EquippedItem != null)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Rect ghostRect = new Rect(mousePos.x - 30, mousePos.y - 30, 60, 60);
                if (_draggedSlotData.EquippedItem.Icon != null)
                {
                    GUI.color = new Color(1, 1, 1, 0.6f);
                    GUI.DrawTexture(ghostRect, _draggedSlotData.EquippedItem.Icon.texture);
                    GUI.color = Color.white;
                }
            }
        }

        private void DrawEquipmentCategory(CatSlotType type)
        {
            TargetCat.InitializeEquipmentSlots();
            if (!TargetCat.EquipmentSlots.ContainsKey(type)) return;
            
            var slot = TargetCat.EquipmentSlots[type];
            GUILayout.Label($"<b>{slot.Title}</b>", _textStyle);
            GUILayout.BeginHorizontal();
            
            DrawSlot(slot, type);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        private void DrawSlot(EquipmentSlotData slot, CatSlotType type)
        {
            GUIStyle sStyle = new GUIStyle(_slotStyle);

            string text = "[ Khóa ]";
            if (slot.IsUnlocked)
            {
                text = "[ Trống ]";
                if (slot.EquippedItem != null)
                {
                    text = $"<b>{slot.EquippedItem.Name}</b>";
                    if (_draggedSlotData == slot)
                    {
                        text = "<color=#888>[ Đang Kéo... ]</color>";
                    }
                }
            }
            else
            {
                sStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            }

            GUILayout.Box(text, sStyle, GUILayout.Width(190), GUILayout.Height(60));
            Rect r = GUILayoutUtility.GetLastRect();

            if (!slot.IsUnlocked) return;

            // Handle Drag Out
            if (slot.EquippedItem != null && _draggedSlotData == null)
            {
                if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    _draggedSlotData = slot;
                    _draggedSlotType = type;
                    Event.current.Use();
                }
            }

            // Handle Drop In from Board
            if (WorldManager.instance != null && WorldManager.instance.DraggingCard != null)
            {
                if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    CardData itemCard = WorldManager.instance.DraggingCard.CardData;
                    if (slot.CanEquip(itemCard))
                    {
                        TargetCat.EquipToSlot(itemCard, type);
                        WorldManager.instance.DraggingCard = null; // Cancel physical drag
                        Event.current.Use();
                    }
                }
            }
            else if (CombatOverlayUI.Instance != null && CombatOverlayUI.Instance.DraggedRingItem != null)
            {
                if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    CardData itemCard = CombatOverlayUI.Instance.DraggedRingItem.CardData;
                    if (slot.CanEquip(itemCard))
                    {
                        TargetCat.EquipToSlot(itemCard, type);
                        CombatOverlayUI.Instance.DraggedRingItem = null;
                        Event.current.Use();
                    }
                }
            }
        }
    }
}
