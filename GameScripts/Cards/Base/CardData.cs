using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CardData : MonoBehaviour, IGameCardOrCardData
{
	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(this.nameOverride))
			{
				return this.nameOverride;
			}
			if (this._oldNameTerm == this.NameTerm && !string.IsNullOrEmpty(this._name))
			{
				return this._name;
			}
			string text = SokLoc.Translate(this.NameTerm);
			if (this.MyCardType == CardType.Ideas)
			{
				Blueprint blueprint = this as Blueprint;
				if (blueprint != null)
				{
					if (blueprint.IsInvention)
					{
						text = SokLoc.Translate("label_invention_fullname", new LocParam[] { LocParam.Create("name", text) });
					}
					else if (blueprint.IsLandmark)
					{
						text = SokLoc.Translate("label_landmark_fullname", new LocParam[] { LocParam.Create("name", text) });
					}
					else
					{
						text = SokLoc.Translate("label_idea_fullname", new LocParam[] { LocParam.Create("name", text) });
					}
				}
				else
				{
					text = SokLoc.Translate("label_idea_fullname", new LocParam[] { LocParam.Create("name", text) });
				}
			}
			if (this.MyCardType == CardType.Rumors)
			{
				text = SokLoc.Translate("label_rumor_full", new LocParam[] { LocParam.Create("name", text) });
			}
			this._oldNameTerm = this.NameTerm;
			this._name = text;
			return text;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.MyGameCard.transform.position;
		}
	}

	public string FullName
	{
		get
		{
			string name = this.Name;
			if (this.IsFoil)
			{
				return name + " " + SokLoc.Translate("label_foil");
			}
			return name;
		}
	}

	public string Description
	{
		get
		{
			if (!string.IsNullOrEmpty(this.descriptionOverride))
			{
				return this.descriptionOverride;
			}
			return SokLoc.Translate(this.DescriptionTerm);
		}
	}

	public virtual bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return false;
		}
	}

	public virtual bool HasInventory
	{
		get
		{
			return false;
		}
	}

	// Khi true, các thẻ con sẽ được dàn theo hàng ngang thay vì xếp chồng dọc.
	// Sử dụng bởi Đền Thờ (Shrine) và Đột Phá Trận (Breakthrough Array).
	public virtual bool UsesHorizontalSlots
	{
		get
		{
			return false;
		}
	}

	// Xác định thẻ này có phải là Cổ Vật cổ xưa hỗ trợ tự động hóa hay không.
	public virtual bool IsAncientRelic
	{
		get
		{
			return this.Id != null && (this.Id.ToLower().Contains("relic") || this.Id.ToLower().StartsWith("item_ancient_relic"));
		}
	}

	// Xác định thẻ này có phải là Cống Phẩm hiến tế cho Đền Thờ hay không.
	public virtual bool IsShrineOffering
	{
		get
		{
			return this.Id != null && this.Id.ToLower() == "item_shrine_offering";
		}
	}

	// Xác định thẻ này có thể dùng làm vật phẩm hỗ trợ trong Đột Phá Trận hay không.
	public virtual bool IsBreakthroughSupport
	{
		get
		{
			if (this.Id == null) return false;
			string lowerId = this.Id.ToLower();
			return lowerId == "item_breakthrough_pill" || lowerId == "item_revive_pill" || 
			       lowerId.Contains("talisman") || lowerId.Contains("potion") || lowerId.Contains("shield");
		}
	}

	// Tỷ lệ giảm sát thương lôi kiếp mặc định khi làm vật phẩm hỗ trợ đột phá (0f - 1f).
	public virtual float BreakthroughDmgReduction
	{
		get
		{
			if (this.Id == null) return 0f;
			string lowerId = this.Id.ToLower();
			if (lowerId == "item_breakthrough_pill") return 0.50f;
			if (lowerId.Contains("talisman") || lowerId.Contains("shield")) return 0.30f;
			return 0.05f; // Đồ vật linh tinh thường giảm nhẹ 5%
		}
	}

	// Lượng sinh mệnh tăng thêm tạm thời mặc định khi làm vật phẩm hỗ trợ đột phá.
	public virtual int BreakthroughHealthBonus
	{
		get
		{
			if (this.Id == null) return 0;
			string lowerId = this.Id.ToLower();
			if (lowerId == "item_breakthrough_pill") return 15;
			if (lowerId.Contains("potion")) return 20;
			return 0;
		}
	}

	// Hiệu ứng bảo mệnh hồi sinh mặc định khi làm vật phẩm hỗ trợ đột phá.
	public virtual bool BreakthroughReviveEffect
	{
		get
		{
			return this.Id != null && this.Id.ToLower() == "item_revive_pill";
		}
	}

	public List<CardBag> GetCardBags()
	{
		Type type = base.GetType();
		List<CardBag> list = new List<CardBag>();
		foreach (FieldInfo fieldInfo in from x in type.GetFields()
			where x.FieldType == typeof(CardBag) || x.FieldType.IsSubclassOf(typeof(CardBag))
			select x)
		{
			CardBag cardBag = (CardBag)fieldInfo.GetValue(this);
			list.Add(cardBag);
		}
		foreach (FieldInfo fieldInfo2 in type.GetFields().Where<FieldInfo>(delegate(FieldInfo x)
		{
			Type fieldType = x.FieldType;
			if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type type2 = fieldType.GetGenericArguments()[0];
				if (type2 == typeof(CardBag) || type2.IsSubclassOf(typeof(CardBag)))
				{
					return true;
				}
			}
			return false;
		}))
		{
			List<object> list2 = ((IEnumerable)fieldInfo2.GetValue(this)).Cast<object>().ToList<object>();
			list.AddRange(list2.Cast<CardBag>());
		}
		return list;
	}

	public int GetValue()
	{
		if (this.MonthlyRequirementResult != null)
		{
			return -1;
		}
		if (this.MyGameCard != null && this.MyGameCard.IsDemoCard)
		{
			if (this.CardUpdateType != CardUpdateType.Cities)
			{
				return this.Value;
			}
			return this.CitiesValue;
		}
		else
		{
			WorldManager instance = WorldManager.instance;
			bool flag;
			if (instance == null)
			{
				flag = false;
			}
			else
			{
				GameBoard currentBoard = instance.CurrentBoard;
				Location? location = ((currentBoard != null) ? new Location?(currentBoard.Location) : null);
				Location location2 = Location.Cities;
				flag = (location.GetValueOrDefault() == location2) & (location != null);
			}
			if (!flag)
			{
				return this.Value;
			}
			return this.CitiesValue;
		}
	}

	public virtual bool ShouldCompleteTimer(string timerActionId)
	{
		return true;
	}

	public virtual bool ShouldStartTimerWorkers(string timerActionId)
	{
		if (this.WorkerAmount <= 0)
		{
			return true;
		}
		bool flag = this.WorkerAmountMet();
		if (this.EducatedWorkers && flag)
		{
			if (this.MyGameCard.WorkerChildren.Any<GameCard>((GameCard c) => c.CardData.Id != "educated_worker" && c.CardData.Id != "genius" && c.CardData.Id != "robot_genius" && c.CardData.Id != "robot_worker"))
			{
				return false;
			}
		}
		return flag;
	}

	public virtual bool ShouldStartTimerEnergy(CardData consumer, string timerActionId)
	{
		return consumer == null || (consumer != null && consumer.HasEnergyInput(null));
	}

	public virtual void OnLanguageChange()
	{
		this._name = null;
		this._oldNameTerm = null;
		if (WorldManager.instance.GameDataLoader.ProfanityChecker.IsProfanityInLanguage(SokLoc.instance.CurrentLanguage, this.CustomName))
		{
			this.CustomName = "Bobba";
		}
		this._cachedConnectorString = null;
	}

	protected virtual void OnValidate()
	{
		foreach (CardBag cardBag in this.GetCardBags())
		{
			cardBag.RecalculateOdds();
		}
	}

	public virtual bool HasEnergyOutput(CardConnector connectedNode = null, List<CardConnector> nodeTracker = null)
	{
		return false;
	}

	private bool HasEnergyInputConnector()
	{
		for (int i = 0; i < this.MyGameCard.CardConnectorChildren.Count; i++)
		{
			if (this.MyGameCard.CardConnectorChildren[i].CardDirection == CardDirection.input && this.MyGameCard.CardConnectorChildren[i].IsEnergyConnector)
			{
				return true;
			}
		}
		return false;
	}

	private bool AllInputConnectorsPowered()
	{
		for (int i = 0; i < this.MyGameCard.CardConnectorChildren.Count; i++)
		{
			if (this.MyGameCard.CardConnectorChildren[i].CardDirection == CardDirection.input && this.MyGameCard.CardConnectorChildren[i].IsEnergyConnector)
			{
				if (this.MyGameCard.CardConnectorChildren[i].ConnectedNode == null)
				{
					return false;
				}
				if (!this.MyGameCard.CardConnectorChildren[i].ConnectedNode.HasEnergyOutput())
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool HasAnySewerOutput()
	{
		foreach (CardConnectorData cardConnectorData in this.EnergyConnectors)
		{
			if (cardConnectorData.EnergyConnectionStrength == ConnectionType.Sewer && cardConnectorData.EnergyConnectionType == CardDirection.output)
			{
				return true;
			}
		}
		return false;
	}

	private bool AllSewerOutputsFunctional()
	{
		foreach (CardConnector cardConnector in this.MyGameCard.CardConnectorChildren)
		{
			if (cardConnector.ConnectionType == ConnectionType.Sewer && cardConnector.CardDirection == CardDirection.output && (cardConnector.ConnectedNode == null || cardConnector.ConnectedNode.Parent.CardData.IsDamaged))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasSewerConnected()
	{
		return !this.HasAnySewerOutput() || this.AllSewerOutputsFunctional();
	}

	public virtual bool HasEnergyInput(CardConnector connectedNode = null)
	{
		return !this.HasEnergyInputConnector() || this.AllInputConnectorsPowered();
	}

	public void NotifyEnergyConsumers()
	{
		foreach (CardConnector cardConnector in this.MyGameCard.CardConnectorChildren)
		{
			if (cardConnector.ConnectedNode != null)
			{
				cardConnector.ConnectedNode.Parent.StackUpdate = true;
			}
		}
		this.MyGameCard.StackUpdate = true;
	}

	public string GetEnergyInputString()
	{
		return "todo: Implement this to show input amount";
	}

	public int GetRepairCost()
	{
		if (this is Creditcard)
		{
			return 20;
		}
		return Mathf.Max(10, Mathf.FloorToInt((float)(this.CitiesValue / 10 / 2)) * 10);
	}

	public virtual void OnSellCard()
	{
	}

	public virtual void OnInitialCreate()
	{
	}

	public virtual bool CanBeDragged
	{
		get
		{
			return true;
		}
	}

	public bool CanHaveCardOnTop(CardData otherCard, bool isPrefab = false)
	{
		GameCard rootCard = this.MyGameCard.GetRootCard();
		int num = rootCard.GetChildCount() + 1;
		if (this.MyGameCard.CardData is Chest)
		{
			num = 0;
		}
		int num2;
		if (isPrefab)
		{
			num2 = 1;
		}
		else
		{
			num2 = otherCard.MyGameCard.GetRootCard().GetChildCount() + 1;
		}
		if (num + num2 > 30)
		{
			return false;
		}
		if (this.IsDamaged || rootCard.CardData.IsDamaged)
		{
			if (this.DamageType == CardDamageType.Fire || rootCard.CardData.DamageType == CardDamageType.Fire || this.DamageType == CardDamageType.Drought || rootCard.CardData.DamageType == CardDamageType.Drought)
			{
				return otherCard.Id == "water";
			}
			if (this.DamageType == CardDamageType.Damaged || rootCard.CardData.DamageType == CardDamageType.Damaged)
			{
				return otherCard is ICurrency;
			}
		}
		if (rootCard.CardData is HeavyFoundation && rootCard.HasChild && rootCard.Child.CardData.DetermineCanHaveCardsWhenIsRoot)
		{
			return rootCard.Child.CardData.CanHaveCard(otherCard);
		}
		if (rootCard.CardData.WorkerAmount > 0)
		{
			if (rootCard.CardData.EducatedWorkers && (otherCard.Id == "genius" || otherCard.Id == "robot_genius" || otherCard.Id == "educated_worker" || otherCard.Id == "robot_worker"))
			{
				return true;
			}
			if (otherCard is Worker)
			{
				return true;
			}
		}
		if (rootCard.CardData.DetermineCanHaveCardsWhenIsRoot)
		{
			return rootCard.CardData.CanHaveCard(otherCard);
		}
		if (this.MyGameCard.IsEquipped || this.MyGameCard.IsWorking)
		{
			return false;
		}
		if (this.MyGameCard.InConflict)
		{
			bool flag = false;
			if (otherCard.Id == "bone" && this.Id == "wolf")
			{
				flag = true;
			}
			else if (otherCard.Id == "milk" && this.Id == "feral_cat")
			{
				flag = true;
			}
			else if (otherCard.Id == "parrot" && this.Id == "pirate")
			{
				flag = true;
			}
			if (!(otherCard is Equipable) && !(otherCard is Combatable) && !flag)
			{
				return false;
			}
		}
		return this.CanHaveCard(otherCard);
	}

	protected virtual bool CanHaveCard(CardData otherCard)
	{
		return false;
	}

	protected virtual bool CanSelectOutput()
	{
		return false;
	}

	public bool CanSelectOutputDirection()
	{
		return this.CanSelectOutput();
	}

	public bool HasOutputConnector()
	{
		return this.MyGameCard.CardConnectorChildren.Any<CardConnector>((CardConnector x) => x.ConnectionType == ConnectionType.Transport && x.CardDirection == CardDirection.output);
	}

	protected virtual bool CanToggleOnOff()
	{
		return false;
	}

	public bool CanToggleCardOnOff()
	{
		return !this.MyGameCard.IsDemoCard && this.CanToggleOnOff();
	}

	public void ToggleCardOnOff()
	{
		this.IsOn = !this.IsOn;
	}

	public virtual void SetFoil()
	{
		this.IsFoil = true;
		if (this.Value != -1)
		{
			this.Value *= 5;
		}
		if (this.CitiesValue != -1)
		{
			this.CitiesValue *= 5;
		}
	}

	public virtual void Clicked()
	{
	}

	public bool HasCardOnTop<T>() where T : CardData
	{
		T t;
		return this.HasCardOnTop<T>(out t);
	}

	public bool HasCardOnTop(string id)
	{
		return this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == id;
	}

	public virtual bool CanBePushedBy(CardData otherCard)
	{
		return true;
	}

	public bool HasCardOnTop(string id, out CardData cardData)
	{
		cardData = null;
		if (!this.MyGameCard.HasChild)
		{
			return false;
		}
		if (this.MyGameCard.Child.CardData.Id == id)
		{
			cardData = this.MyGameCard.Child.CardData;
			return true;
		}
		return false;
	}

	public bool HasCardOnTop<T>(out T card) where T : CardData
	{
		card = default(T);
		if (!this.MyGameCard.HasChild)
		{
			return false;
		}
		card = this.MyGameCard.Child.CardData as T;
		return card != null;
	}

	public bool IsOnCard<T>(out T card) where T : CardData
	{
		card = default(T);
		if (!this.MyGameCard.HasParent)
		{
			return false;
		}
		card = this.MyGameCard.Parent.CardData as T;
		return card != null;
	}

	protected virtual string GetTooltipText()
	{
		return string.Format("{0} (${1})\n<size=70%><i>{2}</i></size>", this.Name, this.GetValue(), this.Description);
	}

	public virtual bool CanHaveCardsWhileHasStatus()
	{
		return false;
	}

	protected virtual void Awake()
	{
	}

	public virtual void UpdateCardText()
	{
		GameCard myGameCard = this.MyGameCard;
		if (myGameCard != null && myGameCard.CardConnectorChildren.Count > 0 && this.MyGameCard.IsHovered)
		{
			this.descriptionOverride = SokLoc.Translate(this.DescriptionTerm);
			this.descriptionOverride = this.descriptionOverride + "\n\n<i>" + this.GetConnectorInfoString(this.MyGameCard) + "</i>";
		}
	}

	private int GetConnectorCount(GameCard card, CardDirection connectionType, ConnectionType strength)
	{
		int num = 0;
		for (int i = 0; i < card.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = card.CardConnectorChildren[i];
			if (cardConnector.CardDirection == connectionType && cardConnector.ConnectionType == strength)
			{
				num++;
			}
		}
		return num;
	}

	public string GetConnectorInfoString(GameCard card)
	{
		if (this._cachedConnectorString != null)
		{
			return this._cachedConnectorString;
		}
		string text = "";
		if (card != null && card.CardConnectorChildren.Count > 0)
		{
			int connectorCount = this.GetConnectorCount(card, CardDirection.input, ConnectionType.LV);
			int connectorCount2 = this.GetConnectorCount(card, CardDirection.input, ConnectionType.HV);
			if (connectorCount > 0 || connectorCount2 > 0)
			{
				text += SokLoc.Translate("label_energy_input");
				if (connectorCount > 0)
				{
					text += string.Format(" {0}{1}", connectorCount, Icons.LV);
				}
				if (connectorCount2 > 0)
				{
					text += string.Format(" {0}{1}", connectorCount2, Icons.HV);
				}
			}
			int connectorCount3 = this.GetConnectorCount(card, CardDirection.output, ConnectionType.LV);
			int connectorCount4 = this.GetConnectorCount(card, CardDirection.output, ConnectionType.HV);
			if (connectorCount3 > 0 || connectorCount4 > 0)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n\n";
				}
				text += SokLoc.Translate("label_energy_output");
				if (connectorCount3 > 0)
				{
					text += string.Format(" {0}{1}", connectorCount3, Icons.LV);
				}
				if (connectorCount4 > 0)
				{
					text += string.Format(" {0}{1}", connectorCount4, Icons.HV);
				}
			}
		}
		this._cachedConnectorString = text;
		return text;
	}

	public virtual void UpdateCard()
	{
		if (this.MyGameCard.IsDemoCard || !this.MyGameCard.MyBoard.IsCurrent)
		{
			return;
		}
		this.MyGameCard.HighlightActive = false;
		if (WorldManager.instance.DraggingCard != null && WorldManager.instance.DraggingCard != this.MyGameCard)
		{
			if (this.CanHaveCardOnTop(WorldManager.instance.DraggingCard.CardData, false) && !this.MyGameCard.HasChild && !this.MyGameCard.IsChildOf(WorldManager.instance.DraggingCard))
			{
				this.MyGameCard.HighlightActive = true;
			}
			if (!(this.MyGameCard.removedChild == WorldManager.instance.DraggingCard))
			{
				GameCard cardWithStatusInStack = this.MyGameCard.GetCardWithStatusInStack();
				if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
				{
					this.MyGameCard.HighlightActive = false;
				}
			}
		}
		if (this.MyGameCard.StackUpdate)
		{
			if (this.HasStatusEffectOfType<StatusEffect_MaxOnBoard>())
			{
				this.RemoveStatusEffect<StatusEffect_MaxOnBoard>();
			}
			if (!this.WorkerAmountMet() && this.MyGameCard.TimerRunning && !this.MyGameCard.SkipCitiesChecks)
			{
				this.MyGameCard.CancelAnyTimer();
			}
			this.CheckBlueprintInStack();
		}
		if (!this.MyGameCard.BeingDragged && this.MyGameCard.LastParent != null && !this.MyGameCard.HasParent)
		{
			if (this.MyGameCard.LastParent.GetRootCard().CardData.DetermineCanHaveCardsWhenIsRoot)
			{
				this.CheckStackValidityAndRestack();
			}
			this.MyGameCard.LastParent = null;
		}
		if (this.WorkerAmount > 0)
		{
			bool flag = this.WorkerAmountMet();
			if (this.EducatedWorkers)
			{
				if (this.MyGameCard.WorkerChildren.Any<GameCard>(delegate(GameCard c)
				{
					Worker worker = c.CardData as Worker;
					return worker != null && worker.GetWorkerType() != WorkerType.Educated && worker.GetWorkerType() != WorkerType.Robot;
				}) || !flag)
				{
					if (!this.HasStatusEffectOfType<StatusEffect_NoEducatedWorkers>())
					{
						this.AddStatusEffect(new StatusEffect_NoEducatedWorkers());
					}
				}
				else
				{
					this.RemoveStatusEffect<StatusEffect_NoEducatedWorkers>();
				}
			}
			else if (!flag)
			{
				if (!this.HasStatusEffectOfType<StatusEffect_NoWorkers>())
				{
					this.AddStatusEffect(new StatusEffect_NoWorkers());
				}
			}
			else
			{
				this.RemoveStatusEffect<StatusEffect_NoWorkers>();
			}
			for (int i = 0; i < this.MyGameCard.WorkerChildren.Count; i++)
			{
				GameCard gameCard = this.MyGameCard.WorkerChildren[i];
				if (gameCard.HasParent || gameCard.HasChild)
				{
					gameCard.RemoveFromStack();
				}
			}
		}
		else
		{
			if (this.HasStatusEffectOfType<StatusEffect_NoWorkers>())
			{
				this.RemoveStatusEffect<StatusEffect_NoWorkers>();
			}
			if (this.HasStatusEffectOfType<StatusEffect_NoEducatedWorkers>())
			{
				this.RemoveStatusEffect<StatusEffect_NoEducatedWorkers>();
			}
		}
		if (!this.IsOn && ((this.WorkerAmount > 0 && this.WorkerAmountMet()) || this.WorkerAmount == 0))
		{
			this.AddStatusEffect(new StatusEffect_CardOff());
		}
		else
		{
			this.RemoveStatusEffect<StatusEffect_CardOff>();
		}
		for (int j = this.StatusEffects.Count - 1; j >= 0; j--)
		{
			this.StatusEffects[j].Update();
		}
		this.UpdateCardText();
	}

	private void CheckBlueprintInStack()
	{
		if (!this.MyGameCard.HasParent)
		{
			Subprint subprint = this.FindMatchingPrint();
			if (subprint != null)
			{
				string id = subprint.ParentBlueprint.Id;
				int subprintIndex = subprint.SubprintIndex;
				BaseVillager baseVillager = (from BaseVillager x in this.CardsInStackMatchingPredicate((CardData x) => x is BaseVillager)
					orderby x.GetActionTimeModifier("finish_blueprint", this)
					select x).Reverse<BaseVillager>().FirstOrDefault<BaseVillager>();
				Worker worker = (from Worker x in this.CardsInStackMatchingPredicate((CardData x) => x is Worker)
					orderby x.GetActionTimeModifier()
					select x).Reverse<Worker>().FirstOrDefault<Worker>();
				if (!subprint.ParentBlueprint.IsInvention || (subprint.ParentBlueprint.IsInvention && WorldManager.instance.HasFoundCard(id)))
				{
					CardData cardData = this.CardsInStackMatchingPredicate((CardData x) => x is IEnergyConsumer).FirstOrDefault<CardData>();
					if (baseVillager != null)
					{
						this.MyGameCard.StartBlueprintTimer(baseVillager.GetActionTimeModifier("finish_blueprint", this) * subprint.Time, new TimerAction(this.FinishBlueprint), subprint.StatusName, this.GetActionId("FinishBlueprint"), id, subprintIndex, cardData, subprint.ParentBlueprint.IgnoreEnergyWorkerDemand);
					}
					else if (worker != null)
					{
						this.MyGameCard.StartBlueprintTimer(worker.GetActionTimeModifier() * subprint.Time, new TimerAction(this.FinishBlueprint), subprint.StatusName, this.GetActionId("FinishBlueprint"), id, subprintIndex, cardData, subprint.ParentBlueprint.IgnoreEnergyWorkerDemand);
					}
					else
					{
						this.MyGameCard.StartBlueprintTimer(subprint.Time, new TimerAction(this.FinishBlueprint), subprint.StatusName, this.GetActionId("FinishBlueprint"), id, subprintIndex, cardData, subprint.ParentBlueprint.IgnoreEnergyWorkerDemand);
					}
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(this.GetActionId("FinishBlueprint"));
			}
		}
		else
		{
			this.MyGameCard.CancelTimer(this.GetActionId("FinishBlueprint"));
		}
		if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "finish_blueprint" && this.FindMatchingPrint() == null)
		{
			this.MyGameCard.CancelTimer(this.GetActionId("FinishBlueprint"));
		}
		this.MyGameCard.StackUpdate = false;
	}

	private void CheckStackValidityAndRestack()
	{
		List<GameCard> allCardsInStack = this.MyGameCard.GetAllCardsInStack();
		List<GameCard> list = new List<GameCard>();
		for (int i = 0; i < allCardsInStack.Count; i++)
		{
			list.Add(allCardsInStack[i]);
			allCardsInStack[i].RemoveFromStack();
			if (i < allCardsInStack.Count - 1 && !allCardsInStack[i].CardData.CanHaveCardOnTop(allCardsInStack[i + 1].CardData, false))
			{
				WorldManager.instance.Restack(list);
				list.Clear();
			}
		}
		WorldManager.instance.Restack(list);
	}

	public virtual void ParseAction(string actions)
	{
		foreach (string text in actions.Split(';', StringSplitOptions.None))
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			if (text.StartsWith("add"))
			{
				string text2 = text.Split(':', StringSplitOptions.None)[1];
				StatusEffect statusEffectWithName = this.GetStatusEffectWithName(text2);
				if (this.HasStatusEffectOfType(statusEffectWithName))
				{
					this.RemoveStatusEffect(statusEffectWithName.GetType());
				}
				this.AddStatusEffect(statusEffectWithName);
			}
			if (text.StartsWith("remove"))
			{
				string text3 = text.Split(':', StringSplitOptions.None)[1];
				this.RemoveStatusEffect(text3);
			}
			if (text.StartsWith("create"))
			{
				string text4 = text.Split(':', StringSplitOptions.None)[1];
				WorldManager.instance.CreateCard(base.transform.position, text4, false, false, true);
			}
			if (text.StartsWith("special"))
			{
				string text5 = text.Split(':', StringSplitOptions.None)[1];
				QuestManager.instance.SpecialActionComplete(text5, null);
			}
		}
	}

	private Subprint FindMatchingPrint()
	{
		Subprint subprint = null;
		int num = int.MaxValue;
		int num2 = int.MinValue;
		foreach (Blueprint blueprint in WorldManager.instance.BlueprintPrefabs)
		{
			if (blueprint.CanCurrentlyBeMade && (WorldManager.instance.CurrentBoard.Location != Location.Cities || blueprint.CardUpdateType == CardUpdateType.Cities))
			{
				SubprintMatchInfo subprintMatchInfo;
				Subprint matchingSubprint = blueprint.GetMatchingSubprint(this.MyGameCard.GetRootCard(), out subprintMatchInfo);
				if (matchingSubprint != null)
				{
					if (blueprint.HasMaxAmountOnBoard && WorldManager.instance.GetCardCount(matchingSubprint.ResultCard) >= blueprint.MaxAmountOnBoard)
					{
						if (!this.HasStatusEffectOfType<StatusEffect_MaxOnBoard>())
						{
							this.AddStatusEffect(new StatusEffect_MaxOnBoard());
						}
					}
					else if (subprintMatchInfo.MatchCount > num2 || (subprintMatchInfo.MatchCount == num2 && subprintMatchInfo.FullyMatchedAt < num))
					{
						num = subprintMatchInfo.FullyMatchedAt;
						num2 = subprintMatchInfo.MatchCount;
						subprint = matchingSubprint;
					}
				}
			}
		}
		return subprint;
	}

	public void EquipItem(Equipable equipable)
	{
		if (equipable.EquipableType == EquipableType.Talisman)
		{
			int maxTalismans = 2;
			if (this is CatCardData cat)
			{
				if (cat.BreakthroughLevel >= 4) maxTalismans = 2;
				else if (cat.BreakthroughLevel == 3) maxTalismans = 1;
				else maxTalismans = 0;
			}

			List<Equipable> existingTalismans = this.GetAllEquipables().FindAll(x => x != null && x.EquipableType == EquipableType.Talisman);
			if (maxTalismans <= 0)
			{
				equipable.MyGameCard.SendIt();
				return;
			}

			if (existingTalismans.Count >= maxTalismans)
			{
				Equipable oldest = existingTalismans[0];
				this.MyGameCard.Unequip(oldest);
				oldest.MyGameCard.SendIt();
			}
			this.MyGameCard.Equip(equipable);
		}
		else if (equipable.EquipableType == EquipableType.Weapon && this is CatCardData catWeap && catWeap.HasTrait(Mewtations.Expedition.HeavenlyTalent.DualWield))
		{
			List<Equipable> existingWeapons = this.GetAllEquipables().FindAll(x => x != null && x.EquipableType == EquipableType.Weapon);
			if (existingWeapons.Count >= 2)
			{
				Equipable oldest = existingWeapons[0];
				this.MyGameCard.Unequip(oldest);
				oldest.MyGameCard.SendIt();
			}
			this.MyGameCard.Equip(equipable);
		}
		else if (equipable.EquipableType == EquipableType.Food && this is CatCardData catFood && catFood.HasTrait(Mewtations.Expedition.HeavenlyTalent.FoodGlutton))
		{
			List<Equipable> existingFoods = this.GetAllEquipables().FindAll(x => x != null && x.EquipableType == EquipableType.Food);
			if (existingFoods.Count >= 2)
			{
				Equipable oldest = existingFoods[0];
				this.MyGameCard.Unequip(oldest);
				oldest.MyGameCard.SendIt();
			}
			this.MyGameCard.Equip(equipable);
		}
		else
		{
			Equipable equipableOfEquipableType = this.GetEquipableOfEquipableType(equipable.EquipableType);
			if (equipableOfEquipableType != null)
			{
				this.MyGameCard.Unequip(equipableOfEquipableType);
				equipableOfEquipableType.MyGameCard.SendIt();
			}
			this.MyGameCard.Equip(equipable);
		}

		if (!(this is Mob))
		{
			QuestManager.instance.ActionComplete(this.MyGameCard.CardData, "equip_item", null);
		}
	}

	public void EquipWorker(Worker worker)
	{
		List<int> list = this.MyGameCard.WorkerChildren.Select<GameCard, int>((GameCard x) => x.CardData.WorkerIndex).ToList<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < this.WorkerAmount; i++)
		{
			list2.Add(i);
		}
		int index = list2.Except<int>(list).DefaultIfEmpty(this.WorkerAmount - 1).First<int>();
		if (this.MyGameCard.WorkerChildren.Count > index)
		{
			GameCard gameCard = this.MyGameCard.WorkerChildren.Where<GameCard>((GameCard x) => x.CardData.WorkerIndex == index).FirstOrDefault<GameCard>();
			if (gameCard != null)
			{
				this.MyGameCard.UnequipWorker(gameCard);
				gameCard.SendIt();
			}
		}
		this.MyGameCard.EquipWorker(worker, index);
	}

	public virtual void StoppedDragging()
	{
	}

	public virtual void OnEquipItem(Equipable equipable)
	{
	}

	public virtual void OnUnequipItem(Equipable equipable)
	{
	}

	public virtual void OnDestroyCard()
	{
	}

	[TimedAction("finish_blueprint")]
	public void FinishBlueprint()
	{
		Blueprint blueprintWithId = WorldManager.instance.GetBlueprintWithId(this.MyGameCard.TimerBlueprintId);
		if (blueprintWithId != null)
		{
			blueprintWithId.BlueprintComplete(this.MyGameCard, this.MyGameCard.GetAllCardsInStack(), blueprintWithId.Subprints[this.MyGameCard.TimerSubprintIndex]);
		}
	}

	public List<ExtraCardData> GetExtraCardData()
	{
		return CardData.GetExtraCardData(this);
	}

	public static List<ExtraCardData> GetExtraCardData(object o)
	{
		List<ExtraCardData> list = new List<ExtraCardData>();
		foreach (FieldInfo fieldInfo in o.GetType().GetFields())
		{
			foreach (ExtraDataAttribute extraDataAttribute in (ExtraDataAttribute[])fieldInfo.GetCustomAttributes(typeof(ExtraDataAttribute), true))
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, (string)fieldInfo.GetValue(o)));
				}
				else if (fieldInfo.FieldType == typeof(int))
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, (int)fieldInfo.GetValue(o)));
				}
				else if (fieldInfo.FieldType.IsEnum)
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, Convert.ToInt32(fieldInfo.GetValue(o))));
				}
				else if (fieldInfo.FieldType == typeof(float))
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, (float)fieldInfo.GetValue(o)));
				}
				else if (fieldInfo.FieldType == typeof(Vector3))
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, (Vector3)fieldInfo.GetValue(o)));
				}
				else if (fieldInfo.FieldType == typeof(bool))
				{
					list.Add(new ExtraCardData(extraDataAttribute.Identifier, (bool)fieldInfo.GetValue(o)));
				}
				else
				{
					Debug.LogError("Can't serialize field " + fieldInfo.Name + " with ExtraDataAttribute because it's not an int or a string!");
				}
			}
		}
		CardData cardData = o as CardData;
		if (cardData != null)
		{
			list.AddRange(cardData.LeftoverExtraData);
		}
		return list;
	}

	public void SetExtraCardData(List<ExtraCardData> extraData)
	{
		CardData.SetExtraCardData(this, extraData);
	}

	public static void SetExtraCardData(object o, List<ExtraCardData> extraData)
	{
		foreach (FieldInfo fieldInfo in o.GetType().GetFields())
		{
			ExtraDataAttribute[] array = (ExtraDataAttribute[])fieldInfo.GetCustomAttributes(typeof(ExtraDataAttribute), true);
			for (int j = 0; j < array.Length; j++)
			{
				ExtraDataAttribute attribute = array[j];
				ExtraCardData extraCardData = extraData.FirstOrDefault<ExtraCardData>((ExtraCardData x) => x.AttributeId == attribute.Identifier);
				if (extraCardData != null)
				{
					if (fieldInfo.FieldType == typeof(string))
					{
						fieldInfo.SetValue(o, extraCardData.StringValue);
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						fieldInfo.SetValue(o, extraCardData.IntValue);
					}
					else if (fieldInfo.FieldType.IsEnum)
					{
						fieldInfo.SetValue(o, Enum.ToObject(fieldInfo.FieldType, extraCardData.IntValue));
					}
					else if (fieldInfo.FieldType == typeof(float))
					{
						fieldInfo.SetValue(o, extraCardData.FloatValue);
					}
					else if (fieldInfo.FieldType == typeof(Vector3))
					{
						fieldInfo.SetValue(o, extraCardData.VectorValue);
					}
					else if (fieldInfo.FieldType == typeof(bool))
					{
						fieldInfo.SetValue(o, extraCardData.BoolValue);
					}
					else
					{
						Debug.LogError("Can't deserialize field " + fieldInfo.Name + " with ExtraDataAttribute because it's not an int or a string!");
					}
					extraData.Remove(extraCardData);
				}
				else
				{
					Debug.LogWarning(string.Format("Could not find matching data for {0} in {1}, using default value..", fieldInfo.Name, o.GetType()));
				}
			}
		}
		CardData cardData = o as CardData;
		if (cardData != null)
		{
			cardData.LeftoverExtraData = extraData;
		}
	}

	public string GetActionId(string methodName)
	{
		if (!this.methodToActionId.ContainsKey(methodName))
		{
			TimedActionAttribute[] array = (TimedActionAttribute[])base.GetType().GetMethod(methodName).GetCustomAttributes(typeof(TimedActionAttribute), true);
			this.methodToActionId[methodName] = array[0].Identifier;
		}
		return this.methodToActionId[methodName];
	}

	public TimerAction GetDelegateForActionId(string id)
	{
		foreach (MethodInfo methodInfo in base.GetType().GetMethods())
		{
			TimedActionAttribute[] array = (TimedActionAttribute[])methodInfo.GetCustomAttributes(typeof(TimedActionAttribute), true);
			if (array.Length != 0 && array[0].Identifier == id)
			{
				return (TimerAction)methodInfo.CreateDelegate(typeof(TimerAction), this);
			}
		}
		Debug.LogError("Could not find delegate for id " + id);
		return null;
	}

	public bool AllChildrenMatchPredicate(Predicate<CardData> pred)
	{
		if (this.MyGameCard == null)
		{
			return false;
		}
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (!pred(gameCard.CardData))
			{
				return false;
			}
			gameCard = gameCard.Child;
		}
		return true;
	}

	public bool AnyChildMatchesPredicate(Predicate<CardData> pred)
	{
		CardData cardData;
		return this.AnyChildMatchesPredicate(pred, out cardData);
	}

	public bool AnyChildMatchesPredicate(Predicate<CardData> pred, out CardData match)
	{
		match = null;
		if (this.MyGameCard == null)
		{
			return false;
		}
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				match = gameCard.CardData;
				return true;
			}
			gameCard = gameCard.Child;
		}
		return false;
	}

	protected int ChildrenMatchingPredicateCount(Predicate<CardData> pred)
	{
		int num = 0;
		if (this.MyGameCard == null)
		{
			return num;
		}
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				num++;
			}
			gameCard = gameCard.Child;
		}
		return num;
	}

	public List<CardData> CardsInStackMatchingPredicate(Predicate<CardData> pred)
	{
		List<CardData> list = new List<CardData>();
		this.GetCardsInStackMatchingPredicate(pred, list);
		return list;
	}

	public void GetCardsInStackMatchingPredicate(Predicate<CardData> pred, List<CardData> outList)
	{
		outList.Clear();
		if (this.MyGameCard == null)
		{
			if (pred(this))
			{
				outList.Add(this);
			}
			return;
		}
		GameCard gameCard = this.MyGameCard.GetRootCard();
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				outList.Add(gameCard.CardData);
			}
			gameCard = gameCard.Child;
		}
	}

	public List<CardData> ChildrenMatchingPredicate(Predicate<CardData> pred)
	{
		List<CardData> list = new List<CardData>();
		if (this.MyGameCard == null)
		{
			return list;
		}
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				list.Add(gameCard.CardData);
			}
			gameCard = gameCard.Child;
		}
		return list;
	}

	public void GetChildrenMatchingPredicate(Predicate<CardData> pred, List<CardData> result)
	{
		result.Clear();
		if (this.MyGameCard == null)
		{
			return;
		}
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				result.Add(gameCard.CardData);
			}
			gameCard = gameCard.Child;
		}
	}

	protected void RemoveFirstChildFromStack()
	{
		if (this.MyGameCard == null)
		{
			return;
		}
		GameCard child = this.MyGameCard.Child;
		GameCard child2 = child.Child;
		child.RemoveFromStack();
		if (child2 != null)
		{
			child2.SetParent(this.MyGameCard);
		}
	}

	public void RestackChildrenMatchingPredicate(Predicate<CardData> pred)
	{
		List<GameCard> list = new List<GameCard>();
		list.Add(this.MyGameCard);
		List<GameCard> list2 = new List<GameCard>();
		List<GameCard> childCards = this.MyGameCard.GetChildCards();
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				list2.Add(gameCard);
			}
			else
			{
				list.Add(gameCard);
			}
			gameCard = gameCard.Child;
		}
		foreach (GameCard gameCard2 in childCards)
		{
			gameCard2.RemoveFromStack();
		}
		WorldManager.instance.Restack(list);
		WorldManager.instance.Restack(list2);
	}

	public void DestroyChildrenMatchingPredicateAndRestack(Predicate<CardData> pred, int count)
	{
		GameCard parent = this.MyGameCard.Parent;
		List<GameCard> list = new List<GameCard>();
		list.Add(this.MyGameCard);
		List<GameCard> list2 = new List<GameCard>();
		GameCard gameCard = this.MyGameCard.Child;
		while (gameCard != null)
		{
			if (pred(gameCard.CardData))
			{
				if (count > 0)
				{
					list2.Add(gameCard);
					count--;
				}
				else
				{
					list.Add(gameCard);
				}
			}
			else
			{
				list.Add(gameCard);
			}
			gameCard = gameCard.Child;
		}
		foreach (GameCard gameCard2 in list2)
		{
			gameCard2.DestroyCard(true, false);
		}
		WorldManager.instance.Restack(list);
		this.MyGameCard.Parent = parent;
	}

	public void AddStatusEffect(StatusEffect effect)
	{
		if (this.HasStatusEffectOfType(effect))
		{
			return;
		}
		if (this.MyGameCard.IsDemoCard)
		{
			return;
		}
		this.StatusEffects.Add(effect);
		effect.ParentCard = this;
		this.MyGameCard.StatusEffectsChanged();
		if (this is BaseVillager)
		{
			QuestManager.instance.SpecialActionComplete(string.Format("add_status_{0}", effect), null);
		}
	}

	public void RemoveStatusEffect(StatusEffect effect)
	{
		if (this.StatusEffects.RemoveAll((StatusEffect x) => x == effect) != 0)
		{
			this.MyGameCard.StatusEffectsChanged();
			QuestManager.instance.SpecialActionComplete(string.Format("remove_status_{0}", effect), null);
		}
	}

	public void RemoveStatusEffect<T>() where T : StatusEffect
	{
		if (this.StatusEffects.RemoveAll((StatusEffect x) => x.GetType() == typeof(T)) != 0)
		{
			this.MyGameCard.StatusEffectsChanged();
			QuestManager.instance.SpecialActionComplete("remove_status_" + typeof(T).Name, null);
		}
	}

	public void RemoveStatusEffect(Type t)
	{
		if (this.StatusEffects.RemoveAll((StatusEffect x) => x.GetType() == t) != 0)
		{
			this.MyGameCard.StatusEffectsChanged();
			QuestManager.instance.SpecialActionComplete("remove_status_" + t.Name, null);
		}
	}

	public void RemoveAllStatusEffects()
	{
		this.StatusEffects.Clear();
		this.MyGameCard.StatusEffectsChanged();
	}

	public List<string> GetPossibleDrops()
	{
		FishingSpot fishingSpot = this as FishingSpot;
		if (fishingSpot != null)
		{
			List<string> cardsInBag = fishingSpot.NormalCardBag.GetCardsInBag();
			cardsInBag.AddRange(fishingSpot.FisherCardBag.GetCardsInBag());
			return cardsInBag;
		}
		Harvestable harvestable = this as Harvestable;
		if (harvestable != null)
		{
			return harvestable.MyCardBag.GetCardsInBag();
		}
		Enemy enemy = this as Enemy;
		if (enemy != null)
		{
			List<string> cardsInBag2 = enemy.Drops.GetCardsInBag();
			if (enemy.CanHaveInventory)
			{
				cardsInBag2.AddRange(enemy.PossibleEquipables.Select<Equipable, string>((Equipable x) => x.Id).ToList<string>());
			}
			return cardsInBag2;
		}
		return new List<string>();
	}

	public bool HasUndiscoveredCardInDrops()
	{
		foreach (string text in this.GetPossibleDrops().Distinct<string>().ToList<string>())
		{
			if (!WorldManager.instance.CurrentSave.FoundCardIds.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveStatusEffect(string statusEffect)
	{
		if (!StatusEffect.StatusEffectExists(statusEffect))
		{
			Debug.LogError("Status effect with name " + statusEffect + " does not exist");
			return;
		}
		QuestManager.instance.SpecialActionComplete("remove_status_" + statusEffect, null);
		if (this.StatusEffects.RemoveAll((StatusEffect x) => x.GetType().Name == statusEffect) > 0)
		{
			this.MyGameCard.StatusEffectsChanged();
		}
	}

	public StatusEffect GetStatusEffectWithName(string statusEffect)
	{
		if (!StatusEffect.StatusEffectExists(statusEffect))
		{
			Debug.LogError("Status effect with name " + statusEffect + " does not exist");
			return null;
		}
		return StatusEffect.CreateStatusEffectFromName(statusEffect);
	}

	public void AddStatusEffect(string statusEffect)
	{
		if (!StatusEffect.StatusEffectExists(statusEffect))
		{
			Debug.LogError("Status effect with name " + statusEffect + " does not exist");
			return;
		}
		StatusEffect statusEffect2 = StatusEffect.CreateStatusEffectFromName(statusEffect);
		this.AddStatusEffect(statusEffect2);
	}

	public bool HasStatusEffectOfType<T>() where T : StatusEffect
	{
		for (int i = 0; i < this.StatusEffects.Count; i++)
		{
			if (this.StatusEffects[i].GetType() == typeof(T))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyStatusEffect()
	{
		return this.StatusEffects.Count > 0;
	}

	public bool HasStatusEffectOfType(StatusEffect effect)
	{
		for (int i = 0; i < this.StatusEffects.Count; i++)
		{
			if (this.StatusEffects[i].GetType() == effect.GetType())
			{
				return true;
			}
		}
		return false;
	}

	public List<Equipable> GetAllEquipables()
	{
		List<Equipable> list = new List<Equipable>();
		if (this.MyGameCard == null)
		{
			return list;
		}
		foreach (GameCard gameCard in this.MyGameCard.EquipmentChildren)
		{
			Equipable equipable = gameCard.CardData as Equipable;
			if (equipable != null)
			{
				list.Add(equipable);
			}
		}
		return list;
	}

	public bool HasEquipableWithId(string id)
	{
		foreach (GameCard gameCard in this.MyGameCard.EquipmentChildren)
		{
			Equipable equipable = gameCard.CardData as Equipable;
			if (equipable != null && equipable.Id == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEquipableOfType<T>() where T : Equipable
	{
		foreach (GameCard gameCard in this.MyGameCard.EquipmentChildren)
		{
			Equipable equipable = gameCard.CardData as Equipable;
			if (equipable != null && equipable.GetType() == typeof(T))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEquipableOfEquipableType(EquipableType type)
	{
		foreach (GameCard gameCard in this.MyGameCard.EquipmentChildren)
		{
			Equipable equipable = gameCard.CardData as Equipable;
			if (equipable != null && equipable.EquipableType == type)
			{
				return true;
			}
		}
		return false;
	}

	public Equipable GetEquipableOfEquipableType(EquipableType type)
	{
		foreach (GameCard gameCard in this.MyGameCard.EquipmentChildren)
		{
			Equipable equipable = gameCard.CardData as Equipable;
			if (equipable != null && equipable.EquipableType == type)
			{
				return equipable;
			}
		}
		return null;
	}

	public bool WorkerAmountMet()
	{
		if (this.WorkerAmount <= 0)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < this.MyGameCard.WorkerChildren.Count; i++)
		{
			if (this.MyGameCard.WorkerChildren[i] != null)
			{
				num++;
			}
		}
		return num >= this.WorkerAmount;
	}

	public static string CardToTermId(CardData card)
	{
		if (card is Blueprint)
		{
			string text = card.Id;
			text = text.Replace("blueprint_", "");
			return "idea_" + text;
		}
		return "card_" + card.Id;
	}

	public bool AllCardsInStackMatchPred(CardData card, Predicate<CardData> pred)
	{
		if (card.MyGameCard == null)
		{
			return pred(card);
		}
		return WorldManager.instance.AllCardsInStackMatchPred(card.MyGameCard, delegate(GameCard x)
		{
			Combatable combatable = x.CardData as Combatable;
			return combatable != null && combatable.CanAttack && !(combatable is Animal);
		});
	}

	public int GetChildCount()
	{
		if (this.MyGameCard == null)
		{
			return 0;
		}
		return this.MyGameCard.GetChildCount();
	}

	public void LogCardReferences()
	{
		List<ICardReference> list = (from x in ((WorldManager.instance != null) ? WorldManager.instance.GameDataLoader : new GameDataLoader(true, true)).DetermineCardReferences()
			where x.ReferencedCardId == this.Id
			select x).ToList<ICardReference>();
		Debug.Log(string.Format("Referenced {0} times: {1}", list.Count, string.Join<ICardReference>(", ", list)));
	}

	public void LogBlueprintUses()
	{
		List<Blueprint> list = ((WorldManager.instance != null) ? WorldManager.instance.GameDataLoader : new GameDataLoader(true, true)).BlueprintPrefabs.Where<Blueprint>((Blueprint bp) => this.BlueprintUsesCard(bp, this.Id)).ToList<Blueprint>();
		Debug.Log(string.Format("Used in blueprint {0} times:\n{1}", list.Count, string.Join<Blueprint>("\n", list)));
	}

	private bool BlueprintUsesCard(Blueprint bp, string id)
	{
		for (int i = 0; i < bp.Subprints.Count; i++)
		{
			Subprint subprint = bp.Subprints[i];
			for (int j = 0; j < subprint.RequiredCards.Length; j++)
			{
				if (subprint.RequiredCards[j] == id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetDollarCountInStack(bool includeInChest)
	{
		if (includeInChest)
		{
			return this.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().Sum<ICurrency>((ICurrency x) => x.CurrencyValue);
		}
		return this.ChildrenMatchingPredicate((CardData x) => x is Dollar).Cast<Dollar>().Sum<Dollar>((Dollar x) => x.DollarValue);
	}

	public void UpdateRequirementResultsInStack(RequirementType requirementType, int add, GameCard card)
	{
		GameCard gameCard = ((this.MyGameCard.WorkerHolder != null) ? this.MyGameCard.WorkerHolder : this.MyGameCard.GetRootCard());
		string text = string.Format("{0}_{1}", card.CardData.Id, requirementType);
		MonthlyRequirementResult monthlyRequirementResult = ((gameCard.CardData.MonthlyRequirementResult != null) ? gameCard.CardData.MonthlyRequirementResult : new MonthlyRequirementResult());
		if (monthlyRequirementResult.results.ContainsKey(text))
		{
			monthlyRequirementResult.results[text].Amount += add;
			if (monthlyRequirementResult.results[text].Card != card)
			{
				monthlyRequirementResult.results[text].CardAmount++;
			}
		}
		else
		{
			monthlyRequirementResult.results[text] = new MonthlyResult();
			monthlyRequirementResult.results[text].Amount = add;
			monthlyRequirementResult.results[text].CardAmount = 1;
			monthlyRequirementResult.results[text].Card = card;
			monthlyRequirementResult.results[text].Type = requirementType;
		}
		gameCard.CardData.MonthlyRequirementResult = monthlyRequirementResult;
	}

	public string GetRequirementDescription(GameCard card, int multipleAmount = 1, bool onlyShowCurrentlySatisfied = true)
	{
		CardData.reqList.Clear();
		Func<CardRequirement, bool> <>9__0;
		foreach (RequirementHolder requirementHolder in this.RequirementHolders)
		{
			bool flag = true;
			bool flag2 = true;
			CardData.tmpList.Clear();
			foreach (CardRequirement cardRequirement in requirementHolder.CardRequirements)
			{
				string text = cardRequirement.RequirementDescriptionNeed(multipleAmount);
				CardData.tmpList.Add(text);
			}
			string text2 = string.Join("& ", CardData.tmpList);
			CardData.tmpList.Clear();
			foreach (CardRequirement cardRequirement2 in requirementHolder.CardRequirements)
			{
				string text3 = cardRequirement2.RequirementDescriptionNeedNegative(multipleAmount);
				CardData.tmpList.Add(text3);
			}
			string text4 = string.Join("& ", CardData.tmpList);
			CardData.tmpList.Clear();
			foreach (CardRequirementResult cardRequirementResult in requirementHolder.PositiveResults)
			{
				string text5 = cardRequirementResult.RequirementDescriptionPositive(multipleAmount, card);
				if (!string.IsNullOrEmpty(text5))
				{
					CardData.tmpList.Add(text5);
				}
			}
			string text6 = string.Join(", ", CardData.tmpList);
			CardData.tmpList.Clear();
			foreach (CardRequirementResult cardRequirementResult2 in requirementHolder.NegativeResults)
			{
				string text7 = cardRequirementResult2.RequirementDescriptionNegative(multipleAmount, card);
				if (!string.IsNullOrEmpty(text7))
				{
					CardData.tmpList.Add(text7);
				}
			}
			string text8 = string.Join(", ", CardData.tmpList);
			if (onlyShowCurrentlySatisfied)
			{
				IEnumerable<CardRequirement> cardRequirements = requirementHolder.CardRequirements;
				Func<CardRequirement, bool> func;
				if ((func = <>9__0) == null)
				{
					func = (<>9__0 = (CardRequirement x) => x.Satisfied(card));
				}
				if (cardRequirements.All<CardRequirement>(func))
				{
					flag2 = false;
				}
				else
				{
					flag = false;
				}
			}
			if (!string.IsNullOrEmpty(text6) && flag)
			{
				CardData.reqList.Add(text2 + "\u00a0" + text6);
			}
			if (!string.IsNullOrEmpty(text8) && flag2)
			{
				CardData.reqList.Add(text4 + "\u00a0" + text8);
			}
		}
		return string.Join("\n", CardData.reqList);
	}

	public void SetCardDamaged(CardDamageType type)
	{
		this.IsDamaged = true;
		this.DamageType = type;
		this.MyGameCard.UpdateCardPalette();
	}

	public void SetCardUndamaged()
	{
		this.IsDamaged = false;
		this.DamageType = CardDamageType.None;
		this.MyGameCard.UpdateCardPalette();
	}

	[Header("General")]
	public string Id = "";

	[Header("Mewtations: Dogma Extensions")]
	[ExtraData("hidden_offering_value")]
	public int HiddenOfferingValue = 0;

	[ExtraData("backpack_capacity")]
	public int BackpackCapacity = 0;

	public bool IsEventCard = false;

	[ExtraData("custom_name")]
	[HideInInspector]
	public string CustomName;

	[HideInInspector]
	[NonSerialized]
	public string descriptionOverride;

	[HideInInspector]
	[NonSerialized]
	public string nameOverride;

	[Term]
	public string NameTerm = "";

	[Term]
	public string DescriptionTerm = "";

	public PickupSoundGroup PickupSoundGroup;

	public AudioClip PickupSound;

	[HideInInspector]
	public string UniqueId = "";

	[HideInInspector]
	public string ParentUniqueId = "";

	[HideInInspector]
	public string EquipmentHolderUniqueId = "";

	[HideInInspector]
	public string WorkerHolderUniqueId = "";

	[HideInInspector]
	public int WorkerIndex = -1;

	public int Value = 1;

	public Sprite Icon;

	[HideInInspector]
	public GameCard MyGameCard;

	[HideInInspector]
	public bool IsFoil;

	public bool IsShiny;

	public CardType MyCardType;

	public bool IsBuilding;

	public bool IsCookedFood;

	public bool HideFromCardopedia;

	[HideInInspector]
	public List<StatusEffect> StatusEffects = new List<StatusEffect>();

	[HideInInspector]
	[ExtraData("creation_month")]
	public int CreationMonth;

	[HideInInspector]
	public CardUpdateType CardUpdateType;

	[HideInInspector]
	public float ExpectedValue;

	public bool HasUniquePalette;

	public CardPalette MyPalette;

	public List<ExtraCardData> LeftoverExtraData = new List<ExtraCardData>();

	internal string _name;

	private string _oldNameTerm;

	private string _cachedConnectorString;

	[Header("Cities options")]
	public int CitiesValue = 10;

	public int WorkerAmount;

	public bool EducatedWorkers;

	[HideInInspector]
	[ExtraData("output_direction")]
	public Vector3 OutputDir = Vector3.right;

	[SerializeReference]
	public List<RequirementHolder> RequirementHolders = new List<RequirementHolder>();

	public MonthlyRequirementResult MonthlyRequirementResult;

	[HideInInspector]
	public bool IsDamaged;

	[HideInInspector]
	public CardDamageType DamageType;

	[HideInInspector]
	[ExtraData("is_on")]
	public bool IsOn = true;

	[Header("Energy options")]
	public List<CardConnectorData> EnergyConnectors = new List<CardConnectorData>();

	private Dictionary<string, string> methodToActionId = new Dictionary<string, string>();

	private static List<string> tmpList = new List<string>();

	private static List<string> reqList = new List<string>();
}
