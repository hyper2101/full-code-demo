using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CatRole { DPS, Tank, ShieldSupport, RageSupport, Debuff, Disruption, Attrition }
public enum CatElement { None, Fire, Poison, Ice, Lightning }

public class CatCardData : Combatable
{
    [Header("Cat Specifics")]
    [ExtraData("cat_role")]
    public CatRole Role;

    [ExtraData("cat_element")]
    public CatElement Element;

    [ExtraData("cat_constitution")]
    public Mewtations.Combat.CatConstitution Constitution = Mewtations.Combat.CatConstitution.None;

    [ExtraData("dao_specialization")]
    public Mewtations.Cards.Cats.DaoSpecialization Specialization = Mewtations.Cards.Cats.DaoSpecialization.None;

    [ExtraData("permanent_scars")]
    public string PermanentScarsString = "";

    public List<string> PermanentScars
    {
        get
        {
            return string.IsNullOrEmpty(PermanentScarsString) 
                ? new List<string>() 
                : new List<string>(PermanentScarsString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public void AddScar(string scarId)
    {
        var list = PermanentScars;
        if (!list.Contains(scarId))
        {
            list.Add(scarId);
            PermanentScarsString = string.Join(",", list);

            // Spawns lore-friendly/existential narrative floating text
            if (this.MyGameCard != null)
            {
                string narrativeMsg = "";
                switch (scarId)
                {
                    case Mewtations.Combat.PermanentScar.CrippledMeridians:
                        narrativeMsg = "Mèo đen dẫm phải thiên đinh!";
                        break;
                    case Mewtations.Combat.PermanentScar.BloodDepletion:
                        narrativeMsg = "Huyết mạch kiệt quệ, khí sắc hao tổn...";
                        break;
                    case Mewtations.Combat.PermanentScar.SoulScar:
                        narrativeMsg = "Để lại một vết cắn khó nhìn...";
                        break;
                    case Mewtations.Combat.PermanentScar.BrokenClaws:
                        narrativeMsg = "Phế trảo hao mòn, lực bất tòng tâm...";
                        break;
                    case Mewtations.Combat.PermanentScar.CursedMeridians:
                        narrativeMsg = "Kinh mạch phế tắc bởi cấm ấn tàn bạo!";
                        break;
                    case Mewtations.Combat.PermanentScar.BrokenFireVein:
                        narrativeMsg = "Tai trái cháy xém vì lôi kiếp...";
                        break;
                    case Mewtations.Combat.PermanentScar.HeartDemonPossessed:
                        narrativeMsg = "Tâm ma quấy rối, hư ảnh mịt mù...";
                        break;
                    case Mewtations.Combat.PermanentScar.ShatteredSoul:
                        narrativeMsg = "Hồn phách lay lắt, linh căn tổn hao...";
                        break;
                    default:
                        narrativeMsg = "Một vết sẹo linh mạch vĩnh viễn...";
                        break;
                }
                WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, narrativeMsg, "", false, 0, 2f, true);
            }
        }
    }

    public void RemoveScar(string scarId)
    {
        var list = PermanentScars;
        if (list.Remove(scarId))
        {
            PermanentScarsString = string.Join(",", list);
        }
    }

    public bool HasScar(string id)
    {
        return PermanentScars.Contains(id);
    }

    private bool _statsDirty = true;
    private CombatStats _cachedCombatStats;
    private ModifierPipeline _modifierPipeline;

    private void Awake()
    {
        _modifierPipeline = new ModifierPipeline(OnModifiersChanged);
        _statusEffectPipeline = new StatusEffectPipeline(this);
    }

    private void OnModifiersChanged()
    {
        // Tối ưu hóa: Chỉ publish event khi chỉ số thực tế sau tính toán thực sự thay đổi
        int oldMaxHp = _cachedCombatStats != null ? _cachedCombatStats.MaxHealth : -1;
        int oldAtk = _cachedCombatStats != null ? _cachedCombatStats.AttackDamage : -1;

        _statsDirty = true;
        CombatStats newStats = ProcessedCombatStats;

        if (newStats.MaxHealth != oldMaxHp || newStats.AttackDamage != oldAtk)
        {
            // Báo hiệu thay đổi chỉ số qua Event Bus
            EventBus.Publish(new OnStatsChangedEvent(this));
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnStatsChangedEvent>(OnStatsChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnStatsChangedEvent>(OnStatsChanged);
    }

    private void OnStatsChanged(OnStatsChangedEvent ev)
    {
        if (ev.Cat == this)
        {
            if (this.MyGameCard != null)
            {
                this.MyGameCard.FlagUiTextDirty();
            }
            else
            {
                this.UpdateCardText();
            }
        }
    }

    public void AddModifier(StatModifier mod)
    {
        _modifierPipeline?.AddModifier(mod);
    }

    public void RemoveModifier(string id)
    {
        _modifierPipeline?.RemoveModifier(id);
    }

    public override CombatStats ProcessedCombatStats
    {
        get
        {
            if (_statsDirty || _cachedCombatStats == null)
            {
                _statsDirty = false;
                CombatStats stats = base.ProcessedCombatStats;

                // Áp dụng Modifier Pipeline
                if (_modifierPipeline != null)
                {
                    stats.MaxHealth = Mathf.RoundToInt(_modifierPipeline.CalculateValue(TargetStat.MaxHealth, stats.MaxHealth));
                    stats.AttackDamage = Mathf.RoundToInt(_modifierPipeline.CalculateValue(TargetStat.AttackDamage, stats.AttackDamage));
                }

                if (Constitution == Mewtations.Combat.CatConstitution.BaoLinhThienKieu)
                {
                    stats.MaxHealth = Mathf.Min(35, stats.MaxHealth);
                }
                if (HasScar(Mewtations.Combat.PermanentScar.BloodDepletion))
                {
                    stats.MaxHealth = Mathf.Max(1, stats.MaxHealth - 15);
                }

                // 2 Thiên phú mới và Sẹo BrokenClaws
                if (HasTrait(Mewtations.Expedition.HeavenlyTalent.DualWield))
                {
                    stats.AttackDamage = Mathf.RoundToInt(stats.AttackDamage * 0.85f); // giảm 15% ATK
                }
                if (HasTrait(Mewtations.Expedition.HeavenlyTalent.FoodGlutton))
                {
                    stats.AttackDamage = Mathf.RoundToInt(stats.AttackDamage * 0.90f); // giảm 10% ATK
                }
                if (HasScar(Mewtations.Combat.PermanentScar.BrokenClaws))
                {
                    stats.AttackDamage = Mathf.Max(1, stats.AttackDamage - 5); // giảm 5 ATK
                }
                if (HasTrait("talent_true_harmony"))
                {
                    stats.MaxHealth = Mathf.RoundToInt(stats.MaxHealth * 1.30f);
                }

                _cachedCombatStats = stats;
            }
            return _cachedCombatStats;
        }
    }

    public override void OnInitialCreate()
    {
        base.OnInitialCreate();
        if (Constitution == Mewtations.Combat.CatConstitution.None)
        {
            if (UnityEngine.Random.value <= 0.60f)
            {
                Array values = Enum.GetValues(typeof(Mewtations.Combat.CatConstitution));
                Constitution = (Mewtations.Combat.CatConstitution)values.GetValue(UnityEngine.Random.Range(1, values.Length));
            }
        }
    }

    [Header("Breakthrough System")]
    [ExtraData("breakthrough_level")]
    public int BreakthroughLevel = 0;

    [ExtraData("has_pill_slot")]
    public bool HasPillSlot = false;
    
    [ExtraData("has_food_slot")]
    public bool HasFoodSlot = false;

    [ExtraData("has_passive1_slot")]
    public bool HasPassive1Slot = false;

    [ExtraData("has_passive2_slot")]
    public bool HasPassive2Slot = false;

    [Header("Level & Exp Tu Vi")]
    [ExtraData("cat_level")]
    public int Level = 1;

    [ExtraData("cat_exp")]
    public int Experience = 0;

    public int MaxExperience => Level * 50;

    [Header("Inspector Stat Gains")]
    public int StatHpGainPerLevel = 1;
    public int StatAtkGainPerLevel = 1;
    public int StatSpeedGainPerLevel = 1;

    public int StatHpGainOnBreakthrough = 10;
    public int StatAtkGainOnBreakthrough = 10;
    public int StatSpeedGainOnBreakthrough = 10;

    public void GainExperience(int amount)
    {
        if (HasScar(Mewtations.Combat.PermanentScar.ShatteredSoul))
        {
            return; // Khóa nhận Exp thường!
        }
        if (Level <= 0) Level = 1;

        // Cấp độ chẵn 9, 19, 29... chuẩn bị lên 10, 20, 30... thì cần đột phá lôi kiếp mới vượt qua được!
        bool isAtCap = (Level + 1) % 10 == 0;

        if (isAtCap && Experience >= MaxExperience - 1)
        {
            Experience = MaxExperience - 1;
            return;
        }

        Experience += amount;

        while (Experience >= MaxExperience)
        {
            if (isAtCap)
            {
                Experience = MaxExperience - 1;

                string alertTitle = "⚠️ CỔ CHAI TU VI / NGHẼN MẠCH!";
                string alertText = $"Thần Miêu <b>{Name}</b> đã tu luyện đạt đến đỉnh phong của cảnh giới hiện tại (Cấp {Level})!\n\n" +
                                   $"Linh lực cuồng bạo đang bị tắc nghẽn. <b>{Name}</b> bắt buộc phải vào <b>Đột Phá Trận</b> vượt qua Lôi Kiếp để thăng tiến lên cảnh giới mới, không thể tiếp tục tích lũy kinh nghiệm!";

                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(alertTitle, alertText, new List<string> { "Đã hiểu!" }, (cIdx) => {});
                }
                break;
            }

            Experience -= MaxExperience;
            Level++;

            // Tăng chỉ số thường
            this.BaseCombatStats.MaxHealth += StatHpGainPerLevel;
            this.HealthPoints += StatHpGainPerLevel;
            this.BaseCombatStats.AttackDamage += StatAtkGainPerLevel;
            this.Speed += StatSpeedGainPerLevel;

            AddMemoir(Mewtations.Expedition.MemoirType.Breakthrough, $"Thăng tu vi", $"Đột phá tu vi đạt Cấp {Level} (+{StatHpGainPerLevel} HP, +{StatAtkGainPerLevel} ATK, +{StatSpeedGainPerLevel} Speed)");

            isAtCap = (Level + 1) % 10 == 0;
        }
    }

    [Header("Turn-Based Combat Stats")]
    [ExtraData("current_rage")]
    public int CurrentRage = 0;

    [ExtraData("stamina")]
    public int Stamina = 100;

    [ExtraData("max_stamina")]
    public int MaxStamina = 100;

    [ExtraData("is_exhausted")]
    public bool IsExhausted = false;

    [ExtraData("hoi_quang_triggered")]
    public bool HoiQuangPhanChieuTriggered = false;

    [ExtraData("exhaustion_level")]
    public int ExhaustionLevel = 0;

    [ExtraData("speed_stat")]
    private int _speedField = 100;

    public int Speed
    {
        get
        {
            int baseSpeed = _speedField;
            if (HasTrait("talent_true_harmony"))
            {
                baseSpeed = Mathf.RoundToInt(baseSpeed * 1.30f);
            }
            return baseSpeed;
        }
        set
        {
            _speedField = value;
        }
    }

    [Header("Traits and Mutations")]
    [ExtraData("lineage_generation")]
    public int LineageGeneration = 1;

    [Header("GDD Punishment States")]
    [ExtraData("is_food_slot_locked")]
    public bool IsFoodSlotLocked = false;

    [ExtraData("is_pill_slot_locked")]
    public bool IsPillSlotLocked = false;

    [ExtraData("is_passive_slots_locked")]
    public bool IsPassiveSlotsLocked = false;

    [ExtraData("is_equipment_slots_locked")]
    public bool IsEquipmentSlotsLocked = false;

    [ExtraData("is_ultimate_locked")]
    public bool IsUltimateLocked = false;

    [ExtraData("character_memoirs")]
    public string CharacterMemoirsString = "";

    public List<Mewtations.Expedition.MemoirEntry> CharacterMemoirs
    {
        get
        {
            return string.IsNullOrEmpty(CharacterMemoirsString)
                ? new List<Mewtations.Expedition.MemoirEntry>()
                : CharacterMemoirsString.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(Mewtations.Expedition.MemoirEntry.Parse)
                    .Where(e => e != null)
                    .ToList();
        }
    }

    public void AddMemoir(Mewtations.Expedition.MemoirType type, string paramA = "", string paramB = "")
    {
        int day = (WorldManager.instance != null) ? WorldManager.instance.CurrentMonth : 1;
        var list = CharacterMemoirs;
        list.Add(new Mewtations.Expedition.MemoirEntry(type, paramA, paramB, day));
        CharacterMemoirsString = string.Join(";", list.Select(m => m.ToString()));
    }

    public void AddMemoir(string milestone)
    {
        AddMemoir(Mewtations.Expedition.MemoirType.Birth, milestone);
    }

    [ExtraData("permanent_traits")]
    public string PermanentTraitsString = "";

    [ExtraData("active_mutations")]
    public string ActiveMutationsString = "";

    public List<string> PermanentTraits
    {
        get
        {
            return string.IsNullOrEmpty(PermanentTraitsString) 
                ? new List<string>() 
                : new List<string>(PermanentTraitsString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public List<string> ActiveMutations
    {
        get
        {
            return string.IsNullOrEmpty(ActiveMutationsString) 
                ? new List<string>() 
                : new List<string>(ActiveMutationsString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public void AddTrait(string traitId)
    {
        var list = PermanentTraits;
        if (list.Count >= 2)
        {
            Debug.LogWarning($"[Song Trọng Dị Biến] Không thể dung hợp thêm thiên phú vĩnh cửu {traitId} cho {Name} vì đã đạt cực hạn (2).");
            return;
        }

        if (!list.Contains(traitId))
        {
            list.Add(traitId);
            PermanentTraitsString = string.Join(",", list);
        }
    }

    public void RemoveTrait(string traitId)
    {
        var list = PermanentTraits;
        if (list.Remove(traitId))
        {
            PermanentTraitsString = string.Join(",", list);
        }
    }

    public void AddMutation(string mutationId)
    {
        var list = ActiveMutations;
        if (!list.Contains(mutationId))
        {
            list.Add(mutationId);
            ActiveMutationsString = string.Join(",", list);
        }
    }

    public void RemoveMutation(string mutationId)
    {
        var list = ActiveMutations;
        if (list.Remove(mutationId))
        {
            ActiveMutationsString = string.Join(",", list);
        }
    }

    public void ClearMutations()
    {
        ActiveMutationsString = "";
    }

    public override void OnEquipItem(Equipable equipable)
    {
        base.OnEquipItem(equipable);
        if (equipable != null && !string.IsNullOrEmpty(equipable.Name))
        {
            AddMemoir(Mewtations.Expedition.MemoirType.Equip, equipable.Name);
        }
    }

    public override void OnUnequipItem(Equipable equipable)
    {
        base.OnUnequipItem(equipable);
        if (equipable != null && !string.IsNullOrEmpty(equipable.Name))
        {
            AddMemoir(Mewtations.Expedition.MemoirType.Unequip, equipable.Name);
        }
    }

    public bool HasTrait(string id)
    {
        return PermanentTraits.Contains(id);
    }

    public bool HasMutation(string id)
    {
        return ActiveMutations.Contains(id);
    }

    protected override string GetTooltipText()
    {
        string baseText = base.GetTooltipText();
        
        // Tooltip Synergy compatibility check
        if (WorldManager.instance.DraggingCard != null && WorldManager.instance.DraggingCard.CardData != null)
        {
            CardData draggingData = WorldManager.instance.DraggingCard.CardData;
            if (draggingData.IsPassiveTalisman)
            {
                int maxTalismans = 2;
                if (this.BreakthroughLevel >= 4) maxTalismans = 2;
                else if (this.BreakthroughLevel == 3) maxTalismans = 1;
                else maxTalismans = 0;

                int currentTalismans = this.GetAllEquipables().Count(x => x != null && x.EquipableType == EquipableType.Talisman);

                if (IsEquipmentSlotsLocked)
                {
                    baseText += "\n<color=red>✗ Kinh mạch bế tắc: Ô trang bị bị khóa!</color>";
                }
                else if (maxTalismans <= 0)
                {
                    baseText += "\n<color=red>✗ Đột phá Luyện Khí chưa đủ để lắp bùa!</color>";
                }
                else if (currentTalismans >= maxTalismans)
                {
                    baseText += "\n<color=yellow>! Ô bùa đầy (Lắp tối đa " + maxTalismans + " bùa, bùa cũ sẽ bị tháo ra).</color>";
                }
                else
                {
                    baseText += "\n<color=green>✓ Tương thích bùa hộ mệnh!</color>";
                }
            }
        }
        if (IsExhausted)
        {
            baseText += $"\n<color=red>⚠️ [KIỆT SỨC - Cấp {ExhaustionLevel}] Giảm mạnh chỉ số, cần được tĩnh dưỡng dưỡng thương!</color>";
        }

        if (HoiQuangPhanChieuTriggered && Constitution == Mewtations.Combat.CatConstitution.BaoLinhThienKieu)
        {
            baseText += "\n<color=#ff9900>⚠️ [HAOTHỂN TIỀM NĂNG - Đã Bộc Phát] Đã đốt thiên phú Hồi Quang Phản Chiếu, cần dùng Tiên dược khôi phục cốt tủy!</color>";
        }

        return baseText;
    }

    public override void UpdateCard()
    {
        base.UpdateCard();

        // 4.3: Tooltip Synergy (Talisman hover highlight)
        if (WorldManager.instance.DraggingCard != null && WorldManager.instance.DraggingCard.CardData != null && WorldManager.instance.DraggingCard != this.MyGameCard)
        {
            CardData draggingData = WorldManager.instance.DraggingCard.CardData;
            if (draggingData.IsPassiveTalisman)
            {
                int maxTalismans = 2;
                if (this.BreakthroughLevel >= 4) maxTalismans = 2;
                else if (this.BreakthroughLevel == 3) maxTalismans = 1;
                else maxTalismans = 0;

                int currentTalismans = this.GetAllEquipables().Count(x => x != null && x.EquipableType == EquipableType.Talisman);

                if (!IsEquipmentSlotsLocked && maxTalismans > 0 && currentTalismans < maxTalismans)
                {
                    this.MyGameCard.HighlightActive = true;
                }
            }
        }

        // Đột phá bây giờ chỉ diễn ra thông qua Đột Phá Trận (BreakthroughArrayCardData)
        // Loại bỏ hoàn toàn trigger cũ khi đặt Linh đan trực tiếp lên Mèo.

        // Kiểm tra giải sẹo CursedMeridians bằng item_healing_potion
        if (HasScar(Mewtations.Combat.PermanentScar.CursedMeridians))
        {
            if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.IsHealingPotion)
            {
                if (!this.MyGameCard.TimerRunning)
                {
                    this.MyGameCard.StartTimer(3f, new TimerAction(ResolveCursedMeridians), "Đang khơi thông kinh mạch...", "resolve_cursed_meridians");
                }
            }
            else
            {
                if (this.MyGameCard != null && this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "resolve_cursed_meridians")
                {
                    this.MyGameCard.CancelTimer("resolve_cursed_meridians");
                }
            }
        }

        // Tĩnh dưỡng dưỡng thương / Hồi phục Stamina & HP trên Mainland
        if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.IsRecoveryItem)
        {
            if (!this.MyGameCard.TimerRunning)
            {
                float duration = 3f * this.MyGameCard.Child.CardData.RecoveryDurationModifier;
                this.MyGameCard.StartTimer(duration, new TimerAction(ConsumeRecoveryItem), "Đang tĩnh dưỡng hồi phục...", "consume_recovery_item");
            }
        }
        else
        {
            if (this.MyGameCard != null && this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "consume_recovery_item")
            {
                this.MyGameCard.CancelTimer("consume_recovery_item");
                WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, "⚠️ [TĨNH DƯỠNG BỊ GIÁN ĐOẠN] Di chuyển thẻ làm gián đoạn phục hồi!", "", false, 0, 2f, true);
            }
        }
    }

    public void ConsumeRecoveryItem()
    {
        if (this.MyGameCard != null && this.MyGameCard.HasChild)
        {
            GameCard itemCard = this.MyGameCard.Child;
            CardData itemData = itemCard.CardData;

            int hpRestore = itemData.HpRecoveryAmount;
            int staminaRestore = itemData.StaminaRecoveryAmount;
            bool cleanExhaust = itemData.CleansesExhaustion;
            bool resetHoiQuang = itemData.ResetsHoiQuang;

            this.HealthPoints = Mathf.Min(this.HealthPoints + hpRestore, this.ProcessedCombatStats.MaxHealth);
            this.Stamina = Mathf.Min(this.Stamina + staminaRestore, this.MaxStamina);

            if (cleanExhaust || this.Stamina >= 50)
            {
                this.IsExhausted = false;
                this.ExhaustionLevel = 0;
            }
            else
            {
                this.ExhaustionLevel = Mathf.Max(0, this.ExhaustionLevel - 2);
            }

            if (resetHoiQuang)
            {
                this.HoiQuangPhanChieuTriggered = false;
            }

            itemCard.DestroyCard(true, true); // Consume recovery item

            string msg = $"🍲 {Name} đã phục hồi! +{hpRestore} HP, +{staminaRestore} Thể Lực.";
            if (IsExhausted == false && cleanExhaust) msg += " Hết kiệt sức!";
            if (resetHoiQuang) msg += " Khôi phục Hồi Quang Phản Chiếu!";

            WorldManager.instance.CreateFloatingText(this.MyGameCard, true, hpRestore, msg, "", true, 0, 3f, true);
        }
        UpdateCardText();
    }

    public void ResolveCursedMeridians()
    {
        if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.IsHealingPotion)
        {
            GameCard potion = this.MyGameCard.Child;
            potion.DestroyCard(true, true); // Tiêu thụ thuốc đỏ
        }

        RemoveScar(Mewtations.Combat.PermanentScar.CursedMeridians);
        IsUltimateLocked = false;

        string title = "☯️ KINH MẠCH PHỤC HỒI!";
        string text = $"Dược lực từ bình thuốc đỏ đã len lỏi vào linh mạch, tẩy rửa hoàn toàn cấm chế phế ấn cho <b>{Name}</b>!\n\n" +
                      $"Linh mạch đã được đả thông hoàn toàn, khôi phục khả năng thi triển **Ultimate Skill**!";

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Quá tốt rồi!" }, (idx) => {});
        }

        UpdateCardText();
    }

    // PerformHealingChamber removed per user request

    public void PerformBreakthroughInArray(int targetLevel, float damageReductionPercent, int healthBonus, bool hasRevivePill)
    {
        string cảnhGiới = "";
        switch (targetLevel)
        {
            case 1: cảnhGiới = "Luyện Khí Cảnh (Mở ô Linh Đan)"; break;
            case 2: cảnhGiới = "Trúc Cơ Cảnh (Mở ô Thức Ăn - Ultimate Skill)"; break;
            case 3: cảnhGiới = "Kim Đan Cảnh (Mở ô Thiên Phú 1)"; break;
            case 4: cảnhGiới = "Nguyên Anh Cảnh (Mở ô Thiên Phú 2)"; break;
            default: cảnhGiới = $"Hóa Thần Cảnh Tầng {targetLevel - 4} (Tăng mạnh Sinh mệnh & Thần tốc)"; break;
        }

        int strikesCount = 1;
        int damagePerStrike = 5;

        if (targetLevel == 1) { strikesCount = 1; damagePerStrike = 5; }
        else if (targetLevel == 2) { strikesCount = 2; damagePerStrike = 10; }
        else if (targetLevel == 3) { strikesCount = 3; damagePerStrike = 15; }
        else if (targetLevel == 4) { strikesCount = 4; damagePerStrike = 20; }
        else { strikesCount = 5; damagePerStrike = 25; }

        // Áp dụng tăng sinh mệnh bổ trợ từ dược phẩm trận pháp
        if (healthBonus > 0)
        {
            this.HealthPoints = Mathf.Min(this.HealthPoints + healthBonus, this.ProcessedCombatStats.MaxHealth);
        }

        // Áp dụng giảm sát thương lôi kiếp từ bùa hộ mệnh trận pháp
        if (damageReductionPercent > 0f)
        {
            damagePerStrike = Mathf.Max(1, Mathf.RoundToInt(damagePerStrike * (1f - damageReductionPercent)));
        }

        StartHeavenlyTribulationInArray(targetLevel, cảnhGiới, strikesCount, damagePerStrike, 1, hasRevivePill);
    }

    private void StartHeavenlyTribulationInArray(int targetLevel, string cảnhGiới, int totalStrikes, int damagePerStrike, int currentStrike, bool hasRevivePill)
    {
        if (currentStrike > totalStrikes)
        {
            ExecuteBreakthroughSuccess(targetLevel, cảnhGiới);
            return;
        }

        string simpleCảnhGiới = cảnhGiới.Split(' ')[0];
        string title = $"⚡ THIÊN KIẾP ĐỘT PHÁ: TIA THỨ {currentStrike}/{totalStrikes}";
        string text = $"Trận pháp đột phá kích hoạt! Thiên lôi giáng xuống đầu <b>{Name}</b> để rèn luyện thăng tiến sức mạnh lên <b><color=#ffcc00>{simpleCảnhGiới}</color></b>!\n\n" +
                      $"• Sát thương thiên lôi: <b><color=red>{damagePerStrike} HP</color></b>\n" +
                      $"• Sinh mệnh hiện tại: <b>{HealthPoints}/{ProcessedCombatStats.MaxHealth} HP</b>\n\n" +
                      $"Bạn muốn đối phó với đợt lôi kiếp này thế nào?";

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            $"Dùng nhục thân gánh chịu lôi kiếp (Chịu {damagePerStrike} HP sát thương)",
            () => {
                this.HealthPoints -= damagePerStrike;
                CheckStrikeResultInArray(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike, hasRevivePill);
            }
        ));

        if (!IsEquipmentSlotsLocked)
        {
            int reducedDamage = damagePerStrike / 2;
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                $"Dẫn lôi lực vào Ô Trang Bị (Chịu {reducedDamage} HP sát thương, 20% nguy cơ khóa Ô Trang Bị)",
                () => {
                    this.HealthPoints -= reducedDamage;
                    if (UnityEngine.Random.value <= 0.20f)
                    {
                        IsEquipmentSlotsLocked = true;
                        string lockTitle = "⚠️ Ô TRANG BỊ BỊ TỔN HẠI!";
                        string lockText = $"Sét lôi kiếp cực mạnh ép vỡ phòng ngự bảo hộ trang bị của <b>{Name}</b>! Ô trang bị đã bị khóa vĩnh viễn.";
                        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                        {
                            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(lockTitle, lockText, new List<string> { "Đành chịu" }, (cIdx) => {
                                CheckStrikeResultInArray(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike, hasRevivePill);
                            });
                        }
                    }
                    else
                    {
                        CheckStrikeResultInArray(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike, hasRevivePill);
                    }
                }
            ));
        }

        // Hỗ trợ từ đồng đội Tank
        List<CatCardData> otherTanks = new List<CatCardData>();
        if (WorldManager.instance != null && WorldManager.instance.AllCards != null)
        {
            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && gc.CardData is CatCardData cat && cat != this && cat.Role == CatRole.Tank)
                {
                    otherTanks.Add(cat);
                }
            }
        }

        if (otherTanks.Count > 0)
        {
            CatCardData tankCat = otherTanks[0];
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                $"Đồng đội {tankCat.Name} (Tank) chắn sét hộ (Tank nhận {damagePerStrike} HP, nhận +10 Max HP vĩnh viễn)",
                () => {
                    tankCat.HealthPoints = Mathf.Max(1, tankCat.HealthPoints - damagePerStrike);
                    tankCat.BaseCombatStats.MaxHealth += 10;
                    tankCat.HealthPoints = Mathf.Min(tankCat.HealthPoints + 10, tankCat.ProcessedCombatStats.MaxHealth);
                    tankCat.AddMemoir($"Đỡ lôi kiếp hộ {Name} tại Đột Phá Trận!");

                    string tankTitle = "🛡️ ĐỒNG ĐỘI HỘ PHÁP!";
                    string tankText = $"Hộ pháp nghĩa khí <b>{tankCat.Name}</b> lao ra Đột Phá Trận để gánh lôi kiếp hộ <b>{Name}</b>!\n\n" +
                                      $"• Sát thương chuyển sang <b>{tankCat.Name}</b>.\n" +
                                      $"• Nhờ tôi luyện sét lôi kiếp, <b>{tankCat.Name}</b> nhận thêm: <b>+10 Max HP</b> vĩnh viễn!";

                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(tankTitle, tankText, new List<string> { "Đáng kính!" }, (cIdx) => {
                            CheckStrikeResultInArray(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike, hasRevivePill);
                        });
                    }
                }
            ));
        }

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }

    private void CheckStrikeResultInArray(int targetLevel, string cảnhGiới, int totalStrikes, int damagePerStrike, int currentStrike, bool hasRevivePill)
    {
        if (this.HealthPoints <= 0)
        {
            if (hasRevivePill)
            {
                this.HealthPoints = this.ProcessedCombatStats.MaxHealth / 2;
                string reviveTitle = "☯️ DƯỢC LỰC HỒI SINH KÍCH HOẠT!";
                string reviveText = $"Nhục thân của <b>{Name}</b> đã tan vỡ dưới sét lôi kiếp tàn bạo!\n\n" +
                                    $"Nhưng <b>Linh Đan Hồi Sinh</b> được chuẩn bị sẵn trong trận pháp đã bùng nổ sinh cơ vô tận, tái tạo cốt nhục cho <b>{Name}</b>!\n" +
                                    $"• <b>{Name}</b> hồi sinh khỏe mạnh với 50% HP (<b>{HealthPoints} HP</b>).\n" +
                                    $"• Đột phá thành công vượt qua lôi kiếp mà không nhận bất kỳ Vết sẹo phế bỏ nào!";

                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(reviveTitle, reviveText, new List<string> { "Quá diệu kỳ!" }, (cIdx) => {
                        ExecuteBreakthroughSuccess(targetLevel, cảnhGiới);
                    });
                }
            }
            else
            {
                this.HealthPoints = 1;
                string[] possibleFailScars = {
                    Mewtations.Combat.PermanentScar.CrippledMeridians,
                    Mewtations.Combat.PermanentScar.BloodDepletion
                };
                string rolledScar = possibleFailScars[UnityEngine.Random.Range(0, possibleFailScars.Length)];
                AddScar(rolledScar);

                AddMemoir($"Đột phá lên {cảnhGiới.Split(' ')[0]} thất bại tại Đột Phá Trận!");

                string failTitle = "⚡ ĐỘT PHÁ THẤT BẠI!";
                string failText = $"Thần lôi đập nát phòng ngự của <b>{Name}</b>! Chú mèo đã thất bại đột phá.\n\n" +
                                   $"• Nhận vết sẹo kinh mạch vĩnh viễn: <b><color=red>{Mewtations.Combat.PermanentScar.GetDisplayName(rolledScar)}</color></b>\n" +
                                   $"• <i>({Mewtations.Combat.PermanentScar.GetDescription(rolledScar)})</i>\n\n" +
                                   $"Chú mèo được bảo hộ bởi Đột Phá Trận cứu mạng tại 1 HP.";

                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(failTitle, failText, new List<string> { "Chấp nhận số phận" }, (choiceIdx) => {});
                }
            }
        }
        else
        {
            StartHeavenlyTribulationInArray(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike + 1, hasRevivePill);
        }
    }

    public void PerformBreakthrough()
    {
        if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == "item_breakthrough_pill")
        {
            GameCard pill = this.MyGameCard.Child;
            pill.DestroyCard(true, true);
        }

        int targetLevel = BreakthroughLevel + 1;
        string cảnhGiới = "";
        
        switch (targetLevel)
        {
            case 1: cảnhGiới = "Luyện Khí Cảnh (Mở ô Linh Đan)"; break;
            case 2: cảnhGiới = "Trúc Cơ Cảnh (Mở ô Thức Ăn - Ultimate Skill)"; break;
            case 3: cảnhGiới = "Kim Đan Cảnh (Mở ô Thiên Phú 1)"; break;
            case 4: cảnhGiới = "Nguyên Anh Cảnh (Mở ô Thiên Phú 2)"; break;
            default: cảnhGiới = $"Hóa Thần Cảnh Tầng {targetLevel - 4} (Tăng mạnh Sinh mệnh & Thần tốc)"; break;
        }

        // Tính toán số lượng lôi kiếp và sát thương dựa vào cấp độ đột phá mục tiêu
        int strikesCount = 1;
        int damagePerStrike = 5;

        if (targetLevel == 1)
        {
            strikesCount = 1;
            damagePerStrike = 5;
        }
        else if (targetLevel == 2)
        {
            strikesCount = 2;
            damagePerStrike = 10;
        }
        else if (targetLevel == 3)
        {
            strikesCount = 3;
            damagePerStrike = 15;
        }
        else if (targetLevel == 4)
        {
            strikesCount = 4;
            damagePerStrike = 20;
        }
        else // targetLevel >= 5
        {
            strikesCount = 5;
            damagePerStrike = 25;
        }

        StartHeavenlyTribulation(targetLevel, cảnhGiới, strikesCount, damagePerStrike, 1);
    }

    private void StartHeavenlyTribulation(int targetLevel, string cảnhGiới, int totalStrikes, int damagePerStrike, int currentStrike)
    {
        if (currentStrike > totalStrikes)
        {
            // Vượt qua toàn bộ lôi kiếp thành công!
            ExecuteBreakthroughSuccess(targetLevel, cảnhGiới);
            return;
        }

        string simpleCảnhGiới = cảnhGiới.Split(' ')[0];
        string title = $"⚡ THIÊN KIẾP: TIA THỨ {currentStrike}/{totalStrikes}";
        string text = $"Một luồng sét lôi kiếp cuồng bạo từ thiên địa đang giáng xuống đầu <b>{Name}</b> khi đột phá lên <b><color=#ffcc00>{simpleCảnhGiới}</color></b>!\n\n" +
                      $"• Sát thương lôi kiếp: <b><color=red>{damagePerStrike} HP</color></b>\n" +
                      $"• Sinh mệnh hiện tại: <b>{HealthPoints}/{ProcessedCombatStats.MaxHealth} HP</b>\n\n" +
                      $"Bạn muốn đối phó với đợt lôi kiếp này thế nào?";

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        // Lựa chọn 1: Gương thân gánh chịu (Flesh Tanking)
        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            $"Gương thân gánh chịu (Chịu {damagePerStrike} HP sát thương)",
            () => {
                this.HealthPoints -= damagePerStrike;
                CheckStrikeResult(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike);
            }
        ));

        // Lựa chọn 2: Dịch chuyển linh lực (Talisman Ward) - Chỉ khả dụng khi không bị khóa ô trang bị
        if (!IsEquipmentSlotsLocked)
        {
            int reducedDamage = damagePerStrike / 2;
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                $"Dịch chuyển linh lực vào bùa (Chịu {reducedDamage} HP sát thương, 20% nguy cơ khóa Ô Trang Bị)",
                () => {
                    this.HealthPoints -= reducedDamage;
                    
                    if (UnityEngine.Random.value <= 0.20f)
                    {
                        IsEquipmentSlotsLocked = true;
                        string lockTitle = "⚠️ BÙA HỘ MỆNH NỨT VỠ!";
                        string lockText = $"Dòng linh lực cuồng bạo từ lôi kiếp đã ép vỡ hoàn toàn hộ thân bùa của <b>{Name}</b>! Ô trang bị đã bị khóa vĩnh viễn.";
                        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                        {
                            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(lockTitle, lockText, new List<string> { "Chấp nhận" }, (cIdx) => {
                                CheckStrikeResult(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike);
                            });
                        }
                    }
                    else
                    {
                        CheckStrikeResult(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike);
                    }
                }
            ));
        }

        // Lựa chọn 3: Đồng đội Tank đỡ (Tank Block)
        List<CatCardData> otherTanks = new List<CatCardData>();
        if (WorldManager.instance != null && WorldManager.instance.AllCards != null)
        {
            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && gc.CardData is CatCardData cat && cat != this && cat.Role == CatRole.Tank)
                {
                    otherTanks.Add(cat);
                }
            }
        }

        if (otherTanks.Count > 0)
        {
            CatCardData tankCat = otherTanks[0];
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                $"Đồng đội {tankCat.Name} (Tank) đỡ hộ (Tank nhận {damagePerStrike} HP & tăng 10 Max HP vĩnh viễn)",
                () => {
                    tankCat.HealthPoints = Mathf.Max(1, tankCat.HealthPoints - damagePerStrike);
                    tankCat.BaseCombatStats.MaxHealth += 10;
                    tankCat.HealthPoints = Mathf.Min(tankCat.HealthPoints + 10, tankCat.ProcessedCombatStats.MaxHealth);
                    tankCat.AddMemoir($"Đỡ thiên kiếp hộ {Name}, nhận phúc lành sinh mệnh!");

                    string tankTitle = "🛡️ ĐỒNG ĐỘI NGHĨA KHÍ!";
                    string tankText = $"Chú mèo Tank <b>{tankCat.Name}</b> đã hiên ngang lao ra chắn trước lôi kiếp cho <b>{Name}</b>!\n\n" +
                                      $"• Sát thương {damagePerStrike} HP chuyển sang {tankCat.Name}.\n" +
                                      $"• <b>{tankCat.Name}</b> rèn luyện xương cốt qua lôi kiếp, nhận Phúc Lành Sinh Mệnh: <b>+10 Max HP</b> vĩnh viễn!";

                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(tankTitle, tankText, new List<string> { "Thật đáng kính!" }, (cIdx) => {
                            CheckStrikeResult(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike);
                        });
                    }
                }
            ));
        }

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }

    private void CheckStrikeResult(int targetLevel, string cảnhGiới, int totalStrikes, int damagePerStrike, int currentStrike)
    {
        if (this.HealthPoints <= 0)
        {
            this.HealthPoints = 1;
            
            string[] possibleFailScars = {
                Mewtations.Combat.PermanentScar.CrippledMeridians,
                Mewtations.Combat.PermanentScar.BloodDepletion
            };
            string rolledScar = possibleFailScars[UnityEngine.Random.Range(0, possibleFailScars.Length)];
            AddScar(rolledScar);
            
            AddMemoir($"Đột phá lên {cảnhGiới.Split(' ')[0]} thất bại do bị Thiên Kiếp đánh bại!");
            
            string failTitle = "⚡ THIÊN KIẾP THẤT BẠI!";
            string failText = $"Thần lôi cuồng bạo đánh nát phòng tuyến của <b>{Name}</b>! Chú mèo đã không chịu nổi lôi kiếp dữ dội và đột phá thất bại.\n\n" +
                               $"• Kinh mạch trọng thương, để lại vết sẹo vĩnh viễn: <b><color=red>{Mewtations.Combat.PermanentScar.GetDisplayName(rolledScar)}</color></b>\n" +
                               $"• <i>({Mewtations.Combat.PermanentScar.GetDescription(rolledScar)})</i>\n\n" +
                               $"Linh đan đột phá đã tan vỡ hoàn toàn. Chú mèo được cứu sống với 1 HP.";
            
            if (Mewtations.Dialogue.DialogueSystem.Instance != null)
            {
                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(failTitle, failText, new List<string> { "Đành chấp nhận..." }, (choiceIdx) => {});
            }
        }
        else
        {
            StartHeavenlyTribulation(targetLevel, cảnhGiới, totalStrikes, damagePerStrike, currentStrike + 1);
        }
    }

    private void ExecuteBreakthroughSuccess(int targetLevel, string cảnhGiới, string scarId = "")
    {
        BreakthroughLevel = targetLevel;
        switch (BreakthroughLevel)
        {
            case 1: HasPillSlot = true; break;
            case 2: HasFoodSlot = true; break;
            case 3: HasPassive1Slot = true; break;
            case 4: HasPassive2Slot = true; break;
        }

        Level++; // Phá vỡ xiềng xích, thăng cấp lên 10.x thành công!
        Experience = 0;

        string simpleCảnhGiới = cảnhGiới.Split(' ')[0];
        AddMemoir(Mewtations.Expedition.MemoirType.Breakthrough, "Đột phá thành công", "Thăng tiến vượt trội lên " + simpleCảnhGiới);

        // Tăng chỉ số đột phá vượt trội từ các thông số Inspector
        this.BaseCombatStats.MaxHealth += StatHpGainOnBreakthrough;
        this.BaseCombatStats.AttackDamage += StatAtkGainOnBreakthrough;
        this.Speed += StatSpeedGainOnBreakthrough;
        this.HealthPoints = this.ProcessedCombatStats.MaxHealth; // Hồi phục đầy sinh mệnh

        // Show breakthrough dialog
        string title = "ĐỘT PHÁ THÀNH CÔNG!";
        string text = $"Thần Miêu <b>{Name}</b> đã đập vỡ xiềng xích phàm trần, đột phá thành công lên <b><color=#ffcc00>{cảnhGiới}</color></b>!\n\n" +
                      $"• Phá vỡ nghẽn mạch, thăng lên **Cấp {Level}** thành công!\n" +
                      $"• Sinh mệnh tối đa tăng lên: <b>{this.ProcessedCombatStats.MaxHealth} HP</b> (+{StatHpGainOnBreakthrough})\n" +
                      $"• Sức tấn công tăng lên: <b>{this.ProcessedCombatStats.AttackDamage} ATK</b> (+{StatAtkGainOnBreakthrough})\n" +
                      $"• Thần tốc tăng lên: <b>{Speed} Speed</b> (+{StatSpeedGainOnBreakthrough})\n";

        if (!string.IsNullOrEmpty(scarId))
        {
            text += $"\n⚠️ <b>Gánh chịu vết sẹo:</b> Cưỡng ép tu vi đã để lại vết sẹo vĩnh viễn: <b><color=#e67e22>{Mewtations.Combat.PermanentScar.GetDisplayName(scarId)}</color></b>!\n" +
                    $"<i>({Mewtations.Combat.PermanentScar.GetDescription(scarId)})</i>\n";
        }

        text += "\n• Cực hạn võ đạo mới đã được khai mở!";

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Chúc mừng!" }, (choiceIdx) => { 
                if (targetLevel == 3)
                {
                    TriggerSpecializationChoice();
                }
            });
        }
    }

    private void TriggerSpecializationChoice()
    {
        string title = "☯️ THỨC TỈNH ĐẠO QUẢ HOÀN MỸ";
        string text = $"Thần Miêu <b>{Name}</b> đã ngưng kết Kim Đan thành công! \n\n" +
                      $"Linh hồn và kinh mạch đã chín muồi để thức tỉnh **Đạo Quả Hoàn Mỹ (Dao Specialization)** định hình phong cách chiến đấu vĩnh viễn:\n\n" +
                      $"• <b>Kiếm Đạo Đạo Quả:</b> Tăng 50% sát thương đòn đánh cơ bản. Khóa vĩnh viễn ô Linh Đan.\n" +
                      $"• <b>Ma Đạo Đạo Quả:</b> Sát thương tăng mạnh theo độ Corruption viễn chinh. Khóa vĩnh viễn ô Trang Bị.\n" +
                      $"• <b>Pháp Đạo Đạo Quả:</b> Bắt đầu trận với +50 Nộ, hồi 15 Nộ sau mỗi kỹ năng.\n" +
                      $"• <b>Thiền Đạo Đạo Quả:</b> Hồi 15 HP khi máu dưới 30% (mỗi trận 1 lần), miễn dịch kịch độc.";

        var choices = new List<string> {
            "☯️ Kiếm Đạo Đạo Quả (Khóa ô Linh Đan)",
            "🔴 Ma Đạo Đạo Quả (Khóa ô Trang Bị)",
            "⚡ Pháp Đạo Đạo Quả (Hồi Nộ khí)",
            "💚 Thiền Đạo Đạo Quả (Trị liệu & Độc miễn)"
        };

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (chosenIdx) =>
            {
                Specialization = (Mewtations.Cards.Cats.DaoSpecialization)(chosenIdx + 1);
                string specName = Mewtations.Cards.Cats.CultivationSpecializationRegistry.GetSpecializationName(Specialization);
                
                AddMemoir(Mewtations.Expedition.MemoirType.Breakthrough, specName, "Thức tỉnh Đạo Quả Hoàn Mỹ");

                if (Specialization == Mewtations.Cards.Cats.DaoSpecialization.SwordDao)
                {
                    IsPillSlotLocked = true;
                }
                else if (Specialization == Mewtations.Cards.Cats.DaoSpecialization.DemonDao)
                {
                    IsEquipmentSlotsLocked = true;
                }

                string resText = $"☯️ <b>{Name}</b> đã thức tỉnh thành công <b><color=#00ffcc>{specName}</color></b>!\n\n" +
                                 $"<i>{Mewtations.Cards.Cats.CultivationSpecializationRegistry.GetSpecializationDescription(Specialization)}</i>";

                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("ĐẠO QUẢ THÀNH TỰU", resText, new List<string> { "Đại Đạo Hoàn Mỹ!" }, (idx) => {});
            });
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // 1. Check Equipment locks (Weapon / Talismans / Food are equipment types)
        if (IsEquipmentSlotsLocked && otherCard.MyCardType == CardType.Equipment)
        {
            return false;
        }

        // Chữa sẹo phế ấn bằng thuốc đỏ
        if (otherCard.IsHealingPotion && HasScar(Mewtations.Combat.PermanentScar.CursedMeridians))
        {
            return true;
        }

        // 2. Validate food slot (BT level 2)
        if (otherCard.MyCardType == CardType.Food || (otherCard is Equipable eqFood && eqFood.EquipableType == EquipableType.Food)) 
        {
            if (IsFoodSlotLocked) return false;
            if (!HasFoodSlot) return false;
            
            int maxFoods = 1;
            if (HasTrait(Mewtations.Expedition.HeavenlyTalent.FoodGlutton))
            {
                maxFoods = 2;
            }
            int currentFoods = GetAllEquipables().Count(eq => eq.EquipableType == EquipableType.Food);
            return currentFoods < maxFoods;
        }

        // 3. Validate Pill slot (BT level 1)
        if (otherCard.IsCultivationPill)
        {
            // breakthrough pill can be stacked on anyone to trigger breakthrough
            if (otherCard.IsBreakthroughPill)
            {
                return true;
            }
            if (IsPillSlotLocked) return false;
            return HasPillSlot;
        }

        // 4. Validate Passive Slots (BT level 3 & 4: Max 1 for level 3, Max 2 for level 4)
        if (otherCard.IsPassiveTalisman)
        {
            if (IsPassiveSlotsLocked) return false;

            int maxPassives = 0;
            if (BreakthroughLevel >= 4) maxPassives = 2;
            else if (BreakthroughLevel == 3) maxPassives = 1;

            int currentPassives = ChildrenMatchingPredicateCount(c => c.IsPassiveTalisman);
            return currentPassives < maxPassives;
        }

        // 5. Equipment slots (Weapon & Talismans) are allowed by default
        if (otherCard.MyCardType == CardType.Equipment)
        {
            if (otherCard is Equipable eqWeap && eqWeap.EquipableType == EquipableType.Weapon)
            {
                int maxWeapons = 1;
                if (HasTrait(Mewtations.Expedition.HeavenlyTalent.DualWield))
                {
                    maxWeapons = 2;
                }
                int currentWeapons = GetAllEquipables().Count(eq => eq.EquipableType == EquipableType.Weapon);
                return currentWeapons < maxWeapons;
            }
            return true;
        }

        // Default parent validation
        return base.CanHaveCard(otherCard);
    }

    public string GetCảnhGiớiName()
    {
        switch (BreakthroughLevel)
        {
            case 0: return "Phàm Nhân Mèo";
            case 1: return "Luyện Khí Cảnh";
            case 2: return "Trúc Cơ Cảnh";
            case 3: return "Kim Đan Cảnh";
            case 4: return "Nguyên Anh Cảnh";
            default: return $"Hóa Thần Cảnh Tầng {BreakthroughLevel - 4}";
        }
    }

    public override void UpdateCardText()
    {
        string desc = $"<b>CẢNH GIỚI:</b> <color=#ffcc00>{GetCảnhGiớiName()}</color>\n";
        desc += $"<b>VAI TRÒ:</b> <color=#5dade2>{Role}</color> | <b>LINH CĂN:</b> <color=#ff33cc>{Element}</color>\n";
        if (Constitution != Mewtations.Combat.CatConstitution.None)
        {
            desc += $"<b>THỂ CHẤT:</b> <color=#e74c3c>{Mewtations.Combat.MewtationsConstitutionRegistry.GetDisplayName(Constitution)}</color>\n";
            desc += $"<i>({Mewtations.Combat.MewtationsConstitutionRegistry.GetDescription(Constitution)})</i>\n";
        }
        if (Specialization != Mewtations.Cards.Cats.DaoSpecialization.None)
        {
            desc += $"<b>ĐẠO QUẢ:</b> <color=#00ffcc>{Mewtations.Cards.Cats.CultivationSpecializationRegistry.GetSpecializationName(Specialization)}</color>\n";
            desc += $"<i>({Mewtations.Cards.Cats.CultivationSpecializationRegistry.GetSpecializationDescription(Specialization)})</i>\n";
        }
        desc += $"<b>SINH MỆNH:</b> {HealthPoints}/{ProcessedCombatStats.MaxHealth} HP\n";
        desc += $"<b>THẦN TỐC:</b> {Speed} Speed\n\n";

        var traits = PermanentTraits;
        if (traits.Count > 0)
        {
            desc += "<b>★ THIÊN PHÚ VĨNH CỬU:</b>\n";
            foreach (var t in traits)
            {
                desc += $"• <color=#00ffcc>{Mewtations.Expedition.HeavenlyTalent.GetDisplayName(t)}</color>: {Mewtations.Expedition.HeavenlyTalent.GetDescription(t)}\n";
            }
            desc += "\n";
        }

        var mutations = ActiveMutations;
        if (mutations.Count > 0)
        {
            desc += "<b>☣️ DỊ BIẾN TẠM THỜI:</b>\n";
            foreach (var m in mutations)
            {
                desc += $"• <color=#ff3333>{Mewtations.Expedition.UnstableMutation.GetDisplayName(m)}</color>: {Mewtations.Expedition.UnstableMutation.GetDescription(m)}\n";
            }
            desc += "\n";
        }

        var scars = PermanentScars;
        if (scars.Count > 0)
        {
            desc += "<b>☠️ VẾT SẸO VĨNH CỬU:</b>\n";
            foreach (var s in scars)
            {
                desc += $"• <color=#e67e22>{Mewtations.Combat.PermanentScar.GetDisplayName(s)}</color>: {Mewtations.Combat.PermanentScar.GetDescription(s)}\n";
            }
            desc += "\n";
        }

        bool hasPunishment = IsFoodSlotLocked || IsPillSlotLocked || IsPassiveSlotsLocked || IsEquipmentSlotsLocked || IsUltimateLocked;
        if (hasPunishment)
        {
            desc += "<b>☠️ HÌNH PHẠT NGÔN NGỮ / KHÓA:</b>\n";
            if (IsUltimateLocked) desc += "• <color=red>[KHÓA KỸ NĂNG NỘ]</color>: Không thể thi triển Ultimate Skill.\n";
            if (IsFoodSlotLocked) desc += "• <color=red>[KHÓA Ô THỨC ĂN]</color>: Slot Ultimate Skill bị phong ấn.\n";
            if (IsPillSlotLocked) desc += "• <color=red>[KHÓA Ô LINH ĐAN]</color>: Slot Linh Đan bị phong ấn.\n";
            if (IsPassiveSlotsLocked) desc += "• <color=red>[KHÓA Ô THIÊN PHÚ]</color>: Slot Thiên Phú bị phong ấn.\n";
            if (IsEquipmentSlotsLocked) desc += "• <color=red>[KHÓA Ô TRANG BỊ]</color>: Không thể trang bị vũ khí/bùa.\n";
            desc += "\n";
        }

        desc += "<i>" + SokLoc.Translate(this.DescriptionTerm) + "</i>";
        this.descriptionOverride = desc;
        base.UpdateCardText();
    }

    // TriggerDemonicAscension removed per user request
}
