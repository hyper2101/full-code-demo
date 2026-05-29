using Mewtations.Core;
using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedAutomation)]
    public class CardConnector : Draggable
{
	public bool IsEnergyConnector
	{
		get
		{
			return this.ConnectionType == ConnectionType.LV || this.ConnectionType == ConnectionType.HV;
		}
	}

	public void InitializeEnergyNode(CardConnectorData data, GameCard parent)
	{
		this.Parent = parent;
		this.CardDirection = data.EnergyConnectionType;
		this.ConnectionType = data.EnergyConnectionStrength;
		this.BasePosition = base.transform.localPosition;
	}

	protected override void Update()
	{
		if (WorldManager.instance.CurrentBoard.Id != "cities")
		{
			bool flag = false;
			if (!WorldManager.instance.CanUseTransport && this.ConnectionType == ConnectionType.Transport)
			{
				flag = true;
			}
			if (this.ConnectionType != ConnectionType.Transport)
			{
				flag = true;
			}
			if (flag)
			{
				base.transform.localScale = Vector3.zero;
				return;
			}
		}
		if (WorldManager.instance.CurrentView == ViewType.Default || WorldManager.instance.CurrentView == ViewType.Calamity)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Energy && this.ConnectionType != ConnectionType.LV && this.ConnectionType != ConnectionType.HV)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Transport && this.ConnectionType != ConnectionType.Transport)
		{
			this.isActive = false;
		}
		else if (WorldManager.instance.CurrentView == ViewType.Sewer && this.ConnectionType != ConnectionType.Sewer)
		{
			this.isActive = false;
		}
		else
		{
			this.isActive = true;
		}
		if (this.isActive)
		{
			bool flag2 = InputController.instance.StoppedGrabbing();
			if ((InputController.instance.GetInputEnded(0) || flag2) && CitiesManager.instance.DrawingConnector != null)
			{
				CardConnector cardConnector = WorldManager.instance.HoveredDraggable as CardConnector;
				if (cardConnector != null && cardConnector != CitiesManager.instance.DrawingConnector)
				{
					if (cardConnector.ConnectedNode == null)
					{
						AudioManager.me.PlaySound2D(this.GetConnectSoundForType(this.ConnectionType), 1f, 0.8f);
						CitiesManager.instance.StopDrawCable(WorldManager.instance.HoveredDraggable as CardConnector);
					}
					else
					{
						CitiesManager.instance.DrawingConnector = null;
					}
				}
				else
				{
					CitiesManager.instance.StopDrawCable(null);
				}
			}
		}
		this.UpdateConnectorVisuals();
	}

	private Sprite GetSpriteForConnection(ConnectionType connection)
	{
		if (connection == ConnectionType.HV)
		{
			return SpriteManager.instance.HighVoltageSprite;
		}
		if (connection == ConnectionType.LV)
		{
			return SpriteManager.instance.LowVoltageSprite;
		}
		if (connection == ConnectionType.Sewer)
		{
			return SpriteManager.instance.SewerSprite;
		}
		if (connection == ConnectionType.Transport)
		{
			return SpriteManager.instance.TransportSprite;
		}
		return null;
	}

	private Color GetColorForConnection(ConnectionType connection, bool isConnected)
	{
		if (connection == ConnectionType.HV)
		{
			if (!isConnected)
			{
				return ColorManager.instance.HighVoltageConnector;
			}
			return ColorManager.instance.HighVoltageConnectorActive;
		}
		else if (connection == ConnectionType.LV)
		{
			if (!isConnected)
			{
				return ColorManager.instance.LowVoltageConnector;
			}
			return ColorManager.instance.LowVoltageConnectorActive;
		}
		else if (connection == ConnectionType.Sewer)
		{
			if (!isConnected)
			{
				return ColorManager.instance.SewerConnector;
			}
			return ColorManager.instance.SewerConnectorActive;
		}
		else
		{
			if (connection != ConnectionType.Transport)
			{
				return ColorManager.instance.LowVoltageConnector;
			}
			if (!isConnected)
			{
				return ColorManager.instance.TransportConnector;
			}
			return ColorManager.instance.TransportConnectorActive;
		}
	}

	public void UpdateConnectorVisuals()
	{
		if (!this.Parent.MyBoard.IsCurrent)
		{
			return;
		}
		CardConnector drawingConnector = CitiesManager.instance.DrawingConnector;
		this.ConnectorIcon.sprite = this.GetSpriteForConnection(this.ConnectionType);
		this.targetScale = Vector3.one;
		this.targetPosition = this.BasePosition;
		bool flag = this.ConnectedNode != null;
		if (this.isActive)
		{
			if (!flag && this.currentVisualsState != CardConnector.VisualsState.ActiveUnconnected)
			{
				this.currentVisualsState = CardConnector.VisualsState.ActiveUnconnected;
				this.OutlineRect.Color = Color.black;
				this.ConnectorRect.Color = this.GetColorForConnection(this.ConnectionType, this.ConnectedNode != null);
				this.ConnectorIcon.sortingLayerID = (this.ConnectorRect.SortingLayerID = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Above")));
				this.OutlineRect.RenderQueue = (this.ConnectorRect.RenderQueue = 3500);
			}
			if (flag && this.currentVisualsState != CardConnector.VisualsState.ActiveConnected)
			{
				this.currentVisualsState = CardConnector.VisualsState.ActiveConnected;
				this.OutlineRect.Color = Color.black;
				this.ConnectorRect.Color = this.GetColorForConnection(this.ConnectionType, this.ConnectedNode != null);
				this.ConnectorIcon.sortingLayerID = (this.ConnectorRect.SortingLayerID = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Above")));
				this.OutlineRect.RenderQueue = (this.ConnectorRect.RenderQueue = 3500);
			}
			PerformanceHelper.SetActive(this.ConnectorIcon.gameObject, true);
			if (WorldManager.instance.HoveredDraggable == this)
			{
				this.targetScale = Vector3.one * 1.1f;
			}
			else
			{
				this.targetScale = Vector3.one;
			}
			if (drawingConnector != null && drawingConnector != this && (drawingConnector.ConnectionType != this.ConnectionType || drawingConnector.CardDirection == this.CardDirection))
			{
				this.targetScale = Vector3.zero;
			}
			if (this.IsHovered)
			{
				if (this.ConnectionType == ConnectionType.LV || this.ConnectionType == ConnectionType.HV)
				{
					string text = ((this.CardDirection == CardDirection.input) ? "label_connection_type_input" : "label_connection_type_output");
					string text2 = ((this.ConnectionType == ConnectionType.LV) ? "label_connection_low_voltage" : "label_connection_high_voltage");
					GameScreen.InfoBoxText = MewtationsLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = MewtationsLoc.Translate(text2) + " " + MewtationsLoc.Translate(text);
				}
				else if (this.ConnectionType == ConnectionType.Sewer)
				{
					GameScreen.InfoBoxText = MewtationsLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = MewtationsLoc.Translate("label_connection_sewer");
				}
				else if (this.ConnectionType == ConnectionType.Transport)
				{
					string text3 = ((this.CardDirection == CardDirection.input) ? "label_connection_type_input" : "label_connection_type_output");
					GameScreen.InfoBoxText = MewtationsLoc.Translate("label_connector_info");
					GameScreen.InfoBoxTitle = MewtationsLoc.Translate("label_connection_transport") + " " + MewtationsLoc.Translate(text3);
				}
			}
		}
		else
		{
			this.SetToBackground();
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.targetScale, 20f * Time.deltaTime);
		base.transform.localPosition = this.targetPosition;
	}

	private void SetToBackground()
	{
		if (this.currentVisualsState != CardConnector.VisualsState.Inactive)
		{
			this.currentVisualsState = CardConnector.VisualsState.Inactive;
			this.ConnectorIcon.sortingLayerID = (this.ConnectorRect.SortingLayerID = (this.OutlineRect.SortingLayerID = SortingLayer.NameToID("Default")));
			this.OutlineRect.RenderQueue = (this.ConnectorRect.RenderQueue = 3000);
			this.OutlineRect.Color = WorldManager.instance.CurrentBoard.BoardOptions.CardBackgroundPallete.Color2;
			this.ConnectorRect.Color = WorldManager.instance.CurrentBoard.BoardOptions.CardBackgroundPallete.Color;
		}
		this.targetScale = Vector3.one * 0.75f;
		this.targetPosition = this.BasePosition + Vector3.forward * 0.03f;
		if (Vector3.Distance(base.transform.localScale, this.targetScale) < 0.1f)
		{
			PerformanceHelper.SetActive(this.ConnectorIcon.gameObject, false);
		}
	}

	public override void Clicked()
	{
		if (this.isActive)
		{
			if (this.ConnectedNode != null)
			{
				if (CitiesManager.instance.DrawingConnector == null)
				{
					this.SetConnectedNode(null);
					CitiesManager.instance.StartDrawCable(this);
				}
				ValueTuple<AudioClip, float> startSoundForType = this.GetStartSoundForType(this.ConnectionType);
				AudioClip item = startSoundForType.Item1;
				float item2 = startSoundForType.Item2;
				AudioManager.me.PlaySound2D(item, 1f, item2);
				return;
			}
			if (CitiesManager.instance.DrawingConnector == null)
			{
				CitiesManager.instance.StartDrawCable(this);
				ValueTuple<AudioClip, float> startSoundForType2 = this.GetStartSoundForType(this.ConnectionType);
				AudioClip item3 = startSoundForType2.Item1;
				float item4 = startSoundForType2.Item2;
				AudioManager.me.PlaySound2D(item3, 1f, item4);
			}
		}
	}

	public void SetConnectedNode(CardConnector connector)
	{
		if (connector != null)
		{
			this.ConnectedNode = connector;
			connector.ConnectedNode = this;
			return;
		}
		if (this.ConnectedNode != null)
		{
			this.ConnectedNode.ConnectedNode = null;
		}
		this.ConnectedNode = null;
	}

	public SavedCardConnector ToSavedEnergyConnector()
	{
		if (this.ConnectedNode == null)
		{
			return null;
		}
		return new SavedCardConnector
		{
			UniqueId = this.GetConnectorUniqueId(),
			ConnectedNodeUniqueId = this.ConnectedNode.GetConnectorUniqueId()
		};
	}

	public string GetConnectorUniqueId()
	{
		string uniqueId = this.Parent.CardData.UniqueId;
		string text = this.CardDirection.ToString();
		string text2 = this.ConnectionType.ToString();
		int myIndex = this.GetMyIndex();
		return string.Format("{0}_{1}_{2}_{3}", new object[] { uniqueId, text2, text, myIndex });
	}

	private int GetMyIndex()
	{
		int num = 0;
		for (int i = 0; i < this.Parent.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = this.Parent.CardConnectorChildren[i];
			if (cardConnector == this)
			{
				return num;
			}
			if (cardConnector.ConnectionType == this.ConnectionType && cardConnector.CardDirection == this.CardDirection)
			{
				num++;
			}
		}
		throw new Exception();
	}

	public ValueTuple<AudioClip, float> GetStartSoundForType(ConnectionType connection)
	{
		if (connection == ConnectionType.HV || connection == ConnectionType.LV)
		{
			return new ValueTuple<AudioClip, float>(AudioManager.me.EnergyStart, 0.6f);
		}
		if (connection == ConnectionType.Sewer)
		{
			return new ValueTuple<AudioClip, float>(AudioManager.me.SewerStart, 0.7f);
		}
		if (connection == ConnectionType.Transport)
		{
			return new ValueTuple<AudioClip, float>(AudioManager.me.TransportStart, 0.8f);
		}
		return new ValueTuple<AudioClip, float>(null, 0f);
	}

	public AudioClip GetConnectSoundForType(ConnectionType connection)
	{
		if (connection == ConnectionType.HV || connection == ConnectionType.LV)
		{
			return AudioManager.me.EnergyConnected;
		}
		if (connection == ConnectionType.Sewer)
		{
			return AudioManager.me.SewerConnected;
		}
		if (connection == ConnectionType.Transport)
		{
			return AudioManager.me.TransportConnected;
		}
		return null;
	}

	public AudioClip GetStretchSoundForType(ConnectionType connection)
	{
		if (connection == ConnectionType.HV || connection == ConnectionType.LV)
		{
			return AudioManager.me.EnergyStrech;
		}
		if (connection == ConnectionType.Sewer)
		{
			return AudioManager.me.SewerStrech;
		}
		if (connection == ConnectionType.Transport)
		{
			return AudioManager.me.TransportStrech;
		}
		return null;
	}

	public bool HasEnergyOutput()
	{
		CardConnector.nodeTracker.Clear();
		return this.Parent.CardData.HasEnergyOutput(this, CardConnector.nodeTracker);
	}

	public bool HasEnergyInput()
	{
		return this.Parent.CardData.HasEnergyInput(this);
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanBeDragged()
	{
		return false;
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		return false;
	}

	protected override void ClampPos()
	{
	}

	public GameCard GetConnectedGameCard()
	{
		CardConnector connectedNode = this.ConnectedNode;
		if (connectedNode == null)
		{
			return null;
		}
		return connectedNode.Parent;
	}

	[HideInInspector]
	public string UniqueId;

	[HideInInspector]
	public GameCard Parent;

	public SpriteRenderer ConnectorIcon;

	[HideInInspector]
	public string ConnectedNodeUniqueId;

	[HideInInspector]
	public CardConnector ConnectedNode;

	public Rectangle ConnectorRect;

	public Rectangle OutlineRect;

	public CardDirection CardDirection;

	public ConnectionType ConnectionType;

	[HideInInspector]
	public Vector3 Middle;

	[HideInInspector]
	public Vector3 MiddleVelo;

	public float ClosedStateScale = 0.2f;

	private bool isActive;

	private Vector3 scaleRef;

	private Vector3 targetScale;

	private Vector3 targetPosition;

	private Vector3 BasePosition;

	private CardConnector.VisualsState currentVisualsState;

	private static List<CardConnector> nodeTracker = new List<CardConnector>();

	private enum VisualsState
	{
		None,
		ActiveUnconnected,
		Inactive,
		ActiveConnected
	}
}


