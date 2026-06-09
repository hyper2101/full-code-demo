using System;
using UnityEngine;

public class RitualCardData : CardData
{
	[SerializeField]
	protected int _ritualTier = 1;

	[SerializeField]
	protected int _requiredDevotion = 50;

	[SerializeField]
	protected string _rewardPackId = Cards.godcat_pack_low;

	[HideInInspector]
	public bool IsLockedWhileActive = true;

	public int RitualTier => _ritualTier;
	public int RequiredDevotion => _requiredDevotion;
	public string RewardPackId => _rewardPackId;

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is CatCardData || otherCard.MyCardType == CardType.Humans || otherCard is RitualCardData)
		{
			return false;
		}
		return true;
	}
}

public class RitualTier1 : RitualCardData
{
	public RitualTier1()
	{
		_ritualTier = 1;
		_requiredDevotion = 50;
		_rewardPackId = Cards.godcat_pack_low;
	}
}

public class RitualTier2 : RitualCardData
{
	public RitualTier2()
	{
		_ritualTier = 2;
		_requiredDevotion = 100;
		_rewardPackId = Cards.godcat_pack_mid;
	}
}

public class RitualTier3 : RitualCardData
{
	public RitualTier3()
	{
		_ritualTier = 3;
		_requiredDevotion = 200;
		_rewardPackId = Cards.godcat_pack_high;
	}
}

public class RitualTier4 : RitualCardData
{
	public RitualTier4()
	{
		_ritualTier = 4;
		_requiredDevotion = 350;
	}
}

public class RitualTier5 : RitualCardData
{
	public RitualTier5()
	{
		_ritualTier = 5;
		_requiredDevotion = 520;
	}
}
