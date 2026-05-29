using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipable : CardData
{
	public override bool CanBeDragged
	{
		get
		{
			return !(this.MyGameCard.EquipmentHolder != null) || this.MyGameCard.EquipmentHolder.Combatable.Team == Team.Player;
		}
	}

	public virtual void Process(CombatStats stats)
	{
	}

	public override void OnLanguageChange()
	{
		this._equipableInfo = null;
		base.OnLanguageChange();
	}

	public override void StoppedDragging()
	{
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Equipable);
		GameCard parent = this.MyGameCard.Parent;
		foreach (CardData cardData in list)
		{
			((Equipable)cardData).TryEquipOnCard(parent);
		}
		base.StoppedDragging();
	}

	private void TryEquipOnCard(GameCard card)
	{
		if (this.MyGameCard.EquipmentHolder != null)
		{
			this.MyGameCard.EquipmentHolder.Unequip(this);
		}
		if (card != null && card.CardData.HasInventory)
		{
			card.OpenInventory(true);
			card.CardData.EquipItem(this);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Equipable || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Resources;
	}

	private string GetAdvancedEquipableInfo()
	{
		string text = MewtationsLoc.Translate("label_combat_speed");
		string text2 = MewtationsLoc.Translate("label_hit_chance");
		string text3 = MewtationsLoc.Translate("label_damage");
		string text4 = MewtationsLoc.Translate("label_defence");
		string text5 = MewtationsLoc.Translate("label_health");
		string text6 = "";
		if (this.MyStats.MaxHealth != 0)
		{
			text6 = string.Concat(new string[]
			{
				text6,
				text5,
				": ",
				this.NumberToStringWithPlus(this.MyStats.MaxHealth),
				"\n"
			});
		}
		if (this.MyStats.AttackSpeedIncrement != 0)
		{
			text6 = string.Concat(new string[]
			{
				text6,
				text,
				" ",
				this.NumberToStringWithPlus(this.MyStats.AttackSpeedIncrement),
				"\n"
			});
		}
		if (this.MyStats.HitChanceIncrement != 0)
		{
			text6 = string.Concat(new string[]
			{
				text6,
				text2,
				" ",
				this.NumberToStringWithPlus(this.MyStats.HitChanceIncrement),
				"\n"
			});
		}
		if (this.MyStats.AttackDamageIncrement != 0)
		{
			text6 = string.Concat(new string[]
			{
				text6,
				text3,
				" ",
				this.NumberToStringWithPlus(this.MyStats.AttackDamageIncrement),
				"\n"
			});
		}
		if (this.MyStats.DefenceIncrement != 0)
		{
			text6 = string.Concat(new string[]
			{
				text6,
				text4,
				" ",
				this.NumberToStringWithPlus(this.MyStats.DefenceIncrement),
				"\n"
			});
		}
		return text6;
	}

	private string NumberToStringWithPlus(int n)
	{
		if (n > 0)
		{
			return string.Format("+{0}", n);
		}
		if (n < 0)
		{
			return string.Format("{0}", n);
		}
		return "0";
	}

	public string GetEquipableInfo()
	{
		if (!string.IsNullOrEmpty(this._equipableInfo))
		{
			return this._equipableInfo;
		}
		string text = "";
		text += MewtationsLoc.Translate("label_itemlevel", new LocParam[] { LocParam.Create("level", Mathf.RoundToInt(this.MyStats.ItemLevel).ToString()) });
		text += "\\d<size=90%>";
		string text2 = this.MyStats.SummarizeSpecialHits();
		if (text2.Length > 0)
		{
			text = text + text2 + "\n\n";
		}
		text += this.GetEquipableInfoAdvanced();
		this._equipableInfo = text;
		return this._equipableInfo;
	}

	public string GetEquipableInfoAdvanced()
	{
		return this.GetAdvancedEquipableInfo() ?? "";
	}

	public string GetEquipableCombatLevel()
	{
		return "" + MewtationsLoc.Translate("label_itemlevel", new LocParam[] { LocParam.Create("level", Mathf.RoundToInt(this.MyStats.ItemLevel).ToString()) });
	}

	public override void UpdateCard()
	{
		if (this.Level > 0)
		{
			this.nameOverride = string.Format("{0}  (+{1})", MewtationsLoc.Translate(this.NameTerm), this.Level);
		}
		this.descriptionOverride = MewtationsLoc.Translate(this.DescriptionTerm) + "\n\n<i>" + this.GetEquipableInfo() + "</i>";
		this.MyGameCard.SpecialIcon.sprite = this.GetIconForEquipableType(this.EquipableType);
		this.MyGameCard.ShowSpecialIcon = true;
		base.UpdateCard();
	}

	private Sprite GetIconForEquipableType(EquipableType type)
	{
		if (type == EquipableType.Head)
		{
			return SpriteManager.instance.HeadIconFilled;
		}
		if (type == EquipableType.Weapon)
		{
			return SpriteManager.instance.HandIconFilled;
		}
		if (type == EquipableType.Torso)
		{
			return SpriteManager.instance.TorsoIconFilled;
		}
		return null;
	}

	public string VillagerTypeOverride;

	[Header("Equipment")]
	public EquipableType EquipableType;

	public List<AudioClip> AttackSounds;

	public AttackType AttackType;

	public Blueprint blueprint;

	public CombatStats MyStats;

	[ExtraData("level")]
	public int Level;

	[Header("Mewtations Tactical Weapon Properties")]
	public Mewtations.Combat.WeaponAttackPattern MewtationsAttackPattern = Mewtations.Combat.WeaponAttackPattern.Single;
	public float OutputEfficiency = 1.0f;
	public float DamageResistance = 0.0f;
	public Mewtations.Combat.WeaponArchetype WeaponArchetype = Mewtations.Combat.WeaponArchetype.None;
	public List<Mewtations.Combat.WeaponPassiveEffect> PassiveEffects = new List<Mewtations.Combat.WeaponPassiveEffect>();

	[Header("Mewtations Dynamic Cân Bằng Dữ Liệu")]
	public int ShieldOnAttack = 0;
	public int RageOnHit = 0;
	public int MaxRowReach = 1;

	private string _equipableInfo;
}
