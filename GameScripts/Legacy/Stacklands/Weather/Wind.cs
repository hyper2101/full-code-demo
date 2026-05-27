using Mewtations.Core;
using System;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedMechanic)]
    public class Wind : Weather
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker || otherCard.Id == "metal_scraps" || otherCard.Id == "factory_parts" || otherCard.Id == "wind";
	}
}

