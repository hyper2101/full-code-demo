using UnityEngine;

/// <summary>
/// Lớp cơ sở cho các loại Linh Thạch trong hệ thống Tu luyện Mewtations.
/// </summary>
public abstract class SpiritStoneData : CardData
{
    [Header("Spirit Stone Properties")]
    [SerializeField, Tooltip("Sức mạnh linh khí (VD: 1.0, 2.5, 5.0)")]
    protected float _spiritPower = 1.0f;

    [SerializeField, Tooltip("Lượng EXP nhận được khi hấp thu hoàn tất")]
    protected int _expValue = 15;

    [SerializeField, Tooltip("Thời gian cơ bản để hấp thu 1 viên (tính bằng giây)")]
    protected float _stoneAbsorptionTime = 40f;

    [SerializeField, Tooltip("Điểm Lòng Thành khi hiến tế Miệng Thần Mèo")]
    protected int _devotionValue = 1;

    public virtual float SpiritPower => _spiritPower;
    public virtual int ExpValue => _expValue;
    public virtual float StoneAbsorptionTime => _stoneAbsorptionTime;

    public override int DevotionValue => _devotionValue;
    public override int BlasphemyValue => 0;
}
