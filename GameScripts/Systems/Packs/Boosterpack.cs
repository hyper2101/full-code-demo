using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Boosterpack : Draggable
{
	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(this.PackData.nameOverride))
			{
				return this.PackData.nameOverride;
			}
			return SokLoc.Translate(this.PackData.NameTerm);
		}
	}

	protected override bool HasPhysics
	{
		get
		{
			return true;
		}
	}

	public string BoosterId
	{
		get
		{
			return this.PackData.BoosterId;
		}
	}

	public bool IsIntroPack
	{
		get
		{
			return this.PackData.IsIntroPack;
		}
	}

	public int Cost
	{
		get
		{
			return this.PackData.Cost;
		}
	}

	public Location BoosterLocation
	{
		get
		{
			return this.PackData.BoosterLocation;
		}
	}

	public List<CardBag> CardBags
	{
		get
		{
			return this.PackData.CardBags;
		}
	}

	public override bool CanBeDragged()
	{
		return !this.IsIntroPack && base.CanBeDragged();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.GetComponentsInChildren<MaterialChanger>(true, this.materialChangers);
		MaterialChanger component = base.GetComponent<MaterialChanger>();
		if (component != null)
		{
			this.materialChangers.Add(component);
		}
		foreach (MaterialChanger materialChanger in this.materialChangers)
		{
			materialChanger.Init();
		}
		this.startScale = base.transform.localScale;
		this.propBlock = new MaterialPropertyBlock();
		if (!WorldManager.instance.AllBoosters.Contains(this))
		{
			WorldManager.instance.AllBoosters.Add(this);
		}
		base.Start();
	}

	protected override void OnDestroy()
	{
		WorldManager.instance.AllBoosters.Remove(this);
		base.OnDestroy();
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		return !(draggable is GameCard) && base.CanBePushedBy(draggable);
	}

	protected override void Update()
	{
		this.PackRenderer.GetPropertyBlock(this.propBlock, 1);
		if (this.PackData.Icon != null)
		{
			this.propBlock.SetTexture("_IconTex", this.PackData.Icon.texture);
		}
		else
		{
			this.propBlock.SetTexture("_IconTex", SpriteManager.instance.EmptyTexture.texture);
		}
		this.PackRenderer.SetPropertyBlock(this.propBlock, 1);
		this.BoosterText.text = this.Name;
		int num = this.PackData.CardBags.Sum<CardBag>((CardBag x) => x.CardsInPack);
		this.CardCountText.text = num.ToString();
		if (num == 0)
		{
			this.CardCountText.transform.parent.gameObject.SetActive(false);
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.startScale, Time.deltaTime * 16f);
		base.Update();
	}

	private Vector3 GetCardVelocity()
	{
		if (this.TotalCardsInPack == 1)
		{
			Vector2 vector = Random.insideUnitCircle.normalized * 4f;
			this.Velocity = new Vector3?(new Vector3(vector.x, 6f, vector.y));
		}
		float num = (float)(this.TotalCardsInPack - this.PackData.CardBags.Sum<CardBag>((CardBag x) => x.CardsInPack));
		int totalCardsInPack = this.TotalCardsInPack;
		float num2 = num / (float)totalCardsInPack * 360f * 0.017453292f;
		return new Vector3(Mathf.Cos(num2) * 4.5f, 6f, Mathf.Sin(num2) * 4.5f);
	}

	public override void Clicked()
	{
		CardBag currentCardBag = this.PackData.CardBags.FirstOrDefault<CardBag>((CardBag x) => x.CardsInPack > 0);
		if (currentCardBag == null)
		{
			return;
		}
		this.WasClicked = true;
		ICardId cardId = currentCardBag.GetCard(true);
		int timesBoosterWasBoughtOnLocation = WorldManager.instance.GetTimesBoosterWasBoughtOnLocation(this.BoosterLocation);
		GameCamera.instance.Screenshake = 0.3f;
		if (this.BoosterLocation == Location.Mainland && timesBoosterWasBoughtOnLocation == 1)
		{
			if (currentCardBag.CardsInPack == 1)
			{
				cardId = (CardId)"berrybush";
			}
			else if (currentCardBag.CardsInPack == 0)
			{
				cardId = (CardId)"tree";
			}
		}
		if (this.BoosterId == "cities_weather" && this.TimesOpened == this.TotalCardsInPack - 1)
		{
			if (CitiesManager.instance.ShouldTriggerEvent())
			{
				cardId = (this.spawnedEvent = CitiesManager.instance.GetEvent());
			}
		}
		else
		{
			int cardCount = WorldManager.instance.CardQuery.GetCardCount<Worker>();
			if (this.BoosterLocation == Location.Cities && cardCount <= 1 && ((2 - cardCount < 0) ? 0 : (2 - cardCount)) - this.TimesOpened >= 0)
			{
				cardId = (CardId)"worker";
			}
		}
		if (WorldManager.instance.GetBoardWithLocation(this.BoosterLocation).BoardOptions.NewVillagerSpawnsFromPack)
		{
			int num = WorldManager.instance.CardQuery.GetCardCount<BaseVillager>((BaseVillager x) => x.CanBreed);
			num += WorldManager.instance.CardQuery.GetCardCount<TeenageVillager>();
			if (!this.IsIntroPack && (timesBoosterWasBoughtOnLocation == 7 || (timesBoosterWasBoughtOnLocation > 7 && timesBoosterWasBoughtOnLocation % 5 == 0)) && currentCardBag.CardsInPack == 0 && num <= 1)
			{
				cardId = (CardId)"villager";
			}
			if (this.MyBoard.BoardOptions.CanSpawnCombatIntro && timesBoosterWasBoughtOnLocation >= 10 && !WorldManager.instance.CurrentSave.FoundBoosterIds.Contains("combat_intro") && currentCardBag.CardsInPack == 0)
			{
				WorldManager.instance.CreateBoosterpack(base.transform.position, "combat_intro").SendIt();
			}
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardId, false, false, true);
		if (cardData == null)
		{
			Debug.LogError(string.Format("CardData is null after creating card with id '{0}'", cardId));
		}
		cardData.MyGameCard.RotWobble(1f);
		cardData.MyGameCard.Velocity = new Vector3?(this.GetCardVelocity());
		AudioManager.me.PlaySound2D(AudioManager.me.OpenBooster, Random.Range(0.9f, 1.1f), 0.3f);
		WorldManager.instance.GivenCards.Add(cardData.Id);
		if (currentCardBag.CardBagType != CardBagType.SetPack && Random.value <= 0.01f)
		{
			cardData.SetFoil();
		}
		this.TimesOpened++;
		this.SetHitEffect(delegate
		{
			if (currentCardBag.CardsInPack <= 0 && currentCardBag == this.PackData.CardBags[this.PackData.CardBags.Count - 1])
			{
				WorldManager.instance.CreateSmoke(this.transform.position);
				Object.Destroy(this.gameObject);
				QuestManager.instance.SpecialActionComplete(this.PackData.BoosterId + "_opened", null);
			}
		});
		if (this.TimesOpened == this.TotalCardsInPack)
		{
			WorldManager.instance.OnBoosterOpened(this.PackData.BoosterId);
			if (this.spawnedEvent != null)
			{
				CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(this.spawnedEvent.Id, true);
				if (cardFromId.MyCardType == CardType.Disaster)
				{
					WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_disaster");
				}
				else if (cardFromId.Id == "ufo_event")
				{
					WorldManager.instance.Cutscene.QueueCutscene("cities_event_ufo");
				}
				this.spawnedEvent = null;
			}
		}
		base.transform.localScale *= 1.2f;
		base.Clicked();
	}

	protected void SetHitEffect(Action after)
	{
		using (List<MaterialChanger>.Enumerator enumerator = this.materialChangers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				MaterialChanger mc = enumerator.Current;
				if (mc != null)
				{
					mc.SetMaterial(WorldManager.instance.HitMaterial);
					base.StartCoroutine(this.WaitFor(0.1f, delegate
					{
						mc.ResetMaterials();
					}));
				}
			}
		}
		base.StartCoroutine(this.WaitFor(0.1f, delegate
		{
			Action after2 = after;
			if (after2 == null)
			{
				return;
			}
			after2();
		}));
	}

	private IEnumerator WaitFor(float time, Action a)
	{
		yield return new WaitForSeconds(time);
		if (a != null)
		{
			a();
		}
		yield break;
	}

	protected override float Mass
	{
		get
		{
			return 10f;
		}
	}

	public BoosterpackData PackData;

	public MeshRenderer PackRenderer;

	private MaterialPropertyBlock propBlock;

	public TextMeshPro BoosterText;

	public TextMeshPro CardCountText;

	protected List<MaterialChanger> materialChangers = new List<MaterialChanger>();

	private Vector3 startScale;

	public int TotalCardsInPack;

	private CardId spawnedEvent;

	[HideInInspector]
	public int TimesOpened;

	[HideInInspector]
	public bool WasClicked;
}
