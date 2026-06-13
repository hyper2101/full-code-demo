using UnityEngine;

[Obsolete("Legacy currency. Use standardized economy definitions instead.")]
public class BrokenSpiritStone : SpiritStoneData
{
    public BrokenSpiritStone()
    {
        _spiritPower = 1.0f;
        _expValue = 15;
        _devotionValue = 1;
    }
}
