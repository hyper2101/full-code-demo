using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
	public Location Location
	{
		get
		{
			if (!this.locationSet)
			{
				this.locationSet = true;
				if (this.Id == "forest")
				{
					this._location = Location.Forest;
				}
				else if (this.Id == "main")
				{
					this._location = Location.Mainland;
				}
				else if (this.Id == "island")
				{
					this._location = Location.Island;
				}
				else if (this.Id == "happiness")
				{
					this._location = Location.Happiness;
				}
				else if (this.Id == "greed")
				{
					this._location = Location.Greed;
				}
				else if (this.Id == "death")
				{
					this._location = Location.Death;
				}
				else
				{
					if (!(this.Id == "cities"))
					{
						throw new ArgumentException();
					}
					this._location = Location.Cities;
				}
			}
			return this._location;
		}
	}

	public bool IsCurrent
	{
		get
		{
			return WorldManager.instance.CurrentBoard == this;
		}
	}

	public Bounds WorldBounds
	{
		get
		{
			Vector3 vector;
			Vector3 vector2;
			Quaternion quaternion;
			this.WorldCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
			return new Bounds(vector, vector2 * 2f + this.WorldSizeIncrease * new Vector3(1f, 0f, 0.58f) * 2f);
		}
	}

	public Bounds TightWorldBounds
	{
		get
		{
			if (!this.hasCachedTightBounds)
			{
				Vector3 vector;
				Vector3 vector2;
				Quaternion quaternion;
				this.TightWorldCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
				this.cachedTightBounds = new Bounds(vector, vector2 * 2f + this.WorldSizeIncrease * new Vector3(1f, 0f, 0.58f) * 2f);
				this.hasCachedTightBounds = true;
			}
			return this.cachedTightBounds;
		}
	}

	public string BoardName
	{
		get
		{
			return MewtationsLoc.Translate("board_" + this.Id + "_name");
		}
	}

	private void Awake()
	{
		if (this.BoardOptions.PostProcessVolume != null)
		{
			this.BoardOptions.PostProcessVolume.enabled = false;
		}
		this.MyMaterial = base.GetComponent<MeshRenderer>().sharedMaterial;
	}

	private void Start()
	{
		CreatePackLine componentInChildren = base.GetComponentInChildren<CreatePackLine>();
		if (componentInChildren != null)
		{
			componentInChildren.CreateBoosterBoxes(this.BoosterIds, this.BoardOptions.Currency);
			this.PackLineWidth = componentInChildren.TotalWidth;
		}
	}

	private void Update()
	{
		if (this.IsCurrent)
		{
			Shader.SetGlobalFloat("_WorldSizeIncrease", this.WorldSizeIncrease);
		}
		float num = WorldManager.instance.DetermineTargetWorldSize(this);
		this.WorldSizeIncrease = Mathf.Lerp(this.WorldSizeIncrease, num, Time.deltaTime * 12f);
		if (this.WorldSizeIncrease != this.PreviousWorldSizeIncrease && this.boardBackground != null)
		{
			this.boardBackground.UpdateBoardBackground();
		}
		this.TopBgElements.localPosition = Vector3.forward * this.WorldSizeIncrease * 0.58f;
		this.BottomBgElements.localPosition = Vector3.back * this.WorldSizeIncrease * 0.58f;
		this.LeftBgElements.localPosition = Vector3.left * this.WorldSizeIncrease;
		this.RightBgElements.localPosition = Vector3.right * this.WorldSizeIncrease;
		this.PreviousWorldSizeIncrease = this.WorldSizeIncrease;
		if (this.BoardOptions.PostProcessVolume != null)
		{
			this.BoardOptions.PostProcessVolume.enabled = this.IsCurrent;
		}
		this.hasCachedTightBounds = false;
	}

	public Vector3 NormalizedPosToWorldPos(Vector2 pos)
	{
		Bounds worldBounds = this.WorldBounds;
		float num = Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, pos.x);
		float num2 = Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, pos.y);
		return new Vector3(num, 0f, num2);
	}

	public Vector2 WorldPosToNormalizedPos(Vector3 pos)
	{
		Bounds worldBounds = this.WorldBounds;
		float num = Mathf.InverseLerp(worldBounds.min.x, worldBounds.max.x, pos.x);
		float num2 = Mathf.InverseLerp(worldBounds.min.z, worldBounds.max.z, pos.z);
		return new Vector2(num, num2);
	}

	public Vector3 MiddleOfBoard()
	{
		return this.NormalizedPosToWorldPos(new Vector2(0.5f, 0.5f));
	}

	public string Id = "";

	public BoxCollider WorldCollider;

	public BoxCollider TightWorldCollider;

	public Transform TopBgElements;

	public Transform BottomBgElements;

	public Transform LeftBgElements;

	public Transform RightBgElements;

	public Transform CameraIntroPosition;

	public Color CardHighlightColor;

	[HideInInspector]
	public TextAsset BoardPreset;

	[HideInInspector]
	public float WorldSizeIncrease;

	private float PreviousWorldSizeIncrease;

	public BoardBackground boardBackground;

	public List<string> BoosterIds;

	[HideInInspector]
	public float PackLineWidth;

	public BoardOptions BoardOptions;

	private Location _location;

	private bool locationSet;

	private Bounds cachedTightBounds;

	private bool hasCachedTightBounds;

	[HideInInspector]
	public Material MyMaterial;
}
