using System;
using UnityEngine;

[Obsolete("Legacy currency. Use standardized economy definitions instead.")]
public class RefinedSpiritStone : SpiritStoneData
{
    public RefinedSpiritStone()
    {
        _spiritPower = 5.0f;
        _expValue = 75;
        _devotionValue = 5;
    }
}
