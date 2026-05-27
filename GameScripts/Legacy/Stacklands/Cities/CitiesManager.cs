using Mewtations.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedEconomyLoop)]
    public class CitiesManager : MonoBehaviour
{
	public int Wellbeing
	{
		get
		{
			return this._wellbeing;
		}
		set
		{
			this.PreviousWellbeing = this._wellbeing;
			this._wellbeing = Mathf.Clamp(value, 0, 200);
			this.UpdateCityState();
		}
	}

	private void Start()
	{
		CitiesManager.instance = this;
		this.InitStrechSource();
		this.UpdateCityState();
	}

	private void InitStrechSource()
	{
		this.stretchSource = AudioManager.me.GetSource(base.transform, true);
		this.stretchSource.pitch = 0f;
		this.stretchSource.volume = 0f;
		this.stretchSource.clip = AudioManager.me.EnergyStrech;
		this.stretchSource.reverbZoneMix = 0f;
		this.stretchSource.spatialBlend = 0f;
		this.stretchSource.bypassListenerEffects = false;
		this.stretchSource.loop = true;
		this.stretchSource.Play();
	}

	private void Update()
	{
		if (!(WorldManager.instance.CurrentBoard == null))
		{
			GameBoard currentBoard = WorldManager.instance.CurrentBoard;
			if (!(((currentBoard != null) ? currentBoard.Id : null) != "cities"))
			{
				WorldManager.instance.CardQuery.GetCardsNonAlloc<Worker>(this.WorkersOnBoard);
				WorldManager.instance.GetCardsImplementingInterfaceNonAlloc<HousingConsumer>(this.HousingConsumers);
				this.HomelessHousingConsumers.Clear();
				for (int i = this.HousingConsumers.Count - 1; i >= 0; i--)
				{
					if (this.HousingConsumers[i].Housing == null && this.HousingConsumers[i].GetHousingSpaceRequired() > 0)
					{
						this.HomelessHousingConsumers.Add(this.HousingConsumers[i]);
					}
				}
				this.CheckConflict();
				this.CheckCutscenes();
				this.CheckForEvents();
				this.DrawConnectors();
				this.DrawConnectorAudio();
				return;
			}
		}
		if (WorldManager.instance.CanUseTransport)
		{
			this.DrawConnectors();
		}
	}

	public void DrawConnectorAudio()
	{
		if (this.DrawingConnector != null)
		{
			AudioClip stretchSoundForType = this.DrawingConnector.GetStretchSoundForType(this.DrawingConnector.ConnectionType);
			this.stretchSource.clip = stretchSoundForType;
			if (!this.stretchSource.isPlaying)
			{
				this.stretchSource.Play();
			}
			Vector3 vector = this.DrawingConnector.transform.position + Vector3.down * 0.01f;
			Vector3 mouseWorldPosition = WorldManager.instance.mouseWorldPosition;
			float num = Vector3.Distance(vector, mouseWorldPosition);
			float num2 = Mathf.Abs(num - this.prevDist);
			this.prevDist = num;
			if (num2 > 0.001f)
			{
				this.targetVolume = 1f;
			}
			else
			{
				this.targetVolume = 0f;
			}
			if (this.DrawingConnector.ConnectionType == ConnectionType.LV || this.DrawingConnector.ConnectionType == ConnectionType.HV)
			{
				this.targetPitch = Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(1f, 8f, num));
			}
			else
			{
				this.targetPitch = 1f;
			}
		}
		else
		{
			this.targetVolume = 0f;
		}
		this.stretchSource.volume = Mathf.Lerp(this.stretchSource.volume, this.targetVolume, Time.deltaTime * 15f);
		this.stretchSource.pitch = Mathf.Lerp(this.stretchSource.pitch, this.targetPitch, Time.deltaTime * 5f);
	}

	public void CheckForEvents()
	{
		EventCard card = WorldManager.instance.CardQuery.GetCard<EventCard>();
		if (card != null && card.EventIsActive)
		{
			this.ActiveEvent = new CardEventType?(card.EventType);
			return;
		}
		this.ActiveEvent = null;
	}

	public void CheckCutscenes()
    {
        if (!Mewtations.Core.LegacyRuntimeFlags.EnableCitiesSystem) return;
		if (TransitionScreen.InTransition || this._wellbeing <= 0 || WorldManager.instance.InAnimation)
		{
			return;
		}
		if (this._wellbeing >= 40)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_wellbeing_30");
		}
		if (this._wellbeing >= 20)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_wellbeing_20");
		}
		if (this._wellbeing < 10)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_wellbeing_10");
		}
	}

	public void StartDrawCable(CardConnector connector)
	{
		this.DrawingConnector = connector;
	}

	public void StopDrawCable(CardConnector endConnector)
	{
		if (this.DrawingConnector != null && endConnector != null && endConnector.CardDirection != this.DrawingConnector.CardDirection && endConnector.ConnectionType == this.DrawingConnector.ConnectionType && this.DrawingConnector.Parent != endConnector.Parent)
		{
			this.DrawingConnector.SetConnectedNode(endConnector);
			QuestManager.instance.SpecialActionComplete("cities_cable_connected", this.DrawingConnector.Parent.CardData);
			this.DrawingConnector.Parent.CardData.NotifyEnergyConsumers();
		}
		this.DrawingConnector = null;
	}

	private Vector3 DetermineConnectorMiddle(Vector3 start, Vector3 end, ConnectionType conn)
	{
		if (conn == ConnectionType.LV || conn == ConnectionType.HV)
		{
			float num = Mathf.Abs(start.x - end.x);
			float num2 = 1f - Mathf.InverseLerp(0f, 3f, num);
			float num3 = (end.z - start.z) * 0.3f;
			float num4 = 0.75f;
			return Vector3.Lerp(start, end, 0.5f + num3) + new Vector3(0f, 0f, -num4) * num2;
		}
		return Vector3.Lerp(start, end, 0.5f);
	}

	public void DrawConnectors()
	{
		if (this.DrawingConnector != null)
		{
			Vector3 vector = this.DrawingConnector.transform.position + Vector3.down * 0.01f;
			Vector3 mouseWorldPosition = WorldManager.instance.mouseWorldPosition;
			Vector3 vector2 = this.DetermineConnectorMiddle(vector, mouseWorldPosition, this.DrawingConnector.ConnectionType);
			if (this.DrawingConnector.ConnectionType == ConnectionType.LV || this.DrawingConnector.ConnectionType == ConnectionType.HV)
			{
				if ((this.DrawingConnector.Middle - vector2).sqrMagnitude >= 100f)
				{
					this.DrawingConnector.MiddleVelo = Vector3.zero;
					this.DrawingConnector.Middle = vector2;
				}
				this.DrawingConnector.Middle = FRILerp.Spring(this.DrawingConnector.Middle, vector2, 25f, 10f, ref this.DrawingConnector.MiddleVelo);
			}
			else if (this.DrawingConnector.ConnectionType == ConnectionType.Sewer)
			{
				this.DrawingConnector.Middle = vector2;
			}
			else if (this.DrawingConnector.ConnectionType == ConnectionType.Transport)
			{
				this.DrawingConnector.Middle = vector2;
			}
			DrawManager.instance.DrawShape(this.GetShapeForConnectionType(this.DrawingConnector.ConnectionType, vector, this.DrawingConnector.Middle, mouseWorldPosition));
		}
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.CardConnectorChildren.Count != 0 && !(gameCard.MyBoard != currentBoard))
			{
				foreach (CardConnector cardConnector in gameCard.CardConnectorChildren)
				{
					if (cardConnector.ConnectedNode != null && cardConnector.CardDirection == CardDirection.output)
					{
						Vector3 vector3 = cardConnector.transform.position + Vector3.down * 0.01f;
						Vector3 vector4 = cardConnector.ConnectedNode.transform.position + Vector3.down * 0.01f;
						Vector3 vector5 = this.DetermineConnectorMiddle(vector3, vector4, cardConnector.ConnectionType);
						if ((cardConnector.Middle - vector5).sqrMagnitude >= 100f)
						{
							cardConnector.MiddleVelo = Vector3.zero;
							cardConnector.Middle = vector5;
						}
						if (cardConnector.ConnectionType == ConnectionType.LV || cardConnector.ConnectionType == ConnectionType.HV)
						{
							cardConnector.Middle = FRILerp.Spring(cardConnector.Middle, vector5, 25f, 10f, ref cardConnector.MiddleVelo);
						}
						else if (cardConnector.ConnectionType == ConnectionType.Sewer)
						{
							cardConnector.Middle = vector5;
						}
						else if (cardConnector.ConnectionType == ConnectionType.Transport)
						{
							cardConnector.Middle = FRILerp.Spring(cardConnector.Middle, vector5, 30f, 30f, ref cardConnector.MiddleVelo);
						}
						DrawManager.instance.DrawShape(this.GetShapeForConnectionType(cardConnector.ConnectionType, vector3, cardConnector.Middle, vector4));
					}
				}
			}
		}
	}

	private IShape GetShapeForConnectionType(ConnectionType connectionType, Vector3 start, Vector3 middle, Vector3 end)
	{
		if (connectionType == ConnectionType.LV || connectionType == ConnectionType.HV)
		{
			return new EnergyCable
			{
				Start = start,
				Middle = middle,
				End = end,
				IsLowVoltage = (connectionType == ConnectionType.LV)
			};
		}
		if (connectionType == ConnectionType.Sewer)
		{
			return new SewerPipe
			{
				Start = start,
				Middle = middle,
				End = end
			};
		}
		if (connectionType != ConnectionType.Transport)
		{
			return null;
		}
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return new TransportArrow
			{
				Start = start,
				Middle = middle,
				End = end
			};
		}
		return new TransportArrowMainland
		{
			Start = start,
			Middle = middle,
			End = end
		};
	}

	public void CheckConflict()
	{
		if (WorldManager.instance.HasFoundCard("blueprint_barrack") && this.Wellbeing >= 25)
		{
			int currentMonth = WorldManager.instance.Time.CurrentMonth;
			if (this.NextConflictMonth < currentMonth - 1)
			{
				this.NextConflictMonth = this.GetNextConflictMonth(currentMonth);
				Debug.Log("Updated Conflict Month to : " + this.NextConflictMonth.ToString());
				return;
			}
		}
		else
		{
			this.NextConflictMonth = -1;
		}
	}

	public int GetNextConflictMonth(int currentMonth)
	{
		return currentMonth + Random.Range(this.MonthsBetweenConflict - this.MaxRandomOffset, this.MonthsBetweenConflict + this.MaxRandomOffset);
	}

	public static string GetCityStateTranslated(CityState state)
	{
		return SokLoc.Translate("label_wellbeing_" + state.ToString().ToLower());
	}

	public void CheckCityHealth()
    {
        if (!Mewtations.Core.LegacyRuntimeFlags.EnableCitiesSystem) return;
	}

	public void AddWellbeing(int wellbeing)
	{
		this.Wellbeing += wellbeing;
	}

	public bool ShouldTriggerEvent()
	{
		return WorldManager.instance.Time.CurrentMonth > this.TriggerEventFromMonth;
	}

	public CardId GetEvent()
	{
		List<string> list = (from x in this.EventList.GetCardsInBag()
			where !WorldManager.instance.CurrentRunVariables.SpawnedEventIds.Contains(x)
			select x).ToList<string>();
		if (list.Count <= 0)
		{
			WorldManager.instance.CurrentRunVariables.SpawnedEventIds = new List<string>();
			list = this.EventList.GetCardsInBag();
		}
		string text = list.Choose<string>();
		WorldManager.instance.CurrentRunVariables.SpawnedEventIds.Add(text);
		return new CardId(text);
	}

	public void UpdateCityState()
    {
        if (!Mewtations.Core.LegacyRuntimeFlags.EnableCitiesSystem) return;
		if (this._wellbeing < 0)
		{
			this._wellbeing = 0;
		}
		this.CityState = CitiesManager.GetCityStateForWellbeing(this._wellbeing);
	}

	public static CityState GetCityStateForWellbeing(int amount)
	{
		if (amount <= 10 && amount > 0)
		{
			return CityState.Miserable;
		}
		if (amount <= 20 && amount > 10)
		{
			return CityState.Unhappy;
		}
		if (amount < 40 && amount > 20)
		{
			return CityState.Normal;
		}
		if (amount >= 40 && amount < 50)
		{
			return CityState.Happy;
		}
		if (amount >= 50)
		{
			return CityState.Euphoric;
		}
		return CityState.Gameover;
	}

	private void UpdateEnergyAmount()
	{
	}

	public bool TryConsumeEnergy(int amount, GameCard consumer)
	{
		if (WorldManager.instance.DebugNoEnergyEnabled)
		{
			return true;
		}
		if (amount == 0)
		{
			return true;
		}
		List<IEnergy> list = WorldManager.instance.CardQuery.GetCardsImplementingInterface<IEnergy>();
		if (list.Sum<IEnergy>((IEnergy x) => x.EnergyAmount) < amount)
		{
			return false;
		}
		list = list.OrderBy<IEnergy, int>(delegate(IEnergy x)
		{
			CardData cardData = x as CardData;
			if (cardData is Battery)
			{
				return -1000 - cardData.MyGameCard.GetCardIndex();
			}
			return -cardData.MyGameCard.GetCardIndex();
		}).ToList<IEnergy>();
		foreach (IEnergy energy in list)
		{
			int num = Mathf.Min(energy.EnergyAmount, amount);
			energy.UseEnergy(num);
			amount -= num;
			if (amount == 0)
			{
				break;
			}
		}
		return true;
	}

	public int TryUseDollars(List<ICurrency> currencyList, int cost, bool onlyTakeIfAmountMet = false, bool spawnSmoke = false, bool keepOnStack = false)
	{
		if (WorldManager.instance.DebugNoFoodEnabled)
		{
			return 0;
		}
		if (cost == 0)
		{
			return cost;
		}
		if (currencyList.Count <= 0)
		{
			return cost;
		}
		if (onlyTakeIfAmountMet)
		{
			if (currencyList.Sum<ICurrency>((ICurrency x) => x.CurrencyValue) < cost)
			{
				return cost;
			}
		}
		int num = currencyList.Sum<ICurrency>((ICurrency x) => x.CurrencyValue);
		int num2 = Mathf.Min(cost, num);
		GameCard rootCard = currencyList[0].Card.MyGameCard.GetRootCard();
		if (num > 0)
		{
			foreach (Creditcard creditcard in (from Creditcard x in currencyList.Where<ICurrency>(delegate(ICurrency x)
				{
					Creditcard creditcard2 = x as Creditcard;
					return creditcard2 != null && creditcard2.DollarCount > 0;
				})
				orderby x.DollarCount
				select x).ToList<Creditcard>())
			{
				int num3 = Mathf.Min(creditcard.CurrencyValue, cost);
				creditcard.UseCurrency(num3, spawnSmoke);
				num2 -= num3;
				if (num2 == 0)
				{
					break;
				}
			}
			if (num2 > 0)
			{
				for (int i = 0; i < this.takeOrder.Length; i++)
				{
					int curBillAmount = this.takeOrder[i];
					int num4 = num2 / curBillAmount;
					num4 = Mathf.Min(currencyList.Count<ICurrency>(delegate(ICurrency x)
					{
						Dollar dollar3 = x as Dollar;
						return dollar3 != null && dollar3.DollarValue == curBillAmount;
					}), num4);
					num2 -= num4 * curBillAmount;
					Func<ICurrency, bool> <>9__5;
					for (int j = 0; j < num4; j++)
					{
						Func<ICurrency, bool> func;
						if ((func = <>9__5) == null)
						{
							func = (<>9__5 = delegate(ICurrency x)
							{
								Dollar dollar4 = x as Dollar;
								return dollar4 != null && dollar4.DollarValue == curBillAmount;
							});
						}
						Dollar dollar = currencyList.Where<ICurrency>(func).FirstOrDefault<ICurrency>() as Dollar;
						currencyList.Remove(dollar);
						dollar.UseCurrency(dollar.CurrencyValue, spawnSmoke);
					}
					if (num2 <= 0)
					{
						break;
					}
				}
			}
			if (num2 > 0 && currencyList.Count > 0)
			{
				Dollar dollar2 = (from x in currencyList
					where x is Dollar
					orderby x.CurrencyValue
					select x).FirstOrDefault<ICurrency>() as Dollar;
				int num5 = dollar2.DollarValue - num2;
				List<GameCard> list = WorldManager.instance.CreateDollarsFromValue(num5, dollar2.Position, true);
				currencyList.AddRange(list.Select<GameCard, ICurrency>((GameCard x) => x.CardData as ICurrency));
				currencyList.Remove(dollar2);
				dollar2.UseCurrency(dollar2.CurrencyValue, spawnSmoke);
				num2 = 0;
			}
		}
		if (keepOnStack && currencyList.Count > 0 && rootCard != null)
		{
			currencyList[0].Card.MyGameCard.GetRootCard().SetParent(rootCard);
		}
		else if (currencyList.Count > 0)
		{
			currencyList[0].Card.MyGameCard.RemoveFromParent();
			currencyList[0].Card.MyGameCard.SendIt();
		}
		return cost - num2;
	}

	public IEnumerator ConsumeFood(int amount, Vector3 targetPos)
	{
		List<Food> foodToUse = this.GetFoodToUse(amount);
		int num = amount;
		using (List<Food>.Enumerator enumerator = foodToUse.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Food food = enumerator.Current;
				if (num <= 0)
				{
					break;
				}
				food.IsReserved = true;
				int num2 = Mathf.Min(num, food.FoodValue);
				num -= num2;
				food.FoodValue -= num2;
				food.MyGameCard.PushEnabled = false;
				if (food.FoodValue <= 0)
				{
					if (food.MyGameCard.HasParent && food.MyGameCard.HasChild)
					{
						GameCard parent = food.MyGameCard.Parent;
						GameCard child = food.MyGameCard.Child;
						food.MyGameCard.RemoveFromStack();
						parent.SetChild(child);
					}
					else
					{
						food.MyGameCard.RemoveFromStack();
					}
					Vector3 position = food.Position;
					if (!(food is FoodWarehouse))
					{
						food.IsConsumed = true;
						food.MyGameCard.SendToPositionCallback(targetPos, delegate
						{
							food.MyGameCard.DestroyCard(false, true);
						});
					}
					else
					{
						WorldManager.instance.CreateSmoke(food.Position);
					}
				}
				food.MyGameCard.PushEnabled = true;
				food.IsReserved = false;
			}
		}
		return null;
	}

	public List<Food> GetFoodToUse(int amount)
	{
		List<Food> list = (from x in WorldManager.instance.CardQuery.GetCards<Food>()
			where !x.IsReserved
			select x).ToList<Food>();
		if (list.Sum<Food>((Food x) => x.FoodValue) < amount)
		{
			return new List<Food>();
		}
		return list.Where<Food>((Food x) => x.FoodValue > 0).OrderByDescending<Food, int>(delegate(Food x)
		{
			bool flag = x.MyGameCard.GetCardWithStatusInStack() != null;
			FoodWarehouse foodWarehouse = x as FoodWarehouse;
			if (foodWarehouse != null)
			{
				Food food = WorldManager.instance.GameDataLoader.GetCardFromId(foodWarehouse.HeldCardId, true) as Food;
				if (food != null)
				{
					return food.FoodValue;
				}
			}
			if (flag)
			{
				return -3;
			}
			if (!x.IsCookedFood)
			{
				return -2;
			}
			return 0;
		}).ThenBy<Food, int>((Food x) => x.FoodValue)
			.ThenBy<Food, int>((Food x) => -x.MyGameCard.GetCardIndex())
			.ToList<Food>();
	}

	public static string GetAmountPrefix(int amount)
	{
		if (amount > 0)
		{
			return "+";
		}
		return "";
	}

	public static CitiesManager instance;

	[HideInInspector]
	private int _wellbeing;

	public int WellbeingStart = 30;

	[HideInInspector]
	public CityState CityState;

	public int TriggerEventFromMonth = 10;

	public CardBag EventList;

	public int MonthsBetweenConflict = 6;

	public int MaxRandomOffset = 1;

	public CardEventType? ActiveEvent;

	[HideInInspector]
	public int NextConflictMonth;

	[HideInInspector]
	public CardConnector DrawingConnector;

	public Material EnergyCableMaterial;

	[HideInInspector]
	public List<HousingConsumer> HousingConsumers = new List<HousingConsumer>();

	[HideInInspector]
	public List<Worker> WorkersOnBoard = new List<Worker>();

	[HideInInspector]
	public List<HousingConsumer> HomelessHousingConsumers = new List<HousingConsumer>();

	[HideInInspector]
	public int PreviousWellbeing;

	private float targetVolume;

	private float targetPitch = 1f;

	private float prevDist;

	private AudioSource stretchSource;

	private int[] takeOrder = new int[] { 10, 20, 50, 100 };
}



