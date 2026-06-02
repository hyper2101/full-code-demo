using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Expedition;

public class GatewayRouteUI : MonoBehaviour
{
    private bool _isVisible = false;
    private GatewayExpeditionCardData _currentGateway;

    private GUIStyle _panelStyle;
    private GUIStyle _slotUnlockedStyle;
    private GUIStyle _slotLockedStyle;
    private GUIStyle _headerStyle;

    public void Open(GatewayExpeditionCardData gateway)
    {
        _currentGateway = gateway;
        _isVisible = true;
    }

    public void Close()
    {
        _isVisible = false;
        if (_currentGateway != null)
        {
            // Reset the flag on the gateway
            _currentGateway.SetRouteUiOpen(false);
        }
        _currentGateway = null;
    }

    private void InitializeStyles()
    {
        if (_panelStyle != null) return;
        
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.95f));
        bgTex.Apply();

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = bgTex;
        _panelStyle.padding = new RectOffset(20, 20, 20, 20);
        
        _slotUnlockedStyle = new GUIStyle(GUI.skin.button);
        _slotUnlockedStyle.fontSize = 14;
        _slotUnlockedStyle.fontStyle = FontStyle.Bold;
        
        _slotLockedStyle = new GUIStyle(GUI.skin.box);
        _slotLockedStyle.fontSize = 13;
        _slotLockedStyle.normal.textColor = Color.gray;

        _headerStyle = new GUIStyle(GUI.skin.label);
        _headerStyle.fontSize = 24;
        _headerStyle.fontStyle = FontStyle.Bold;
        _headerStyle.normal.textColor = new Color(0.9f, 0.8f, 0.4f);
        _headerStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        if (!_isVisible || _currentGateway == null) return;

        InitializeStyles();
        
        float width = 640;
        float height = 400;
        Rect panelRect = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);

        GUILayout.BeginArea(panelRect, _panelStyle);
        GUILayout.Label("GATEWAY EXPEDITION ROUTES", _headerStyle);
        GUILayout.Space(30);

        int cols = 4;
        for (int i = 0; i < _currentGateway.Routes.Count; i += cols)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int j = 0; j < cols; j++)
            {
                if (i + j < _currentGateway.Routes.Count)
                {
                    var route = _currentGateway.Routes[i + j];
                    DrawRouteSlot(route);
                    GUILayout.Space(10);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", GUILayout.Width(150), GUILayout.Height(40)))
        {
            Close();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawRouteSlot(ExpeditionRouteDefinition route)
    {
        if (route.IsUnlocked)
        {
            if (GUILayout.Button($"<color=#ffffff>{route.DisplayName}</color>\n\n<color=#ffcc00>[{route.Difficulty}]</color>", _slotUnlockedStyle, GUILayout.Width(130), GUILayout.Height(110)))
            {
                _currentGateway.BeginPreparingExpedition(route);
                Close();
            }
        }
        else
        {
            GUILayout.Box($"<color=#888888>[ L O C K E D ]\n\nReq: {route.UnlockConditionId}</color>", _slotLockedStyle, GUILayout.Width(130), GUILayout.Height(110));
        }
    }
}
