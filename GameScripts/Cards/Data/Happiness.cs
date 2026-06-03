using System;

$attrpublic class Happiness : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Happiness || otherCard is Unhappiness || otherCard is BaseVillager || otherCard.Id == "plank";
	}
}

