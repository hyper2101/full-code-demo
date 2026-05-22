using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[ExecuteAlways]
public class ColorManager : MonoBehaviour
{
	private void Awake()
	{
		ColorManager.instance = this;
		this.InitNameToField();
		this.InitializeCardColorRules();
	}

	private void InitializeCardColorRules()
	{
		this.AddCardColorRule(this.CalamityCards, delegate(CardData c)
		{
			if (WorldManager.instance.CurrentView != ViewType.Calamity)
			{
				return false;
			}
			if (!c.IsDamaged)
			{
				EventCard eventCard = c as EventCard;
				return eventCard != null && !eventCard.IsPositiveEvent;
			}
			return true;
		});
		this.AddCardColorRule(this.EnergyCards, (CardData c) => (c.MyGameCard.HasConnectorOfType(ConnectionType.HV) || c.MyGameCard.HasConnectorOfType(ConnectionType.LV)) && WorldManager.instance.CurrentView == ViewType.Energy);
		this.AddCardColorRule(this.SewerCards, (CardData c) => c.MyGameCard.HasConnectorOfType(ConnectionType.Sewer) && WorldManager.instance.CurrentView == ViewType.Sewer);
		this.AddCardColorRule(this.TransportCards, (CardData c) => c.MyGameCard.HasConnectorOfType(ConnectionType.Transport) && WorldManager.instance.CurrentView == ViewType.Transport && WorldManager.instance.CurrentBoard.Id == "cities");
		this.AddCardColorRule(this.TransportMainCards, (CardData c) => c.MyGameCard.HasConnectorOfType(ConnectionType.Transport) && WorldManager.instance.CurrentView == ViewType.Transport && WorldManager.instance.CurrentBoard.Id != "cities");
		using (List<GameBoard>.Enumerator enumerator = Object.FindObjectsOfType<GameBoard>().ToList<GameBoard>().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				GameBoard board = enumerator.Current;
				this.AddCardColorRule(board.BoardOptions.CardBackgroundPallete, (CardData c) => WorldManager.instance.CurrentView != ViewType.Default && WorldManager.instance.GetCurrentBoardSafe().Id == board.Id);
			}
		}
		this.AddCardColorRule(this.EnergyProducer, (CardData c) => c.MyGameCard.CardData.EnergyConnectors.Any<CardConnectorData>((CardConnectorData x) => x.EnergyConnectionType == CardDirection.output && (x.EnergyConnectionStrength == ConnectionType.LV || x.EnergyConnectionStrength == ConnectionType.HV)));
		this.AddCardColorRule(this.Landmark, (CardData c) => c is Landmark);
		this.AddCardColorRule(this.Gold, (CardData c) => c is Dollar);
		this.AddCardColorRule(this.Energy, (CardData c) => c is Energy);
		this.AddCardColorRule(this.Gold, (CardData c) => c is DragonEgg);
		this.AddCardColorRule(this.Gold, (CardData c) => c.Id == "merch");
		this.AddCardColorRule(this.Ingredient, delegate(CardData c)
		{
			if (c.MyCardType == CardType.Food)
			{
				Food food = c as Food;
				if (food != null)
				{
					return food.FoodValue < 1;
				}
			}
			return false;
		});
		this.AddCardColorRule(this.Poop, (CardData c) => c is Poop);
		this.AddCardColorRule(this.Spirit, (CardData c) => c is Spirit);
		this.AddCardColorRule(this.Curse, (CardData c) => c is Curse);
		this.AddCardColorRule(this.Happiness, (CardData c) => c.Id == "happiness" || c.Id == "euphoria");
		this.AddCardColorRule(this.Unhappiness, (CardData c) => c.Id == "unhappiness");
		this.AddCardColorRule(this.Rumor, (CardData c) => c.MyCardType == CardType.Rumors);
		this.AddCardColorRule(this.Location, (CardData c) => c.MyCardType == CardType.Locations);
		this.AddCardColorRule(this.Gold, delegate(CardData c)
		{
			Mimic mimic = c as Mimic;
			return mimic != null && !mimic.WasDetected;
		});
		this.AddCardColorRule(this.AggresiveMob, delegate(CardData c)
		{
			Mob mob = c as Mob;
			return (mob != null && mob.IsAggressive) || c is StrangePortal || c is PirateBoat || c is GoblinAttack;
		});
		this.AddCardColorRule(this.Fish, (CardData c) => c.MyCardType == CardType.Fish);
		this.AddCardColorRule(this.Gold, (CardData c) => c.Id == "gold" || c.Id == "goblet" || c.Id == "key" || c.Id == "treasure_chest" || c.Id == "temple" || c.Id == "shell" || c.Id == "sacred_key" || c.Id == "sacred_chest" || c.Id == "island_relic" || c.Id == "compass" || c.Id == "city_hall");
		this.AddCardColorRule(this.Corpse, (CardData c) => c.Id == "corpse");
		this.AddCardColorRule(this.Building, (CardData c) => c.MyCardType == CardType.Structures && c.IsBuilding);
		this.AddCardColorRule(this.Structure, (CardData c) => c.MyCardType == CardType.Structures && !c.IsBuilding);
		this.AddCardColorRule(this.Blueprint, (CardData c) => c.MyCardType == CardType.Ideas);
		this.AddCardColorRule(this.Resource, (CardData c) => c.MyCardType == CardType.Resources);
		this.AddCardColorRule(this.Food, (CardData c) => c.MyCardType == CardType.Food);
		this.AddCardColorRule(this.Mob, (CardData c) => c.MyCardType == CardType.Mobs);
		this.AddCardColorRule(this.Weather, (CardData c) => c.MyCardType == CardType.Weather);
		this.AddCardColorRule(this.Equipable, (CardData c) => c.MyCardType == CardType.Equipable);
		this.AddCardColorRule(this.Disaster, (CardData c) => c.MyCardType == CardType.Disaster);
		this.AddCardColorRule(this.Event, (CardData c) => c.MyCardType == CardType.Event);
		this.AddCardColorRule(this.Energy, (CardData c) => c is Battery);
	}

	private void InitNameToField()
	{
		foreach (FieldInfo fieldInfo in base.GetType().GetFields())
		{
			if (fieldInfo.FieldType == typeof(Color))
			{
				this.nameToField[fieldInfo.Name.ToLower()] = fieldInfo;
			}
		}
	}

	private void OnValidate()
	{
		ColorManager.instance = this;
	}

	public Color GetColorWithName(string colorName)
	{
		if (!this.nameToField.ContainsKey(colorName))
		{
			throw new ArgumentException("No color with name '" + colorName + "'");
		}
		return (Color)this.nameToField[colorName].GetValue(this);
	}

	public Color GetColor(UIColor col)
	{
		if (col == UIColor.Background)
		{
			return this.BackgroundColor;
		}
		if (col == UIColor.Button)
		{
			return this.ButtonColor;
		}
		if (col == UIColor.Text)
		{
			return this.TextColor;
		}
		if (col == UIColor.DisabledText)
		{
			return this.DisabledTextColor;
		}
		if (col == UIColor.Background2)
		{
			return this.BackgroundColor2;
		}
		if (col == UIColor.InactiveBackground)
		{
			return this.InactiveBackgroundColor;
		}
		return Color.cyan;
	}

	public void AddCardColorRule(CardPalette palette, Predicate<CardData> pred)
	{
		this.cardColorRules.Add(new CardColorRule(palette, pred));
	}

	public void InsertCardColorRule(int index, CardPalette palette, Predicate<CardData> pred)
	{
		this.cardColorRules.Insert(index, new CardColorRule(palette, pred));
	}

	public CardPalette GetPaletteForCard(CardData cd)
	{
		if (cd.HasUniquePalette)
		{
			return cd.MyPalette;
		}
		foreach (CardColorRule cardColorRule in this.cardColorRules)
		{
			if (cardColorRule.Predicate(cd))
			{
				return cardColorRule.Palette;
			}
		}
		return this.Default;
	}

	public static ColorManager instance;

	[Header("UI")]
	public Color ButtonColor;

	public Color BackgroundColor;

	public Color BackgroundColor2;

	public Color TextColor;

	public Color RedTextColor;

	public Color YellowTextColor;

	public Color DisabledTextColor;

	public Color DisabledButtonTextColor;

	public Color ButtonTextColor;

	public Color DisabledColor;

	public Color HoverButtonColor;

	public Color InactiveBackgroundColor;

	public Color CombatLineColor;

	public Color EffectiveCombatLineColor;

	public Color FloatingTextColorSuccess;

	public Color FloatingTextColorFailed;

	public Color HighVoltageConnector;

	public Color HighVoltageConnectorActive;

	public Color LowVoltageConnector;

	public Color LowVoltageConnectorActive;

	public Color SewerConnector;

	public Color SewerConnectorActive;

	public Color TransportConnector;

	public Color TransportConnectorActive;

	[Header("Cards")]
	public CardPalette Default;

	public CardPalette Structure;

	public CardPalette Blueprint;

	public CardPalette Location;

	public CardPalette Resource;

	public CardPalette Food;

	public CardPalette Gold;

	public CardPalette Mob;

	public CardPalette Fish;

	public CardPalette Rumor;

	public CardPalette Corpse;

	public CardPalette AggresiveMob;

	public CardPalette Building;

	public CardPalette Equipable;

	public CardPalette Happiness;

	public CardPalette Unhappiness;

	public CardPalette Curse;

	public CardPalette Spirit;

	public CardPalette Poop;

	public CardPalette Ingredient;

	public CardPalette Energy;

	public CardPalette Disaster;

	public CardPalette Event;

	public CardPalette Landmark;

	public CardPalette EnergyProducer;

	public CardPalette Weather;

	[Header("Cards 2")]
	public CardPalette EnergyCards;

	public CardPalette SewerCards;

	public CardPalette TransportCards;

	public CardPalette TransportMainCards;

	public CardPalette CalamityCards;

	[Header("Status Effect")]
	public Color StatusEffect_Poison_A;

	public Color StatusEffect_Poison_B;

	public Color StatusEffect_Drunk_A;

	public Color StatusEffect_Drunk_B;

	public Color StatusEffect_Spoiling_A;

	public Color StatusEffect_Spoiling_B;

	public Color StatusEffect_Stunned_A;

	public Color StatusEffect_Stunned_B;

	public Color StatusEffect_WellFed_A;

	public Color StatusEffect_WellFed_B;

	public Color StatusEffect_Frenzy_A;

	public Color StatusEffect_Frenzy_B;

	public Color StatusEffect_Invulnerable_A;

	public Color StatusEffect_Invulnerable_B;

	public Color StatusEffect_Bleeding_A;

	public Color StatusEffect_Bleeding_B;

	public Color StatusEffect_Demand_A;

	public Color StatusEffect_Demand_B;

	public Color StatusEffect_Sick_A;

	public Color StatusEffect_Sick_B;

	public Color StatusEffect_Anxious_A;

	public Color StatusEffect_Anxious_B;

	public Color StatusEffect_No_Energy_A;

	public Color StatusEffect_No_Energy_B;

	public Color StatusEffect_No_Workers_A;

	public Color StatusEffect_No_Workers_B;

	public Color StatusEffect_Homeless_A;

	public Color StatusEffect_Homeless_B;

	public Color StatusEffect_Max_On_Board_A;

	public Color StatusEffect_Max_On_Board_B;

	public Color StatusEffect_No_Educated_Workers_A;

	public Color StatusEffect_No_Educated_Workers_B;

	public Color StatusEffect_Dissolving_A;

	public Color StatusEffect_Dissolving_B;

	public Color StatusEffect_Radar_A;

	public Color StatusEffect_Radar_B;

	public Color StatusEffect_Damaged_A;

	public Color StatusEffect_Damaged_B;

	public Color StatusEffect_On_Fire_A;

	public Color StatusEffect_On_Fire_B;

	public Color StatusEffect_Space_A;

	public Color StatusEffect_Space_B;

	public Color StatusEffect_Card_Off_A;

	public Color StatusEffect_Card_Off_B;

	public Color StatusEffect_Depleted_A;

	public Color StatusEffect_Depleted_B;

	public Color StatusEffect_Energy_View_A;

	public Color StatusEffect_Energy_View_B;

	public Color StatusEffect_Drought_A;

	public Color StatusEffect_Drought_B;

	public Color StatusEffect_No_Sewer_A;

	public Color StatusEffect_No_Sewer_B;

	private Dictionary<string, FieldInfo> nameToField = new Dictionary<string, FieldInfo>();

	private List<CardColorRule> cardColorRules = new List<CardColorRule>();
}
