using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameCard : Draggable, IGameCardOrCardData
{
	protected override bool HasPhysics
	{
		get
		{
			return true;
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public override bool IsHovered
	{
		get
		{
			return base.IsHovered;
		}
	}

	public static float CardHeight
	{
		get
		{
			return PrefabManager.instance.GameCardPrefab.GetHeight();
		}
	}

	public GameCard TryGetNthChild(int n)
	{
		GameCard gameCard = this;
		for (int i = 0; i < n; i++)
		{
			if (!(gameCard.Child != null))
			{
				return null;
			}
			gameCard = gameCard.Child;
		}
		return gameCard;
	}

	public bool BeingHovered
	{
		get
		{
			return WorldManager.instance.HoveredCard == this || (this.IsParentOf(WorldManager.instance.HoveredCard) || this.IsChildOf(WorldManager.instance.HoveredCard));
		}
	}

	public override Vector3 AutoMoveSnapPosition
	{
		get
		{
			if (WorldManager.instance != null && WorldManager.instance.DraggingCard != null)
			{
				return this.CardNameText.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, -WorldManager.instance.CardOverlayOffset);
			}
			if (this.Child == null && this.Parent == null)
			{
				return base.transform.position;
			}
			return this.CardNameText.transform.position;
		}
	}

	public override bool CanBeAutoMovedTo
	{
		get
		{
			return (!(WorldManager.instance.DraggingCard != null) || !(this.Child != null)) && (!this.IsEquipped || (!(WorldManager.instance.DraggingCard == this.EquipmentHolder) && this.EquipmentHolder.ShowInventory)) && (!this.IsWorking || (!(WorldManager.instance.DraggingCard == this.WorkerHolder) && this.WorkerHolder.ShowInventory)) && !this.BeingDragged;
		}
	}

	public bool InventoryVisible
	{
		get
		{
			return this.ShowInventory;
		}
	}

	public bool IsWorkerInventory
	{
		get
		{
			CardData cardData = this.CardData;
			return cardData != null && cardData.WorkerAmount > 0;
		}
	}

	public bool TimerRunningInStack
	{
		get
		{
			return this.GetAllCardsInStack().Any<GameCard>((GameCard x) => x.TimerRunning);
		}
	}

	protected override void Awake()
	{
		this.Combat = new CardCombat(this);
		this.Stacking = new CardStacking(this);
		this.Visuals = new CardVisuals(this);
		base.Awake();
		base.transform.rotation = Quaternion.Euler(270f, 90f, 90f);
		this.propBlock = new MaterialPropertyBlock();
		this.combatCirclePropBlock = new MaterialPropertyBlock();
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
		this.CombatStatusCircle.gameObject.SetActiveFast(false);
		this.DropShadowRenderer.enabled = false;
		this.newCircleStartSize = this.NewCircle.transform.localScale;
		this.NewCircle.gameObject.SetActiveFast(true);
		this.NewCircle.transform.localScale = Vector3.zero;
		this.CombatCircleColor = this.CombatStatusCircle.color;
		this.StatusEffectBackground.transform.localScale = Vector3.zero;
		this.FoilParticles.emission.enabled = false;
		this.EquipmentRectangle.gameObject.SetActiveFast(true);
		this.WorkerRectangle.gameObject.SetActiveFast(true);
		this.SpecialText.gameObject.SetActiveFast(false);
		this.CoinText.gameObject.SetActiveFast(false);
		this.CoinIcon.gameObject.SetActiveFast(false);
		this.statusEffectBackgroundTransform = this.StatusEffectBackground.transform;
	}

	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		if (this.IsDemoCard)
		{
			this.startScale *= 0.2f;
			base.transform.localScale = this.startScale;
		}
		this.UpdateIcon();
		this.lastPosition = (this.TargetPosition = base.transform.position);
		this.UpdateCardPalette();
		this.SetColors();
		this.HighlightRectangle.enabled = false;
		if (!WorldManager.instance.AllCards.Contains(this) && !this.IsDemoCard)
		{
			WorldManager.instance.AllCards.Add(this);
		}
		if (!WorldManager.instance.UniqueIdToCard.ContainsKey(this.CardData.UniqueId))
		{
			WorldManager.instance.UniqueIdToCard[this.CardData.UniqueId] = this;
		}
		this.onOffBasePosition = this.OnOffInteractable.transform.localPosition;
		this.onOffTargetPosition = this.onOffBasePosition + new Vector3(0.09f, 0f, 0f);
		this.onOffTargetPos = this.onOffBasePosition;
		this.OnOffInteractable.gameObject.SetActive(false);
		if (!this.CardData.HasInventory)
		{
			Object.Destroy(this.HeadEquipmentPosition);
			Object.Destroy(this.HandEquipmentPosition);
			Object.Destroy(this.TorsoEquipmentPosition);
		}
	}

	public void UpdateIcon()
	{
		if (this.CardData.MyCardType == CardType.Ideas)
		{
			if (this.CardData.CardUpdateType == CardUpdateType.Main)
			{
				this.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Island)
			{
				this.IconRenderer.sprite = SpriteManager.instance.IslandIdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Spirit)
			{
				this.IconRenderer.sprite = SpriteManager.instance.SpiritIdeaIcon;
			}
			else if (this.CardData.CardUpdateType == CardUpdateType.Cities)
			{
				this.IconRenderer.sprite = SpriteManager.instance.CitiesIdeaIcon;
			}
			else
			{
				this.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
			}
		}
		if (this.CardData.Icon != null)
		{
			this.IconRenderer.sprite = this.CardData.Icon;
		}
	}

	public void UpdateCardPalette()
	{
		this.myCardPalette = ColorManager.instance.GetPaletteForCard(this.CardData);
	}

	public void ToggleDirection()
	{
		if (this.CardData.OutputDir == Vector3.zero)
		{
			this.CardData.OutputDir = Vector3.right;
		}
		else if (this.CardData.OutputDir == Vector3.right)
		{
			this.CardData.OutputDir = Vector3.back;
		}
		else if (this.CardData.OutputDir == Vector3.back)
		{
			this.CardData.OutputDir = Vector3.left;
		}
		else if (this.CardData.OutputDir == Vector3.left)
		{
			this.CardData.OutputDir = Vector3.forward;
		}
		else if (this.CardData.OutputDir == Vector3.forward)
		{
			this.CardData.OutputDir = Vector3.zero;
		}
		QuestManager.instance.SpecialActionComplete("output_direction_changed", this.CardData);
	}

	private void SetColors()
	{
		this.CombatStatusCircle.color = this.CombatCircleColor;
		this.CombatStatusCircle.color = Color.red;
		if (this.myCardPalette == null)
		{
			Debug.LogError("Could not find card color pallet");
			return;
		}
		Color color = this.myCardPalette.Color;
		Color color2 = this.myCardPalette.Color2;
		Color color3 = this.myCardPalette.Icon;
		if (this.IsHit)
		{
			this.CombatStatusCircle.color = Color.white;
			color2 = (color = (color3 = Color.white));
		}
		this.CardRenderer.shadowCastingMode = ((this.IsEquipped || this.IsWorking) ? ShadowCastingMode.Off : ShadowCastingMode.On);
		this.CardRenderer.GetPropertyBlock(this.propBlock, 2);
		this.propBlock.SetColor(this.propColor, color);
		this.propBlock.SetColor(this.propColor2, color2);
		this.propBlock.SetColor(this.propIconColor, color3);
		Texture2D texture2D = null;
		bool flag = false;
		if (this.CardData is ResourceChest || this.CardData is FoodWarehouse)
		{
			texture2D = SpriteManager.instance.ChestIconSecondary.texture;
		}
		else if (this.CardData is ResourceMagnet)
		{
			texture2D = SpriteManager.instance.MagnetIconSecondary.texture;
		}
		bool flag2 = texture2D != null;
		this.propBlock.SetFloat(this.propHasSecondaryIcon, flag2 ? 1f : 0f);
		this.propBlock.SetFloat(this.propHasOutputDir, flag ? 1f : 0f);
		if (texture2D != null)
		{
			this.propBlock.SetTexture(this.propSecondaryTex, texture2D);
		}
		float num = ((this.CardData is Equipable) ? 0.3f : 1f);
		this.propBlock.SetFloat(this.propBigShineStrength, (this.CardData is Equipable) ? 0f : 1f);
		this.propBlock.SetFloat(this.propShineStrength, num);
		this.propBlock.SetFloat(this.propFoil, (this.CardData.IsFoil || this.CardData.IsShiny || this.CardData is Equipable) ? 1f : 0f);
		this.propBlock.SetFloat(this.propDamaged, this.CardData.IsDamaged ? 1f : 0f);
		if (this.IconRenderer.sprite != null)
		{
			this.propBlock.SetTexture(this.propIconTex, this.IconRenderer.sprite.texture);
		}
		else
		{
			this.propBlock.SetTexture(this.propIconTex, SpriteManager.instance.EmptyTexture.texture);
		}
		this.CardRenderer.SetPropertyBlock(this.propBlock, 2);
		if (this.SpecialText.color != color)
		{
			this.SpecialText.color = color;
		}
		this.SpecialIcon.color = color3;
		this.IconRenderer.color = color3;
		if (this.CoinText.color != color)
		{
			this.CoinText.color = color;
		}
		this.CoinIcon.color = color3;
		if (this.EquipmentButton.Color != color)
		{
			this.EquipmentButton.Color = color;
		}
		if (this.WorkerButton.Color != color)
		{
			this.WorkerButton.Color = color;
		}
		Color color4 = color3;
		color4.a = 0.5f;
		this.WorkerInventoryIcon.color = (this.HasAnyWorkers() ? color3 : color4);
		if (this.CardNameText.color != color3)
		{
			this.CardNameText.color = color3;
		}
	}

	private static Sprite GetSpriteForAttackType(AttackType type)
	{
		if (type == AttackType.Magic)
		{
			return SpriteManager.instance.MagicFightIcon;
		}
		if (type == AttackType.Melee)
		{
			return SpriteManager.instance.MeleeFightIcon;
		}
		if (type == AttackType.Ranged)
		{
			return SpriteManager.instance.RangedFightIcon;
		}
		if (type == AttackType.Foot)
		{
			return SpriteManager.instance.FootFightIcon;
		}
		if (type == AttackType.Armour)
		{
			return SpriteManager.instance.ArmourFightIcon;
		}
		if (type == AttackType.Air)
		{
			return SpriteManager.instance.AirFightIcon;
		}
		return null;
	}

	protected override void OnDestroy()
	{
		if (WorldManager.instance != null)
		{
			WorldManager.instance.AllCards.Remove(this);
			if (WorldManager.instance.UniqueIdToCard.ContainsKey(this.CardData.UniqueId) && WorldManager.instance.UniqueIdToCard[this.CardData.UniqueId] == this)
			{
				WorldManager.instance.UniqueIdToCard.Remove(this.CardData.UniqueId);
			}
		}
		base.OnDestroy();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(this.debugBounds.center, this.debugBounds.size);
	}

	public virtual void DestroyCard(bool spawnSmoke = false, bool playSound = true)
	{
		this.RemoveFromStack();
		WorldManager.instance.AllCards.Remove(this);
		WorldManager.instance.UniqueIdToCard.Remove(this.CardData.UniqueId);
		this.Destroyed = true;
		this.CardData.OnDestroyCard();
		if (playSound)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, Random.Range(0.8f, 1.2f), 0.3f);
		}
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.transform.position);
		}
		Curse curse = this.CardData as Curse;
		if (curse != null)
		{
			WorldManager.instance.ActiveCurses.Remove(curse);
		}
		if (this.CardData.HasInventory)
		{
			foreach (GameCard gameCard in this.EquipmentChildren)
			{
				gameCard.EquipmentHolder = null;
				gameCard.IsEquipped = false;
				gameCard.DestroyCard(false, false);
			}
		}
		if (this.CardData.WorkerAmount > 0)
		{
			foreach (GameCard gameCard2 in this.WorkerChildren)
			{
				gameCard2.WorkerHolder = null;
				gameCard2.IsWorking = false;
				gameCard2.DestroyCard(false, false);
			}
		}
		if (this.Combatable != null && this.Combatable.InConflict)
		{
			this.Combatable.MyConflict.LeaveConflict(this.Combatable);
		}
		Object.Destroy(base.gameObject);
	}

	public void SetChild(GameCard card)
	{
		this.cardsInvolved.Clear();
		this.cardsInvolved.Add(this);
		if (card == this)
		{
			Debug.LogError("Child is same as Parent");
			return;
		}
		if (card == null)
		{
			if (this.Child != null)
			{
				this.cardsInvolved.Add(this.Child);
				this.Child.Parent = null;
			}
			this.Child = null;
			this.NotifyStackUpdate(this.cardsInvolved);
			return;
		}
		this.Child = card;
		card.Parent = this;
		this.cardsInvolved.Add(card);
		this.NotifyStackUpdate(this.cardsInvolved);
	}

	public void SetParent(GameCard card)
	{
		this.cardsInvolved.Clear();
		this.cardsInvolved.Add(this);
		if (card == this)
		{
			Debug.LogError("Child is same as Parent");
			return;
		}
		if (card == null)
		{
			if (this.Parent != null)
			{
				this.cardsInvolved.Add(this.Parent);
				this.Parent.Child = null;
			}
			this.Parent = null;
			this.NotifyStackUpdate(this.cardsInvolved);
			return;
		}
		this.Parent = card;
		card.Child = this;
		this.cardsInvolved.Add(card);
		this.NotifyStackUpdate(this.cardsInvolved);
	}

	public bool HasParent
	{
		get
		{
			return this.Parent != null;
		}
	}

	public bool HasChild
	{
		get
		{
			return this.Child != null;
		}
	}

	public void RemoveFromStack()
	{
		this.SetParent(null);
		this.SetChild(null);
	}

	private void NotifyStackUpdate(List<GameCard> cardsInvolved)
	{
		foreach (GameCard gameCard in cardsInvolved)
		{
			gameCard.GetRootCard().StackUpdate = true;
			gameCard.StackUpdate = true;
		}
	}

	public void RemoveFromParent()
	{
		if (this.Parent != null)
		{
			this.Parent.SetChild(null);
		}
		this.SetParent(null);
	}

	public override bool CanBePushed()
	{
		return (!(this.CardData is Food) || !WorldManager.instance.InEatingAnimation) && !(this.CardData is Spirit) && !(this.CardData is CityAdvisor) && !this.IsWorking && !this.IsEquipped && !this.BeingDragged && this.PushEnabled;
	}

	protected override float Mass
	{
		get
		{
			float num = 1f;
			if (this.CardData is Mob)
			{
				num += 50f;
			}
			if (this.CardData.MyCardType == CardType.Structures && this.CardData.IsBuilding)
			{
				num += 8f;
			}
			if (this.CardData is HeavyFoundation)
			{
				num += 1000f;
			}
			if (this.Child != null)
			{
				num += this.Child.Mass;
			}
			return num;
		}
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		if (this.IsEquipped || this.IsWorking)
		{
			return false;
		}
		if (draggable is Boosterpack && WorldManager.instance.CurrentBoard.Id == "cities" && this.GetRootCard().CardData.MyCardType == CardType.Structures)
		{
			return false;
		}
		GameCard gameCard = draggable as GameCard;
		if (gameCard != null)
		{
			if (gameCard.IsChildOf(this) || gameCard.IsParentOf(this))
			{
				return false;
			}
			if (gameCard.BounceTarget != null)
			{
				return false;
			}
			if (gameCard.Destroyed)
			{
				return false;
			}
			if (!gameCard.PushEnabled)
			{
				return false;
			}
			if (gameCard.CardData is Food && WorldManager.instance.InEatingAnimation)
			{
				return false;
			}
			if (WorldManager.instance.CurrentBoard.Id == "cities" && this.GetRootCard().CardData.MyCardType == CardType.Structures && (gameCard.CardData is Resource || gameCard.CardData is Food))
			{
				return false;
			}
			if (gameCard.CardData is Spirit || gameCard.CardData is CityAdvisor)
			{
				return false;
			}
			if (gameCard.CardData is Energy)
			{
				return false;
			}
			if (gameCard.IsEquipped || gameCard.IsWorking)
			{
				return false;
			}
			if (!this.CardData.CanBePushedBy(gameCard.CardData))
			{
				return false;
			}
		}
		return base.CanBePushedBy(draggable);
	}

	public override bool CanBeDragged()
	{
		Combatable combatable = this.CardData as Combatable;
		if (combatable != null && combatable.BeingAttacked)
		{
			return false;
		}
		if (WorldManager.instance.RemovingCards)
		{
			Boat boat = this.GetRootCard().CardData as Boat;
			if (boat != null && boat.InSailOff)
			{
				return false;
			}
		}
		return !this.BeingDragged && this.CardData.CanBeDragged && this.FaceUp;
	}

	public override void Clicked()
	{
		if (!this.FaceUp)
		{
			this.FaceUp = true;
		}
		if (this.DragTag == "inventory")
		{
			this.InventoryInteractable.Clicked();
			this.WorkerInventoryInteractable.Clicked();
		}
		else
		{
			this.CardData.Clicked();
		}
		this.WasClicked = true;
		base.Clicked();
	}

	public void ForceUpdate()
	{
		this.Update();
	}

	public void Equip(Equipable equipable)
	{
		GameCard myGameCard = equipable.MyGameCard;
		this.EquipmentChildren.Add(myGameCard);
		myGameCard.EquipmentHolder = this;
		myGameCard.IsEquipped = true;
		myGameCard.RemoveFromStack();
		this.CardData.OnEquipItem(equipable);
	}

	public void Unequip(Equipable equipable)
	{
		GameCard myGameCard = equipable.MyGameCard;
		this.EquipmentChildren.Remove(myGameCard);
		myGameCard.EquipmentHolder = null;
		myGameCard.IsEquipped = false;
		this.CardData.OnUnequipItem(equipable);
		if (this.Combatable != null && this.Combatable.HealthPoints > this.Combatable.ProcessedCombatStats.MaxHealth)
		{
			this.Combatable.HealthPoints = this.Combatable.ProcessedCombatStats.MaxHealth;
		}
	}

	public void EquipWorker(Worker worker, int index)
	{
		GameCard myGameCard = worker.MyGameCard;
		worker.WorkerIndex = index;
		this.WorkerChildren.Add(myGameCard);
		myGameCard.WorkerHolder = this;
		myGameCard.IsWorking = true;
		myGameCard.RemoveFromStack();
		this.CardData.OnEquipItem(null);
	}

	public void UnequipWorker(GameCard worker)
	{
		this.WorkerChildren.Remove(worker);
		worker.CardData.WorkerIndex = -1;
		worker.WorkerHolder = null;
		worker.IsWorking = false;
		this.GetRootCard().StackUpdate = true;
		CardData cardData = this.CardData;
		if (cardData == null)
		{
			return;
		}
		cardData.OnUnequipItem(null);
	}

	protected override void Bounce()
	{
		if (this.HasParent)
		{
			this.BounceTarget = null;
		}
		if (this.BounceTarget != null)
		{
			GameCard gameCard = this.BounceTarget;
			if (gameCard.Child != null)
			{
				gameCard = gameCard.GetLeafCard();
			}
			this.BounceTarget = null;
			if (gameCard == this)
			{
				return;
			}
			if (gameCard.BounceTarget != null)
			{
				return;
			}
			if (gameCard.GetCardInCombatInStack() != null)
			{
				return;
			}
			if (gameCard.BeingDragged)
			{
				return;
			}
			GameCard cardWithStatusInStack = gameCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
			{
				return;
			}
			if (gameCard.CardData.CanHaveCardOnTop(this.CardData, false))
			{
				this.SetParent(gameCard);
				this.Velocity = null;
				AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, Random.Range(0.8f, 1.2f), 0.3f);
			}
		}
		base.Bounce();
	}

	protected override void Update()
	{
		if (!this.IsDemoCard && !this.MyBoard.IsCurrent)
		{
			return;
		}
		if (this.HasChild && this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Fire && this.Child.CardData.Id == "water")
			{
				this.Child.DestroyCard(false, true);
				this.CardData.SetCardUndamaged();
				WorldManager.instance.CreateSmoke(this.Position);
				AudioManager.me.PlaySound2D(AudioManager.me.ExtinguishCardSound, Random.Range(0.9f, 1.1f), 0.3f);
			}
			else
			{
				if (this.CardData.DamageType == CardDamageType.Drought)
				{
					if (this.CardData.ChildrenMatchingPredicate((CardData x) => x.Id == "water").Count >= 3)
					{
						this.CardData.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == "water", 3);
						this.CardData.SetCardUndamaged();
						WorldManager.instance.CreateSmoke(this.Position);
						AudioManager.me.PlaySound2D(AudioManager.me.DroughtSolved, Random.Range(0.9f, 1.1f), 0.3f);
						goto IL_022C;
					}
				}
				if (this.CardData.DamageType == CardDamageType.Damaged && this.Child.CardData is ICurrency && this.CardData.GetDollarCountInStack(true) >= this.CardData.GetRepairCost())
				{
					List<ICurrency> list = this.CardData.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>();
					CitiesManager.instance.TryUseDollars(list, this.CardData.GetRepairCost(), true, false, false);
					this.CardData.SetCardUndamaged();
					AudioManager.me.PlaySound2D(AudioManager.me.RepairCardSound, Random.Range(0.9f, 1.1f), 0.3f);
				}
			}
		}
		IL_022C:
		this.CardData.UpdateCard();
		this.SetColors();
		string name = this.CardData.Name;
		if (this.CardNameText.text != name)
		{
			this.CardNameText.text = this.CardData.Name;
		}
		Vector3 vector = (this.IsNew ? this.newCircleStartSize : Vector3.zero);
		this.NewCircle.transform.localScale = Vector3.Lerp(this.NewCircle.transform.localScale, vector, Time.deltaTime * 20f);
		bool flag = WorldManager.instance.DraggingCard != null && WorldManager.instance.DraggingCard.CardData.Id == this.CardData.Id;
		if (this.BeingDragged || this.WasClicked || this.Child != null || this.Parent != null || this.InConflict || this.GetCardWithStatusInStack() != null || flag || this.CardData is Spirit || this.CardData is CityAdvisor)
		{
			this.IsNew = false;
		}
		if (this.Child != null && !(this.Child.CardData is Equipable))
		{
			this.ShowInventory = false;
		}
		if (this.BeingDragged)
		{
			this.ShowInventory = false;
		}
		if (this.Combatable != null && this.Combatable.InAttack)
		{
			this.ShowInventory = false;
		}
		this.FoilParticles.emission.enabled = !this.IsDemoCard && (this.CardData.IsFoil || this.CardData.Id == "goblet");
		PerformanceHelper.SetActive(this.CombatStatusCircle.gameObject, this.InConflict || (this.Combatable != null && this.Combatable is Enemy && !this.IsDemoCard));
		if (this.Combatable != null)
		{
			this.CombatStatusCircle.sprite = GameCard.GetSpriteForAttackType(this.Combatable.ProcessedAttackType);
			this.CombatStatusCircle.GetPropertyBlock(this.combatCirclePropBlock);
			float num = (this.Combatable.InConflict ? this.Combatable.TimeToAttackNormalized : 1f);
			this.combatCirclePropBlock.SetFloat("_FillAmount", num);
			this.CombatStatusCircle.SetPropertyBlock(this.combatCirclePropBlock);
		}
		PerformanceHelper.SetActive(this.SpecialText.gameObject, this.SpecialValue != null);
		if (this.SpecialValue != null)
		{
			this.SpecialText.text = this.SpecialValue.Value.ToStringCached();
		}
		PerformanceHelper.SetActive(this.SpecialIcon.gameObject, this.SpecialValue != null || this.ShowSpecialIcon);
		int value = this.CardData.GetValue();
		if (value != -1)
		{
			this.CoinText.text = value.ToStringCached();
			PerformanceHelper.SetActive(this.CoinIcon.gameObject, true);
			PerformanceHelper.SetActive(this.CoinText.gameObject, true);
		}
		else
		{
			PerformanceHelper.SetActive(this.CoinIcon.gameObject, false);
			PerformanceHelper.SetActive(this.CoinText.gameObject, false);
		}
		this.UpdateShowInventory();
		this.UpdateShowWorkerInventory();
		if (this.CardData.HasInventory)
		{
			this.HandInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Weapon) ? this.colOn : this.colOff);
			this.TorsoInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Torso) ? this.colOn : this.colOff);
			this.HeadInventoryIcon.color = (this.CardData.HasEquipableOfEquipableType(EquipableType.Head) ? this.colOn : this.colOff);
		}
		this.DropShadowRenderer.enabled = this.IsEquipped && this.EquipmentHolder.ShowInventory;
		this.DropShadowRenderer.enabled = this.IsWorking && this.WorkerHolder.ShowInventory;
		this.OnOffInteractable.gameObject.SetActiveFast(false);
		Vector3 vector2 = this.startScale;
		if ((this.IsEquipped || this.IsWorking) && !this.BeingDragged)
		{
			vector2 = this.startScale * 0.8f;
		}
		if (!this.IsDemoCard)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, vector2, Time.deltaTime * 12f);
		}
		if (!this.IsDemoCard)
		{
			this.UpdatePosition();
		}
		Vector3 position = base.transform.position;
		position.y = -position.z * 0.001f;
		this.EquipmentRectangle.position = position + this.equipmentRectangleStartOffset;
		this.WorkerRectangle.position = position + this.equipmentRectangleStartOffset;
		if (!this.IsDemoCard && !this.FaceUp)
		{
			this.flipTimer += Time.deltaTime * WorldManager.instance.PhysicsTimeScale;
			if (this.flipTimer >= 0.1f)
			{
				this.FaceUp = true;
			}
		}
		this.wobbleRotVelo -= Time.deltaTime * this.RotWobbleSpringiness;
		if (this.wobbleRotVelo <= 0f)
		{
			this.wobbleRotVelo = 0f;
		}
		float num2 = this.RotWobbleAmp * Mathf.Sin(this.wobbleRotVelo * this.RotWobbleSpeed) * this.wobbleRotVelo;
		if (this.AutoRotWobble)
		{
			this.rotWobbleTimer += Time.deltaTime;
			if (this.rotWobbleTimer > this.AutoRotWobbleTimer)
			{
				this.rotWobbleTimer -= this.AutoRotWobbleTimer;
				this.RotWobble(this.AutoRotWobbleAmount);
			}
		}
		bool flag2 = true;
		if (!this.IsDemoCard)
		{
			if (this.IsEquipped)
			{
				if (this.EquipmentHolder.ShowInventory && !this.BeingDragged)
				{
					Transform myEquipmentStackPosition = this.GetMyEquipmentStackPosition();
					base.transform.localRotation = Camera.main.transform.localRotation;
					base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, myEquipmentStackPosition.localEulerAngles.z);
				}
				else if (!this.BeingDragged)
				{
					flag2 = false;
				}
			}
			else if (this.IsWorking)
			{
				if (this.WorkerHolder.ShowInventory && !this.BeingDragged)
				{
					Transform transformAtIndex = this.WorkerHolder.WorkerTransformHolder.GetTransformAtIndex(this.CardData.WorkerIndex);
					base.transform.localRotation = Camera.main.transform.localRotation;
					base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, transformAtIndex.localEulerAngles.z);
				}
				else if (!this.BeingDragged)
				{
					flag2 = false;
				}
			}
			else
			{
				float num3 = (this.FaceUp ? 90f : 270f);
				this.curZ = Mathf.Lerp(this.curZ, num3, Time.deltaTime * 14f * WorldManager.instance.PhysicsTimeScale);
				if (this.Parent != null)
				{
					this.curZ = num3;
				}
				base.transform.localRotation = Quaternion.Euler(this.curZ, 0f + num2 + this.ZRotOffset, 0f);
			}
		}
		else
		{
			this.SetDemoCardRotation();
		}
		PerformanceHelper.SetActive(this.Visuals.gameObject, flag2);
		if (this.Parent == null)
		{
			this.snappedToParent = false;
		}
		if (WorldManager.instance.CurrentBoard != null && this.HighlightActive)
		{
			this.HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
		}
		this.HighlightRectangle.enabled = this.HighlightActive;
		if (this.HighlightActive)
		{
			this.HighlightRectangle.DashOffset += Time.deltaTime;
			if (this.HighlightRectangle.DashOffset >= 1f)
			{
				this.HighlightRectangle.DashOffset -= 1f;
			}
		}
		this.lastPosition = base.transform.position;
		this.UpdateTimer();
		if (this.removedChild != null && !this.removedChild.BeingDragged)
		{
			this.removedChild = null;
			this.StackUpdate = true;
		}
		this.UpdateStatusEffectElements();
		this.UpdateCardAnimations();
		if (this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Damaged)
			{
				this.CardData.AddStatusEffect(new StatusEffect_Damaged());
			}
			if (this.CardData.DamageType == CardDamageType.Fire)
			{
				this.CardData.AddStatusEffect(new StatusEffect_OnFire());
			}
			if (this.CardData.DamageType == CardDamageType.Drought)
			{
				this.CardData.AddStatusEffect(new StatusEffect_Drought());
			}
		}
		else
		{
			this.CardData.RemoveStatusEffect<StatusEffect_Damaged>();
			this.CardData.RemoveStatusEffect<StatusEffect_OnFire>();
			this.CardData.RemoveStatusEffect<StatusEffect_Drought>();
		}
		if (this.IsHovered && this.CardData.IsDamaged)
		{
			if (this.CardData.DamageType == CardDamageType.Damaged)
			{
				Tooltip.Text = "<b>" + SokLoc.Translate("label_damaged") + "</b>\n" + SokLoc.Translate("label_damaged_card_cost", new LocParam[]
				{
					LocParam.Create("amount", this.CardData.GetRepairCost().ToStringCached()),
					LocParam.Create("icon", Icons.Dollar)
				});
			}
			if (this.CardData.DamageType == CardDamageType.Fire)
			{
				Tooltip.Text = "<b>" + SokLoc.Translate("label_on_fire") + "</b>\n" + SokLoc.Translate("label_fire_card_cost");
			}
		}
	}

	private bool HasAnyWorkers()
	{
		List<GameCard> workerChildren = this.CardData.MyGameCard.WorkerChildren;
		for (int i = 0; i < workerChildren.Count; i++)
		{
			if (workerChildren[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	private void animateOnOffInteractable()
	{
		bool flag = false;
		if (!this.CardData.WorkerAmountMet())
		{
			flag = false;
		}
		if (WorldManager.instance.CurrentView != ViewType.Default)
		{
			flag = true;
		}
		if (this.CardData.CanToggleCardOnOff())
		{
			if (this.OnOffInteractable.Velocity == null && this.onOffBasePosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude < 0.001f && this.onOffBasePosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude > -0.001f)
			{
				if (flag)
				{
					this.OnOffInteractable.gameObject.SetActive(true);
					this.onOffTargetPos = this.onOffTargetPosition;
				}
				else
				{
					this.OnOffInteractable.gameObject.SetActive(false);
				}
			}
			else if (!flag && this.OnOffInteractable.Velocity == null && this.onOffTargetPosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude < 0.001f && this.onOffTargetPosition.magnitude - this.OnOffInteractable.transform.localPosition.magnitude > -0.001f)
			{
				this.onOffTargetPos = this.onOffBasePosition;
			}
		}
		else
		{
			this.OnOffInteractable.gameObject.SetActive(false);
		}
		this.OnOffInteractable.transform.localPosition = FRILerp.Spring(this.OnOffInteractable.transform.localPosition, this.onOffTargetPos, 20f, 30f, ref this.onOffVelocity);
	}

	public void UpdateCardAnimations()
	{
		for (int i = 0; i < this.CardAnimations.Count; i++)
		{
			CardAnimation cardAnimation = this.CardAnimations[i];
			if (!cardAnimation.HasStarted)
			{
				cardAnimation.Start();
			}
			cardAnimation.Update();
			if (cardAnimation.IsDone)
			{
				this.CardAnimations.RemoveAt(i);
				i--;
			}
			else if (cardAnimation.IsBlocking)
			{
				break;
			}
		}
	}

	public void CreateCardConnectors()
	{
		this.CardData.EnergyConnectors.OrderBy<CardConnectorData, ConnectionType>((CardConnectorData x) => x.EnergyConnectionStrength);
		foreach (CardConnectorData cardConnectorData in this.CardData.EnergyConnectors)
		{
			int energyConnectionAmount = cardConnectorData.EnergyConnectionAmount;
			float num = ((cardConnectorData.EnergyConnectionType == CardDirection.input) ? (-0.19f) : 0.19f);
			for (int i = 0; i < energyConnectionAmount; i++)
			{
				Vector3 vector = new Vector3(num, (float)i * this.ConnectorAmountOffset - (float)(energyConnectionAmount / 2) * this.ConnectorAmountOffset + this.ConnectorAmountOffset / 2f * ((energyConnectionAmount % 2 == 0) ? 1f : 0f) - this.CardTextOffset, -0.03f);
				GameObject gameObject = Object.Instantiate<GameObject>(this.EnergyConnectorPrefab, Vector3.zero, base.transform.rotation, this.EnergyConnectorTransform);
				gameObject.transform.localPosition = vector;
				CardConnector component = gameObject.GetComponent<CardConnector>();
				component.InitializeEnergyNode(cardConnectorData, this);
				this.CardConnectorChildren.Add(component);
			}
		}
	}

	private void UpdateConnectors()
	{
		foreach (CardConnector cardConnector in this.CardConnectorChildren)
		{
			if (WorldManager.instance.CurrentBoard.Id != "cities")
			{
				cardConnector.gameObject.SetActive(false);
				break;
			}
			if (WorldManager.instance.CurrentView == ViewType.Default)
			{
				cardConnector.gameObject.SetActive(true);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Energy)
			{
				cardConnector.gameObject.SetActive(cardConnector.ConnectionType == ConnectionType.LV || cardConnector.ConnectionType == ConnectionType.HV);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Sewer)
			{
				cardConnector.gameObject.SetActive(cardConnector.ConnectionType == ConnectionType.Sewer);
			}
			else if (WorldManager.instance.CurrentView == ViewType.Transport)
			{
				cardConnector.gameObject.SetActive(cardConnector.ConnectionType == ConnectionType.Transport);
			}
		}
	}

	private void UpdateShowInventory()
	{
		bool flag = this.CardData.WorkerAmount > 0 && this.Child == null && !this.CardData.HasInventory;
		bool flag2 = this.CardData.HasInventory && this.Child == null && this.EquipmentChildren.Count > 0;
		PerformanceHelper.SetActive(this.EquipmentButton.gameObject, flag2);
		PerformanceHelper.SetActive(this.InventoryInteractable.gameObject, flag2);
		if (this.ShowInventory && !flag2 && !flag)
		{
			this.ShowInventory = false;
		}
	}

	private void UpdateShowWorkerInventory()
	{
		bool flag = this.CardData.WorkerAmount > 0 && this.Child == null && !this.CardData.HasInventory && !this.IsDemoCard;
		PerformanceHelper.SetActive(this.WorkerButton.gameObject, flag);
		PerformanceHelper.SetActive(this.WorkerInventoryInteractable.gameObject, flag);
	}

	private GameCard.PositionType DeterminePositionType()
	{
		if (this.CardAnimations.Count > 0)
		{
			return GameCard.PositionType.InAnimation;
		}
		if (this.IsEquipped)
		{
			if (this.BeingDragged)
			{
				return GameCard.PositionType.None;
			}
			return GameCard.PositionType.IsEquipped;
		}
		else if (this.IsWorking)
		{
			if (this.BeingDragged)
			{
				return GameCard.PositionType.None;
			}
			return GameCard.PositionType.IsWorking;
		}
		else if (this.InConflict)
		{
			if (this.BeingDragged)
			{
				return GameCard.PositionType.None;
			}
			if (this.InAttack)
			{
				return GameCard.PositionType.InAttack;
			}
			return GameCard.PositionType.InConflict;
		}
		else
		{
			if (this.Parent != null)
			{
				return GameCard.PositionType.InStack;
			}
			if (this.Parent == null)
			{
				return GameCard.PositionType.IsRoot;
			}
			return GameCard.PositionType.None;
		}
	}

	private void UpdatePosition()
	{
		GameCard.PositionType positionType = this.DeterminePositionType();
		if (positionType == GameCard.PositionType.InConflict)
		{
			this.TargetPosition = this.Combatable.MyConflict.GetPositionInConflict(this.Combatable);
			base.transform.position = Vector3.Lerp(base.transform.position, this.TargetPosition, Time.deltaTime * 20f);
		}
		else if (positionType == GameCard.PositionType.InAttack)
		{
			AttackAnimation currentAttackAnimation = this.Combatable.CurrentAttackAnimation;
			base.transform.position = currentAttackAnimation.Position;
			this.TargetPosition = currentAttackAnimation.TargetPosition;
		}
		else if (positionType == GameCard.PositionType.InAnimation)
		{
			CardAnimation cardAnimation = this.CardAnimations[0];
			base.transform.position = cardAnimation.Position;
			this.TargetPosition = cardAnimation.TargetPosition;
		}
		else if (positionType == GameCard.PositionType.IsEquipped)
		{
			if (this.EquipmentHolder.InventoryVisible)
			{
				this.TargetPosition = this.GetMyEquipmentStackPosition().position;
				if (this.IsHovered)
				{
					this.TargetPosition -= base.transform.forward * 0.1f;
				}
				base.transform.position = this.TargetPosition;
			}
			else
			{
				this.TargetPosition = this.EquipmentHolder.transform.position + new Vector3(0f, -0.1f, 0f);
				base.transform.position = this.TargetPosition;
			}
		}
		else if (positionType == GameCard.PositionType.IsWorking)
		{
			if (this.WorkerHolder.InventoryVisible)
			{
				this.TargetPosition = this.WorkerHolder.WorkerTransformHolder.GetTransformAtIndex(this.CardData.WorkerIndex).position;
				if (this.IsHovered)
				{
					this.TargetPosition -= base.transform.forward * 0.1f;
				}
				base.transform.position = this.TargetPosition;
			}
			else
			{
				this.TargetPosition = this.WorkerHolder.transform.position + new Vector3(0f, -0.1f, 0f);
				base.transform.position = this.TargetPosition;
			}
		}
		else if (positionType == GameCard.PositionType.InStack)
		{
			this.SetToParentPosition(false);
			this.TargetPosition = base.transform.position;
		}
		else if (positionType == GameCard.PositionType.IsRoot || positionType == GameCard.PositionType.None)
		{
			if (this.Velocity == null)
			{
				Vector3 targetPosition = this.TargetPosition;
				float num = 20f;
				if (this.SetY)
				{
					targetPosition.y = -targetPosition.z * 0.001f;
					targetPosition.y += (this.BeingDragged ? 0.1f : 0f);
					if (this.IsHovered && this.CanBeDragged() && WorldManager.instance.CanInteract)
					{
						targetPosition.y += 0.06f;
					}
					if (this.CardData is Spirit || this.CardData is CityAdvisor)
					{
						targetPosition.y += 0.25f;
					}
				}
				else
				{
					num = 10f + WorldManager.instance.EndOfMonthSpeedup * 3f;
				}
				base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, Time.deltaTime * num);
			}
			this.UpdateChildPositions(false);
		}
		if (this.closeToTargetPositionCallback != null && Vector3.Distance(base.transform.position, this.TargetPosition) < 0.1f)
		{
			this.closeToTargetPositionCallback();
		}
	}

	public Transform GetEquipmentStackPosition(EquipableType equipableType)
	{
		if (equipableType == EquipableType.Head)
		{
			return this.HeadEquipmentPosition.transform;
		}
		if (equipableType == EquipableType.Torso)
		{
			return this.TorsoEquipmentPosition.transform;
		}
		if (equipableType == EquipableType.Weapon)
		{
			return this.HandEquipmentPosition.transform;
		}
		throw new ArgumentException(string.Format("EquipableType does not have a stack position set for {0}", equipableType));
	}

	public void ToggleInventory()
	{
		this.OpenInventory(!this.ShowInventory);
	}

	public void ToggleCardOnOff()
	{
		this.CardData.ToggleCardOnOff();
	}

	public void OpenInventory(bool showInventory)
	{
		if (showInventory == this.ShowInventory)
		{
			return;
		}
		this.ShowInventory = showInventory;
		if (this.ShowInventory)
		{
			foreach (GameCard gameCard in WorldManager.instance.AllCards)
			{
				if (gameCard != this && gameCard.ShowInventory)
				{
					gameCard.ShowInventory = false;
				}
			}
		}
	}

	public void StatusEffectsChanged()
	{
		foreach (StatusEffect statusEffect in this.CardData.StatusEffects)
		{
			if (!this.ElementExistsForStatusEffect(statusEffect))
			{
				StatusEffectElement statusEffectElement = this.CreateElementForStatusEffect(statusEffect);
				this.StatusEffectElements.Add(statusEffectElement);
			}
		}
		for (int i = 0; i < this.StatusEffectElements.Count; i++)
		{
			if (!this.CardData.StatusEffects.Contains(this.StatusEffectElements[i].MyStatusEffect))
			{
				this.StatusEffectElements[i].DestroyMe = true;
			}
		}
		List<StatusEffectElement> list = this.StatusEffectElements.Where<StatusEffectElement>((StatusEffectElement x) => !x.DestroyMe).ToList<StatusEffectElement>();
		for (int j = 0; j < list.Count; j++)
		{
			float num = (float)j * this.DistanceBetweenStatusses - (float)(this.CardData.StatusEffects.Count - 1) * this.DistanceBetweenStatusses * 0.5f;
			list[j].TargetLocalPosition = new Vector3(num, 0f, -0.001f);
		}
	}

	private void UpdateStatusEffectElements()
	{
		Vector3 vector = ((this.StatusEffectElements.Count == 0) ? Vector3.zero : Vector3.one);
		this.statusEffectBackgroundTransform.localScale = Vector3.Lerp(this.statusEffectBackgroundTransform.localScale, vector, Time.deltaTime * 12f);
		PerformanceHelper.SetActive(this.StatusEffectBackground.gameObject, this.statusEffectBackgroundTransform.localScale.sqrMagnitude > 0.001f);
		float num = 0.1125f + (float)(this.StatusEffectElements.Count - 1) * this.DistanceBetweenStatusses;
		this.statusEffectBackgroundWidth = Mathf.Lerp(this.statusEffectBackgroundWidth, num, Time.deltaTime * 12f);
		if (Mathf.Abs(this.statusEffectBackgroundWidth - this.StatusEffectBackground.Width) > 0.01f)
		{
			this.StatusEffectBackground.Width = this.statusEffectBackgroundWidth;
		}
	}

	public void SetDemoCardRotation()
	{
		if (this.FaceUp)
		{
			base.transform.rotation = Camera.main.transform.rotation;
			return;
		}
		base.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
	}

	private Transform GetMyEquipmentStackPosition()
	{
		if (!this.IsEquipped)
		{
			throw new Exception("Not equipped!");
		}
		return this.EquipmentHolder.GetEquipmentStackPosition(((Equipable)this.CardData).EquipableType);
	}

	protected override void LateUpdate()
	{
		if (this.MyBoard != null && !this.MyBoard.IsCurrent)
		{
			return;
		}
		this.PushAwayFromOthers();
		if (this.Parent == null && !this.IsEquipped && !this.IsWorking)
		{
			this.ClampPos();
		}
		if (this.Parent != null)
		{
			this.LastParent = this.Parent;
		}
	}

	public void SetFaceUp(bool faceUp)
	{
		this.FaceUp = faceUp;
		this.curZ = (this.FaceUp ? 90f : 270f);
		base.transform.localRotation = Quaternion.Euler(this.curZ, 0f, 0f);
	}

	public override void SendIt()
	{
		if (this.MyBoard.Id == "cities" && this.HasParent)
		{
			this.Velocity = new Vector3?(this.GetRootCard().CardData.OutputDir * 7f);
		}
		else
		{
			base.SendIt();
		}
		this.RotWobble(1f);
	}

	public GameCard FindNextGameCardInDirection(Vector3 direction, CardType? type = null)
	{
		float num = float.MinValue;
		GameCard gameCard = null;
		foreach (GameCard gameCard2 in WorldManager.instance.AllCards)
		{
			if (gameCard2.gameObject.activeInHierarchy && !(gameCard2 == WorldManager.instance.DraggingDraggable))
			{
				if (gameCard2.MyBoard == null)
				{
					GameCard gameCard3 = gameCard2;
					Debug.Log(((gameCard3 != null) ? gameCard3.ToString() : null) + " does not have a board");
				}
				else if (gameCard2.MyBoard.IsCurrent && gameCard2.CanBeAutoMovedTo)
				{
					if (type != null)
					{
						CardType myCardType = gameCard2.CardData.MyCardType;
						CardType? cardType = type;
						if (!((myCardType == cardType.GetValueOrDefault()) & (cardType != null)))
						{
							continue;
						}
					}
					Vector3 vector = gameCard2.AutoMoveSnapPosition - base.transform.position;
					float num2 = Vector3.Dot(direction, vector);
					if ((double)num2 > 0.3)
					{
						float num3 = num2 / vector.sqrMagnitude;
						if (num3 > num && vector.sqrMagnitude < 1f)
						{
							num = num3;
							gameCard = gameCard2;
						}
					}
				}
			}
		}
		return gameCard;
	}

	public override void SendDirection(Vector3 direction)
	{
		this.RotWobble(1f);
		base.SendDirection(direction);
	}

	public override void SendToPosition(Vector3 position)
	{
		this.RotWobble(1f);
		base.SendToPosition(position);
	}

	public void SendToPositionCallback(Vector3 position, Action callback)
	{
		this.RotWobble(1f);
		this.TargetPosition = position;
		this.closeToTargetPositionCallback = callback;
	}

	public void RotWobble(float amount)
	{
		this.wobbleRotVelo = amount;
	}

	public bool IsCollapsed
	{
		get
		{
			return this.BeingDragged && (WorldManager.instance.NearbyCardTarget != null || (this.GetRootCard().GetChildCount() >= 10 && !WorldManager.instance.IsShiftDragging));
		}
	}

	public Combatable Combatable
	{
		get
		{
			return this.CardData as Combatable;
		}
	}

	public bool InConflict
	{
		get
		{
			return this.Combatable != null && this.Combatable.InConflict;
		}
	}

	public bool InAttack
	{
		get
		{
			return this.Combatable != null && this.Combatable.InAttack;
		}
	}

	private void SetToParentPosition(bool hardSetPos = false)
	{
		Vector3 vector;
		if (this.IsCollapsed)
		{
			vector = this.Parent.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, -WorldManager.instance.CollapsedCardOverlayOffset);
		}
		else
		{
			vector = this.Parent.transform.position + new Vector3(0f, WorldManager.instance.CardOverlayHeightOffset, -WorldManager.instance.CardOverlayOffset);
		}
		if (!this.snappedToParent)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, vector, Time.deltaTime * 20f);
			if (Vector3.Distance(base.transform.position, vector) < 0.001f)
			{
				this.snappedToParent = true;
			}
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, vector, Time.deltaTime * 20f);
			Vector3 position = base.transform.position;
			position.y = vector.y;
			base.transform.position = position;
		}
		if (hardSetPos)
		{
			base.transform.position = (this.TargetPosition = vector);
		}
	}

	public void UpdateChildPositions(bool hardSetPos = false)
	{
		if (this.Child == null)
		{
			return;
		}
		this.Child.SetToParentPosition(hardSetPos);
		this.Child.UpdateChildPositions(hardSetPos);
	}

	public Conflict GetOverlappingConflict()
	{
		foreach (Conflict conflict in WorldManager.instance.GetAllConflicts())
		{
			if (conflict.GetBounds().Intersects(base.DraggableBounds))
			{
				return conflict;
			}
		}
		return null;
	}

	public List<GameCard> GetOverlappingCardsInBox(Vector3 center, Vector3 size)
	{
		List<GameCard> list = new List<GameCard>();
		int num = Physics.OverlapBoxNonAlloc(center, size * 0.5f, this.hits, Quaternion.identity, -5, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			GameCard component = this.hits[i].gameObject.GetComponent<GameCard>();
			if (component != null && component != this)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public List<GameCard> GetOverlappingCards()
	{
		List<GameCard> list = new List<GameCard>();
		int num = PhysicsExtensions.OverlapBoxNonAlloc(this.boxCollider, this.hits, -5, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			GameCard component = this.hits[i].gameObject.GetComponent<GameCard>();
			if (component != null && component != this)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public void StartBlueprintTimer(float time, TimerAction a, string status, string actionId, string blueprintId, int subprintIndex, CardData consumer, bool skipWorkerEnergyCheck = false)
	{
		if (this.IsDemoCard)
		{
			return;
		}
		if (this.BeingDragged)
		{
			return;
		}
		GameCard gameCard = this.GetRootCard();
		if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
		{
			gameCard = gameCard.Child;
		}
		if (this.HasTransportCard() && actionId != "sail_off" && actionId != "leave_spirit" && actionId != "take_portal")
		{
			return;
		}
		if (this.removedChild != null && this.removedChild.BeingDragged)
		{
			return;
		}
		if (this.TimerActionId == actionId && this.TimerBlueprintId == blueprintId && this.TimerSubprintIndex == subprintIndex)
		{
			this.TargetTimerTime = time;
			return;
		}
		if (!this.CardData.IsOn)
		{
			return;
		}
		if (!skipWorkerEnergyCheck && !gameCard.CardData.ShouldStartTimerWorkers(actionId))
		{
			return;
		}
		if (!skipWorkerEnergyCheck && !gameCard.CardData.ShouldStartTimerEnergy(consumer, actionId))
		{
			return;
		}
		if (gameCard.CardData.IsDamaged)
		{
			return;
		}
		this.TimerBlueprintId = blueprintId;
		this.TimerSubprintIndex = subprintIndex;
		this.SkipCitiesChecks = skipWorkerEnergyCheck;
		this.InitTimer(time, a, status, actionId, true);
	}

	public void StartTimer(float time, TimerAction a, string status, string actionId, bool withStatusBar = true, bool skipWorkerEnergyCheck = false, bool skipDamageOnOffCheck = false)
	{
		if (this.IsDemoCard)
		{
			return;
		}
		if (this.BeingDragged)
		{
			return;
		}
		if (this.TimerActionId == actionId)
		{
			this.TargetTimerTime = time;
			return;
		}
		if (!this.CardData.IsOn && !skipDamageOnOffCheck)
		{
			return;
		}
		if (!skipWorkerEnergyCheck && !this.CardData.ShouldStartTimerWorkers(actionId))
		{
			return;
		}
		if (!skipWorkerEnergyCheck && !this.CardData.HasEnergyInput(null))
		{
			return;
		}
		if (!skipWorkerEnergyCheck && !this.CardData.HasSewerConnected())
		{
			return;
		}
		if (this.CardData.IsDamaged && !skipDamageOnOffCheck)
		{
			return;
		}
		this.InitTimer(time, a, status, actionId, withStatusBar);
	}

	private void InitTimer(float time, TimerAction a, string status, string actionId, bool withStatusBar = true)
	{
		if (withStatusBar)
		{
			Statusbar statusbar = Object.Instantiate<Statusbar>(PrefabManager.instance.StatusBarPrefab);
			statusbar.StatusTime = time;
			statusbar.ParentCard = this;
			this.CurrentStatusbar = statusbar;
		}
		this.Status = status;
		this.TimerRunning = true;
		this.TimerAction = a;
		this.TimerActionId = actionId;
		this.CurrentTimerTime = 0f;
		this.TargetTimerTime = time;
	}

	public void CancelTimer(string actionId)
	{
		if (this.removedChild != null && this.removedChild.BeingDragged)
		{
			return;
		}
		if (!this.TimerRunning || this.TimerActionId != actionId)
		{
			return;
		}
		this.StopTimer();
	}

	private void StopTimer()
	{
		this.TimerRunning = false;
		this.TimerActionId = "";
		this.Status = "";
		this.TimerBlueprintId = "";
		this.TimerSubprintIndex = 0;
		this.CurrentTimerTime = 0f;
		this.SkipCitiesChecks = false;
		if (this.CurrentStatusbar != null)
		{
			this.CurrentStatusbar.DestroyMe = true;
			this.CurrentStatusbar = null;
		}
	}

	public void CancelAnyTimer()
	{
		if (!this.TimerRunning)
		{
			return;
		}
		this.StopTimer();
	}

	public void UpdateTimer()
	{
		if (!this.TimerRunning)
		{
			return;
		}
		if (this.removedChild == null || !this.removedChild.BeingDragged)
		{
			this.CurrentTimerTime += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.CurrentStatusbar != null)
		{
			this.CurrentStatusbar.Paused = this.removedChild != null && this.removedChild.BeingDragged;
		}
		if (this.CurrentTimerTime >= this.TargetTimerTime)
		{
			this.TimerRunning = false;
			if (!this.ShouldCompleteTimer(this.TimerActionId))
			{
				this.TimerActionId = "";
				this.Status = "";
				this.TimerBlueprintId = "";
				this.TimerSubprintIndex = 0;
				this.CurrentTimerTime = 0f;
				this.CurrentStatusbar.DestroyMe = true;
				this.CurrentStatusbar = null;
				return;
			}
			try
			{
				this.TimerAction();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
			if (this.TimerActionId == "finish_blueprint")
			{
				QuestManager.instance.ActionComplete(WorldManager.instance.GetBlueprintWithId(this.TimerBlueprintId), this.TimerActionId, this.CardData);
			}
			else
			{
				QuestManager.instance.ActionComplete(this.CardData, this.TimerActionId, null);
			}
			this.TimerActionId = "";
			this.Status = "";
			this.TimerBlueprintId = "";
			this.TimerSubprintIndex = 0;
			this.CurrentTimerTime = 0f;
			if (this.CurrentStatusbar != null)
			{
				this.CurrentStatusbar.DestroyMe = true;
			}
			this.CurrentStatusbar = null;
		}
	}

	public virtual bool ShouldCompleteTimer(string timerActionId)
	{
		return this.CardData.ShouldCompleteTimer(timerActionId);
	}

	public bool HasTransportCard()
	{
		GameCard gameCard = this.GetRootCard();
		if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
		{
			gameCard = gameCard.Child;
		}
		return gameCard.CardData is Boat || gameCard.CardData is Spirit || gameCard.CardData is Portal;
	}

	public bool ElementExistsForStatusEffect(StatusEffect effect)
	{
		using (List<StatusEffectElement>.Enumerator enumerator = this.StatusEffectElements.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.MyStatusEffect == effect)
				{
					return true;
				}
			}
		}
		return false;
	}

	public StatusEffectElement CreateElementForStatusEffect(StatusEffect effect)
	{
		StatusEffectElement statusEffectElement = Object.Instantiate<StatusEffectElement>(PrefabManager.instance.StatusEffectElementPrefab);
		statusEffectElement.SetStatusEffect(this, effect);
		statusEffectElement.transform.SetParent(this.StatusEffectElementParent);
		statusEffectElement.transform.localRotation = Quaternion.identity;
		statusEffectElement.transform.localScale = Vector3.zero;
		float num = (float)this.StatusEffectElements.Count * this.DistanceBetweenStatusses - (float)(this.StatusEffectElements.Count - 1) * this.DistanceBetweenStatusses * 0.5f;
		statusEffectElement.transform.localPosition = new Vector3(num, 0f, -0.001f);
		return statusEffectElement;
	}

	public bool IsPartOfStack()
	{
		return this.Parent != null || this.Child != null;
	}

	public GameCard GetCardWithStatusInStack()
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard.TimerRunning)
			{
				return gameCard;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public int GetCardIndex()
	{
		GameCard gameCard = this.GetRootCard();
		int num = 0;
		while (gameCard != null)
		{
			if (gameCard == this)
			{
				return num;
			}
			gameCard = gameCard.Child;
			num++;
		}
		return -1;
	}

	public GameCard GetCardInCombatInStack()
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard.Combatable != null && gameCard.Combatable.InConflict)
			{
				return gameCard;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public List<GameCard> GetAllCardsInStack()
	{
		GameCard rootCard = this.GetRootCard();
		List<GameCard> childCards = rootCard.GetChildCards();
		childCards.Insert(0, rootCard);
		return childCards;
	}

	public CardData HasCardInStack(Predicate<CardData> pred)
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				return gameCard.CardData;
			}
			gameCard = gameCard.Child;
		}
		return null;
	}

	public bool IsPartOfSameStack(GameCard otherCard)
	{
		GameCard gameCard = this.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard == otherCard)
			{
				return true;
			}
			gameCard = gameCard.Child;
		}
		return false;
	}

	public string GetStackSummary()
	{
		return WorldManager.instance.GetStackSummary(this.GetAllCardsInStack());
	}

	public bool IsChildOf(GameCard card)
	{
		if (card == null)
		{
			return false;
		}
		for (GameCard gameCard = this.Parent; gameCard != null; gameCard = gameCard.Parent)
		{
			if (gameCard == card)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsParentOf(GameCard card)
	{
		if (card == null)
		{
			return false;
		}
		for (GameCard gameCard = this.Child; gameCard != null; gameCard = gameCard.Child)
		{
			if (gameCard == card)
			{
				return true;
			}
		}
		return false;
	}

	public void SetCollidersInStack(bool enabled)
	{
		for (GameCard gameCard = this; gameCard != null; gameCard = gameCard.Child)
		{
			gameCard.boxCollider.enabled = enabled;
		}
	}

	public List<GameCard> GetChildCards()
	{
		List<GameCard> list = new List<GameCard>();
		GameCard gameCard = this.Child;
		while (gameCard != null)
		{
			list.Add(gameCard);
			gameCard = gameCard.Child;
		}
		return list;
	}

	public GameCard GetRootCard()
	{
		GameCard gameCard = this;
		while (gameCard.Parent != null)
		{
			gameCard = gameCard.Parent;
		}
		return gameCard;
	}

	public GameCard GetLeafCard()
	{
		GameCard gameCard = this;
		while (gameCard.Child != null)
		{
			gameCard = gameCard.Child;
		}
		return gameCard;
	}

	public int GetChildCount()
	{
		GameCard gameCard = this;
		int num = 0;
		while (gameCard.Child != null)
		{
			num++;
			gameCard = gameCard.Child;
		}
		return num;
	}

	public int GetStackCount()
	{
		GameCard gameCard = this.GetRootCard();
		int num = 1;
		while (gameCard.Child != null)
		{
			num++;
			gameCard = gameCard.Child;
		}
		return num;
	}

	private void NotifyChildDrag(GameCard card)
	{
		this.removedChild = card;
	}

	public override void StopDragging()
	{
		if (this.Parent != null)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, Random.Range(0.8f, 1.2f), 0.3f);
		}
		else if (this.CardData.PickupSound != null && this.CardData.PickupSoundGroup == PickupSoundGroup.Custom)
		{
			AudioManager.me.PlaySound2D(this.CardData.PickupSound, Random.Range(0.8f, 1f), 0.5f);
		}
		else
		{
			List<AudioClip> soundForPickupSoundGroup = AudioManager.me.GetSoundForPickupSoundGroup(this.CardData.PickupSoundGroup);
			AudioManager.me.PlaySound2D(soundForPickupSoundGroup, Random.Range(0.8f, 1f), 0.5f);
		}
		GameCard gameCard = this.Child;
		while (gameCard != null)
		{
			gameCard.BeingDragged = false;
			gameCard = gameCard.Child;
		}
		this.CardData.StoppedDragging();
		this.StackUpdate = true;
		base.StopDragging();
	}

	public override void StartDragging()
	{
		if (this.CardData.PickupSound != null && this.CardData.PickupSoundGroup == PickupSoundGroup.Custom)
		{
			AudioManager.me.PlaySound2D(this.CardData.PickupSound, Random.Range(1f, 1.2f), 0.5f);
		}
		else
		{
			List<AudioClip> soundForPickupSoundGroup = AudioManager.me.GetSoundForPickupSoundGroup(this.CardData.PickupSoundGroup);
			AudioManager.me.PlaySound2D(soundForPickupSoundGroup, Random.Range(1f, 1.2f), 0.5f);
		}
		GameCard gameCard = this.Parent;
		while (gameCard != null)
		{
			gameCard.NotifyChildDrag(this);
			gameCard = gameCard.Parent;
		}
		if (this.Parent != null)
		{
			this.SetParent(null);
		}
		gameCard = this.Child;
		while (gameCard != null)
		{
			gameCard.BeingDragged = true;
			gameCard = gameCard.Child;
		}
		this.BounceTarget = null;
		base.StartDragging();
	}

	public void Clampieee()
	{
		this.ClampPos();
	}

	protected override void ClampPos()
	{
		if (this.IsDemoCard || !this.SetY)
		{
			return;
		}
		int childCount = this.GetChildCount();
		float num = (float)childCount * WorldManager.instance.CardOverlayOffset;
		if (this.IsCollapsed)
		{
			num = (float)childCount * WorldManager.instance.CollapsedCardOverlayOffset;
		}
		this.curHeight = Mathf.Lerp(this.curHeight, num, Time.deltaTime * 12f);
		base.transform.position = this.ClampPos2(base.transform.position);
		this.TargetPosition = this.ClampPos2(this.TargetPosition);
	}

	public float GetHeight()
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		PrefabManager.instance.GameCardPrefab.boxCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return vector2.y * 2f;
	}

	public float GetWidth()
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		PrefabManager.instance.GameCardPrefab.boxCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return vector2.x * 2f;
	}

	public Bounds GetBounds()
	{
		return new Bounds(base.transform.position, new Vector3(this.GetWidth(), 0.01f, this.GetHeight()));
	}

	private Vector3 ClampPos2(Vector3 p)
	{
		Bounds bounds = (this.BeingDragged ? this.MyBoard.WorldBounds : this.MyBoard.TightWorldBounds);
		Vector3 vector;
		this.boxCollider.ToWorldSpaceBox2(out vector);
		float num = 0.1f;
		p.x = Mathf.Clamp(p.x, bounds.min.x + vector.x + num, bounds.max.x - vector.x - num);
		p.z = Mathf.Clamp(p.z, bounds.min.z + vector.y + num + this.curHeight, bounds.max.z - vector.y - num);
		return p;
	}

	public SavedCard ToSavedCard()
	{
		SavedCard savedCard = new SavedCard();
		savedCard.CardPosition = base.transform.position;
		savedCard.CardPrefabId = this.CardData.Id;
		savedCard.UniqueId = this.CardData.UniqueId;
		savedCard.IsFoil = this.CardData.IsFoil;
		savedCard.FaceUp = this.FaceUp;
		savedCard.IsDamaged = this.CardData.IsDamaged;
		savedCard.DamageType = this.CardData.DamageType;
		if (this.Parent != null)
		{
			savedCard.ParentUniqueId = this.Parent.CardData.UniqueId;
		}
		if (this.EquipmentHolder != null)
		{
			savedCard.EquipmentHolderUniqueId = this.EquipmentHolder.CardData.UniqueId;
		}
		if (this.WorkerHolder != null)
		{
			savedCard.WorkerHolderUniqueId = this.WorkerHolder.CardData.UniqueId;
			savedCard.WorkerIndex = this.CardData.WorkerIndex;
		}
		savedCard.ExtraCardData = this.CardData.GetExtraCardData();
		savedCard.TimerRunning = this.TimerRunning;
		savedCard.WithStatusBar = this.CurrentStatusbar != null;
		savedCard.TimerActionId = this.TimerActionId;
		savedCard.Status = this.Status;
		savedCard.CurrentTimerTime = this.CurrentTimerTime;
		savedCard.TargetTimerTime = this.TargetTimerTime;
		savedCard.TimerBlueprintId = this.TimerBlueprintId;
		savedCard.SkipCitiesChecks = this.SkipCitiesChecks;
		savedCard.SubprintIndex = this.TimerSubprintIndex;
		savedCard.BoardId = this.MyBoard.Id;
		savedCard.StatusEffects = this.CardData.StatusEffects.Select<StatusEffect, SavedStatusEffect>((StatusEffect x) => x.ToSavedStatusEffect()).ToList<SavedStatusEffect>();
		savedCard.CardConnectors = (from x in this.CardConnectorChildren
			select x.ToSavedEnergyConnector() into x
			where x != null
			select x).ToList<SavedCardConnector>();
		return savedCard;
	}

	public void SetHitEffect(Action after = null)
	{
		this.IsHit = true;
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
		base.StartCoroutine(this.WaitFor(0.11f, delegate
		{
			this.IsHit = false;
			Action after2 = after;
			if (after2 == null)
			{
				return;
			}
			after2();
		}));
	}

	public bool HasConnectorOfType(ConnectionType connectionType)
	{
		for (int i = 0; i < this.CardData.EnergyConnectors.Count; i++)
		{
			if (this.CardData.EnergyConnectors[i].EnergyConnectionStrength == connectionType)
			{
				return true;
			}
		}
		return false;
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

	public TextMeshPro CardNameText;

	public SpriteRenderer IconRenderer;

	public CardData CardData;

	public CardCombat Combat { get; private set; }
	public CardStacking Stacking { get; private set; }
	public CardVisuals Visuals { get; private set; }

	public GameCard Parent { get => Stacking.Parent; set => Stacking.Parent = value; }
	public GameCard Child { get => Stacking.Child; set => Stacking.Child = value; }
	public GameCard LastParent { get => Stacking.LastParent; set => Stacking.LastParent = value; }

	public Transform HitTextPosition;

	public Transform Visuals;

	public int ConnectorOutputIndex;

	public bool IsEquipped;

	public bool IsWorking;

	public bool ShowInventory;

	public GameCard EquipmentHolder;

	public List<GameCard> EquipmentChildren;

	public GameCard WorkerHolder;

	public List<GameCard> WorkerChildren = new List<GameCard>();

	public GameObject EnergyConnectorPrefab;

	public Transform EnergyConnectorTransform;

	private Vector3 startScale;

	public Renderer CardRenderer;

	public Rectangle HighlightRectangle;

	public SpriteRenderer CoinIcon;

	public TextMeshPro CoinText;

	public TextMeshPro SpecialText;

	public SpriteRenderer SpecialIcon;

	public SpriteRenderer CombatStatusCircle;

	public SpriteRenderer DropShadowRenderer;

	public Transform EquipmentRectangle;

	public Transform WorkerRectangle;

	public WorkerTransformHolder WorkerTransformHolder;

	public InventoryInteractable InventoryInteractable;

	public InventoryInteractable WorkerInventoryInteractable;

	public OnOffInteractable OnOffInteractable;

	private Vector3 onOffBasePosition;

	private Vector3 onOffTargetPosition;

	public SpriteRenderer HeadInventoryIcon;

	public SpriteRenderer TorsoInventoryIcon;

	public SpriteRenderer HandInventoryIcon;

	public SpriteRenderer WorkerInventoryIcon;

	public GameObject HeadEquipmentPosition;

	public GameObject TorsoEquipmentPosition;

	public GameObject HandEquipmentPosition;

	public Rectangle EquipmentButton;

	public Rectangle WorkerButton;

	public int? SpecialValue;

	public bool HighlightActive;

	private Vector3 lastPosition;

	public Vector3 SpawnRotation;

	private bool snappedToParent;

	private MaterialPropertyBlock propBlock;

	private MaterialPropertyBlock combatCirclePropBlock;

	public bool FaceUp;

	public SpriteRenderer NewCircle;

	private Vector3 newCircleStartSize;

	public ParticleSystem FoilParticles;

	protected List<MaterialChanger> materialChangers = new List<MaterialChanger>();

	[HideInInspector]
	public bool IsDemoCard;

	public GameCard BounceTarget;

	[HideInInspector]
	public bool PushEnabled = true;

	[HideInInspector]
	public bool SetY = true;

	[Header("Status")]
	public float DistanceBetweenStatusses = 0.01f;

	[HideInInspector]
	public List<StatusEffectElement> StatusEffectElements = new List<StatusEffectElement>();

	public Vector3 equipmentRectangleStartOffset;

	[HideInInspector]
	public bool ShowSpecialIcon;

	[HideInInspector]
	public bool StackUpdate;

	public CardPalette myCardPalette { get => Visuals.myCardPalette; set => Visuals.myCardPalette = value; }

	public List<CardAnimation> CardAnimations = new List<CardAnimation>();

	[HideInInspector]
	private Action closeToTargetPositionCallback;

	[HideInInspector]
	public List<CardConnector> CardConnectorChildren = new List<CardConnector>();

	public Color CombatCircleColor;

	private int propColor = Shader.PropertyToID("_Color");

	private int propColor2 = Shader.PropertyToID("_Color2");

	private int propIconColor = Shader.PropertyToID("_IconColor");

	private int propHasSecondaryIcon = Shader.PropertyToID("_HasSecondaryIcon");

	private int propHasOutputDir = Shader.PropertyToID("_HasOutputDir");

	private int propSecondaryTex = Shader.PropertyToID("_SecondaryTex");

	private int propBigShineStrength = Shader.PropertyToID("_BigShineStrength");

	private int propShineStrength = Shader.PropertyToID("_ShineStrength");

	private int propFoil = Shader.PropertyToID("_Foil");

	private int propDamaged = Shader.PropertyToID("_Damaged");

	private int propIconTex = Shader.PropertyToID("_IconTex");

	[HideInInspector]
	public bool Destroyed;

	private List<GameCard> cardsInvolved = new List<GameCard>();

	public bool WasClicked;

	public bool IsNew;

	public float ZRotOffset;

	private Vector3 onOffVelocity;

	private Vector3 onOffTargetPos;

	private Color colOff = new Color(0f, 0f, 0f, 0.5f);

	private Color colOn = new Color(0f, 0f, 0f, 1f);

	private float ConnectorAmountOffset = 0.077f;

	private float CardTextOffset = 0.01f;

	public Rectangle StatusEffectBackground;

	private Transform statusEffectBackgroundTransform;

	private float statusEffectBackgroundWidth;

	private float flipTimer;

	public float RotWobbleAmp = 1f;

	public float RotWobbleSpeed = 1f;

	public float RotWobbleSpringiness = 1f;

	private float wobbleRotVelo;

	public bool AutoRotWobble;

	public float AutoRotWobbleTimer;

	public float AutoRotWobbleAmount = 0.1f;

	private float timer;

	private float rotWobbleTimer;

	private float curZ = 270f;

	public bool TimerRunning;

	public string Status;

	public float CurrentTimerTime;

	public float TargetTimerTime;

	public TimerAction TimerAction;

	public string TimerBlueprintId;

	public int TimerSubprintIndex;

	public bool SkipCitiesChecks;

	public string TimerActionId;

	public Statusbar CurrentStatusbar;

	[HideInInspector]
	public GameCard removedChild;

	public Transform StatusEffectElementParent;

	private float curHeight;

	[HideInInspector]
	public bool IsHit;

	private enum PositionType
	{
		InConflict,
		InAttack,
		InStack,
		IsRoot,
		IsEquipped,
		InAnimation,
		None,
		IsWorking
	}
}
