using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat.Core;
using Mewtations.Combat.Battlefield;

public class Combatable : CardData
{
	public List<Equipable> PossibleEquipables
	{
		get
		{
			if (!Application.isPlaying)
			{
				return (from id in this.PossibleEquipableIds
					select (Equipable)new GameDataLoader(true, true).GetCardFromId(id, true) into e
					where e != null
					select e).ToList<Equipable>();
			}
			return (from id in this.PossibleEquipableIds
				select (Equipable)WorldManager.instance.GameDataLoader.GetCardFromId(id, true) into e
				where e != null
				select e).ToList<Equipable>();
		}
	}

	public AttackType ProcessedAttackType
	{
		get
		{
			Equipable equipableOfEquipableType = base.GetEquipableOfEquipableType(EquipableType.Weapon);
			if (equipableOfEquipableType != null)
			{
				return equipableOfEquipableType.AttackType;
			}
			AttackType attackType = this.BaseAttackType;
			if (this.InheritCombatStatsFromOtherCard)
			{
				Combatable combatable = WorldManager.instance.GameDataLoader.GetCardFromId(this.InheritCombatStatsFrom, true) as Combatable;
				if (combatable.InheritCombatStatsFromOtherCard)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + this.Id + " also inherits from another card");
				}
				attackType = combatable.BaseAttackType;
			}
			return attackType;
		}
	}

	public override bool HasInventory
	{
		get
		{
			return this.CanHaveInventory;
		}
	}

	public CombatStats RealBaseCombatStats
	{
		get
		{
			CombatStats combatStats = new CombatStats();
			if (!this.InheritCombatStatsFromOtherCard)
			{
				combatStats.InitStats(this.BaseCombatStats);
			}
			else
			{
				Combatable combatable = WorldManager.instance.GameDataLoader.GetCardFromId(this.InheritCombatStatsFrom, true) as Combatable;
				if (!combatable)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + this.Id + " is not set or incorrect");
				}
				else if (combatable.InheritCombatStatsFromOtherCard)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + this.Id + " also inherits from another card");
				}
				else
				{
					combatStats.InitStats(combatable.BaseCombatStats);
				}
			}
			return combatStats;
		}
	}

	public CombatStats ProcessedCombatStats
	{
		get
		{
			CombatStats realBaseCombatStats = this.RealBaseCombatStats;
			foreach (Equipable equipable in base.GetAllEquipables())
			{
				realBaseCombatStats.AddStats(equipable.MyStats);
			}
			realBaseCombatStats.MaxHealth = ((realBaseCombatStats.MaxHealth >= 1) ? realBaseCombatStats.MaxHealth : 1);
			return realBaseCombatStats;
		}
	}

	[HideInInspector]
	public bool InConflict
	{
		get
		{
			return this.MyConflict != null;
		}
	}

	public bool IsPassiveCombatant => !Mewtations.Core.LegacyRuntimeFlags.EnableRealtimeCombat;

	public bool CanLeaveConflict
	{
		get
		{
			return this.MyConflict != null && this.MyConflict.CanLeaveConflict(this);
		}
	}

	public Team Team
	{
		get
		{
			if (this is BaseVillager || this is CitiesCombatable)
			{
				return Team.Player;
			}
			return Team.Enemy;
		}
	}

	public override void OnLanguageChange()
	{
		this._combatableDescription = null;
		base.OnLanguageChange();
	}

	public virtual float GetAccuracyScore()
	{
		float num = this.ProcessedCombatStats.Accuracy;
		if (base.HasStatusEffectOfType<StatusEffect_Drunk>())
		{
			num *= 0.6f;
		}
		if (this is CatCardData cat && cat.HasScar(Mewtations.Combat.PermanentScar.HeartDemonPossessed))
		{
			num *= 0.8f; // Giảm 20% tỷ lệ đánh trúng
		}
		return num;
	}

	public virtual float GetInitiativeScore()
	{
		float baseInitiative = this.ProcessedCombatStats.Initiative;
		if (base.HasStatusEffectOfType<StatusEffect_Frenzy>())
		{
			baseInitiative = CombatStats.IncrementAttackSpeed(baseInitiative, 1);
		}
		if (_statusEffectPipeline != null)
		{
			int slowPercent = _statusEffectPipeline.GetSlowPercent();
			if (slowPercent > 0)
			{
				baseInitiative *= (1f + slowPercent / 100f);
			}
		}
		return baseInitiative;
	}



	public float DamageMultiplier
	{
		get
		{
			if (base.HasStatusEffectOfType<StatusEffect_Drunk>())
			{
				return 2f;
			}
			return 1f;
		}
	}

	public string GetCombatTypeTitle()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return MewtationsLoc.Translate("label_melee_title");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return MewtationsLoc.Translate("label_ranged_title");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return MewtationsLoc.Translate("label_magic_title");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return MewtationsLoc.Translate("label_air_title");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return MewtationsLoc.Translate("label_foot_title");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return MewtationsLoc.Translate("label_armour_title");
		}
		return "";
	}

	public string GetCombatTypeDescription()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return MewtationsLoc.Translate("label_melee_description");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return MewtationsLoc.Translate("label_ranged_description");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return MewtationsLoc.Translate("label_magic_description");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return MewtationsLoc.Translate("label_air_description");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return MewtationsLoc.Translate("label_foot_description");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return MewtationsLoc.Translate("label_armour_description");
		}
		return "";
	}

	public string GetCombatTypeLore()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return MewtationsLoc.Translate("label_melee_lore");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return MewtationsLoc.Translate("label_ranged_lore");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return MewtationsLoc.Translate("label_magic_lore");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return MewtationsLoc.Translate("label_air_lore");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return MewtationsLoc.Translate("label_foot_lore");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return MewtationsLoc.Translate("label_armour_lore");
		}
		return "";
	}

	public override void OnEquipItem(Equipable equipable)
	{
		this._combatableDescription = null;
		if (this.HealthPoints > this.ProcessedCombatStats.MaxHealth)
		{
			this.HealthPoints = this.ProcessedCombatStats.MaxHealth;
		}
	}

	public override void OnUnequipItem(Equipable equipable)
	{
		this._combatableDescription = null;
	}

	private void StartOrJoinConflictInStack()
	{
		if (this.MyGameCard.HasTransportCard())
		{
			return;
		}
		BattlefieldContext conflictInStack = this.GetConflictInStack();
		if (conflictInStack != null)
		{
			conflictInStack.JoinConflict(this);
			return;
		}
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Combatable && x != this);

		// Turn-based is the SINGLE SOURCE OF TRUTH for all battles
		List<Combatable> players = new List<Combatable>();
		List<Combatable> enemies = new List<Combatable>();

		if (this.HealthPoints > 0)
		{
			if (this.Team == Team.Player) players.Add(this);
			else enemies.Add(this);
		}

		foreach (var card in list)
		{
			if (card is Combatable comb && comb.HealthPoints > 0)
			{
				if (comb.Team == Team.Player) players.Add(comb);
				else enemies.Add(comb);
			}
		}

		if (players.Count > 0 && enemies.Count > 0)
		{
			Mewtations.Combat.TurnBasedCombatManager.Instance.StartCombat(players, enemies, (result) =>
			{
				if (result == Mewtations.Combat.CombatResult.Victory)
				{
					foreach (var enemy in enemies)
					{
						if (enemy != null && enemy.MyGameCard != null)
						{
							enemy.MyGameCard.DestroyCard(true, true);
						}
					}
				}
				else
				{
					// Separate players and enemies to avoid instant re-trigger
					foreach (var player in players)
					{
						if (player != null && player.MyGameCard != null)
						{
							player.MyGameCard.RemoveFromStack();
							player.MyGameCard.TargetPosition = player.MyGameCard.transform.position + new Vector3(UnityEngine.Random.Range(-2.5f, -1.5f), 0f, UnityEngine.Random.Range(-2.5f, -1.5f));
						}
					}
					foreach (var enemy in enemies)
					{
						if (enemy != null && enemy.MyGameCard != null)
						{
							enemy.MyGameCard.RemoveFromStack();
							enemy.MyGameCard.TargetPosition = enemy.MyGameCard.transform.position + new Vector3(UnityEngine.Random.Range(1.5f, 2.5f), 0f, UnityEngine.Random.Range(1.5f, 2.5f));
						}
					}
				}
			});
		}
	}

	public void OnHealthChange()
	{
		this._combatableDescription = null;
	}

	private BattlefieldContext GetConflictInStack()
	{
		foreach (GameCard gameCard in this.MyGameCard.GetAllCardsInStack())
		{
			Combatable combatable = gameCard.CardData as Combatable;
			if (combatable != null && combatable.MyConflict != null)
			{
				return combatable.MyConflict;
			}
		}
		return null;
	}

	public override void UpdateCard()
	{
		if (_statusEffectPipeline != null && WorldManager.instance != null && !WorldManager.instance.GamePaused)
		{
			_statusEffectPipeline.UpdateTick(Time.deltaTime * WorldManager.instance.TimeScale);
		}
		if (this.previouseHealthPoints != this.HealthPoints)
		{
			this.OnHealthChange();
		}
		this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.HealthIcon;
		if (this.MyGameCard != null && this.MyGameCard.IsDemoCard)
		{
			this.MyGameCard.SpecialValue = new int?(this.ProcessedCombatStats.MaxHealth);
		}
		else
		{
			this.MyGameCard.SpecialValue = new int?(this.HealthPoints);
		}
		this.UpdateCombatableTargets();
		if (this.Team != Team.Enemy && (this.combatableTargets.Count > 0 || this.GetConflictInStack() != null) && !this.InConflict)
		{
			this.StartOrJoinConflictInStack();
		}
		if (this.MyConflict != null && this.MyConflict.Initiator == this)
		{
			this.MyConflict.UpdateConflict();
		}
		// Legacy realtime combat loop disabled. Combat execution controlled exclusively by CombatV2.
		this.previouseHealthPoints = this.HealthPoints;
		base.UpdateCard();
		this.CheckDeath();
	}

	public override void UpdateCardText()
	{
		this.descriptionOverride = MewtationsLoc.Translate(this.DescriptionTerm);
		this.descriptionOverride = this.descriptionOverride + "\n\n<i>" + this.GetCombatableDescription() + "</i>";
		if (AdvancedSettingsScreen.AdvancedCombatStatsEnabled || GameCanvas.instance.CurrentScreen is CardopediaScreen)
		{
			this.descriptionOverride = this.descriptionOverride + "\n\n<i>" + this.GetCombatableDescriptionAdvanced() + "</i>";
		}
		base.UpdateCardText();
	}

	public virtual void NotifyParticipantUpdate(Combatable oldParticipant, Combatable newParticipant)
	{
	}

	public Projectile CreateProjectile(Projectile projectilePrefab, Combatable target, AttackAnimation originAnimation)
	{
		Projectile projectile = Object.Instantiate<Projectile>(projectilePrefab);
		projectile.ShotBy = this;
		projectile.Target = target;
		projectile.transform.position = (projectile.StartPosition = base.transform.position);
		projectile.TargetPosition = originAnimation.AttackTargetPosition;
		projectile.OriginAnimation = originAnimation;
		return projectile;
	}



	public override void StoppedDragging()
	{
		GameCard parent = this.MyGameCard.Parent;
		Combatable combatable = ((parent != null) ? parent.Combatable : null);
		if (this.InConflict)
		{
			if (combatable != null && combatable.InConflict)
			{
				if (combatable.MyConflict == this.MyConflict)
				{
					this.MyConflict.SetParticipantTeamIndex(this, this.MyConflict.GetIndexInTeam(combatable));
				}
				else
				{
					this.MyConflict.LeaveConflict(this);
					this.StartOrJoinConflictInStack();
				}
				this.MyGameCard.RemoveFromStack();
				return;
			}
			if (!this.CanLeaveConflict)
			{
				this.MyGameCard.RemoveFromStack();
				return;
			}
			BattlefieldContext overlappingConflict = this.MyGameCard.GetOverlappingConflict();
			if (overlappingConflict != null && overlappingConflict != this.MyConflict)
			{
				this.MyConflict.LeaveConflict(this);
				overlappingConflict.JoinConflict(this);
			}
			if (overlappingConflict == null)
			{
				this.MyConflict.LeaveConflict(this);
				return;
			}
		}
		else
		{
			if (combatable != null && combatable.Team != this.Team)
			{
				this.MyGameCard.transform.position = combatable.transform.position;
				this.StartOrJoinConflictInStack();
			}
			BattlefieldContext overlappingConflict2 = this.MyGameCard.GetOverlappingConflict();
			if (overlappingConflict2 != null && !this.InConflict)
			{
				overlappingConflict2.JoinConflict(this);
			}
		}
	}

	public void CreateAndEquipCard(string cardId, bool markAsFound)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(cardId, true);
		if (!(cardPrefab is Equipable))
		{
			Debug.LogError(string.Concat(new string[] { "Can't give ", cardId, " to ", this.Id, " because it is not an Equipable" }));
			return;
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardPrefab, false, true, true, markAsFound);
		cardData.MyGameCard.MyBoard = this.MyGameCard.MyBoard;
		base.EquipItem(cardData as Equipable);
		cardData.MyGameCard.Visuals.gameObject.SetActive(false);
	}

	public void ExitConflict()
	{
	}

	public SpecialHit DetermineSpecialHit()
	{
		WeightedRandomBag<SpecialHit> weightedRandomBag = new WeightedRandomBag<SpecialHit>();
		float num = 0f;
		foreach (SpecialHit specialHit in this.ProcessedCombatStats.SpecialHits)
		{
			num += specialHit.Chance;
			weightedRandomBag.AddEntry(specialHit, specialHit.Chance);
		}
		SpecialHit specialHit2 = new SpecialHit();
		specialHit2.HitType = SpecialHitType.None;
		specialHit2.Target = SpecialHitTarget.Target;
		specialHit2.Chance = 100f - num;
		weightedRandomBag.AddEntry(specialHit2, specialHit2.Chance);
		return weightedRandomBag.Choose();
	}

	public int GetDamage(Combatable target)
	{
		if (target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
		{
			return 0;
		}
		int attackDamage = this.ProcessedCombatStats.AttackDamage;
		int num = CombatStats.IncrementAttackDefence(this.ProcessedCombatStats.AttackDamage, 1);
		int num2 = ((Random.value < 0.5f) ? attackDamage : num);
		int defence = target.ProcessedCombatStats.Defence;
		int num3 = num2 - Mathf.CeilToInt((float)defence * 0.5f);
		num3 = Mathf.RoundToInt((float)num3 * this.GetCombatRuleMultiplier(target, this) * this.DamageMultiplier);
		if (num3 > 0)
		{
			return num3;
		}
		return Mathf.RoundToInt(Random.value);
	}

	public bool IsVeryEffective(AttackType self, AttackType target)
	{
		return (self == AttackType.Melee && target == AttackType.Magic) || (self == AttackType.Magic && target == AttackType.Ranged) || (self == AttackType.Ranged && target == AttackType.Melee);
	}

	public float GetCombatRuleMultiplier(Combatable target, Combatable self)
	{
		if (!this.IsVeryEffective(self.ProcessedAttackType, target.ProcessedAttackType))
		{
			return 1f;
		}
		return 1.4f;
	}

	private List<Combatable> GetSpecialHitTargets(SpecialHit specialHit, Combatable target)
	{
		if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			List<Combatable> list = (from x in this.MyConflict.GetFriendlyParticipants(this)
				orderby x.HealthPoints
				select x).ToList<Combatable>();
			if (list.Count > 0)
			{
				return list[0].AsList<Combatable>();
			}
		}
		switch (specialHit.Target)
		{
		case SpecialHitTarget.Self:
			return this.AsList<Combatable>();
		case SpecialHitTarget.Target:
			return target.AsList<Combatable>();
		case SpecialHitTarget.RandomFriendly:
			return this.MyConflict.GetFriendlyParticipants(this).Choose<Combatable>().AsList<Combatable>();
		case SpecialHitTarget.RandomEnemy:
			return this.MyConflict.GetEnemyParticipants(this).Choose<Combatable>().AsList<Combatable>();
		case SpecialHitTarget.AllFriendly:
			return this.MyConflict.GetFriendlyParticipants(this);
		case SpecialHitTarget.AllEnemy:
			return this.MyConflict.GetEnemyParticipants(this);
		default:
			return target.AsList<Combatable>();
		}
	}

	private void ApplyElementalAttackEffect(Combatable target, int damage)
	{
		if (target == null || target.HealthPoints <= 0) return;
		CatElement attackerElement = CatElement.None;
		if (this is CatCardData cat)
		{
			attackerElement = cat.Element;
		}
		if (attackerElement == CatElement.None) return;
		switch (attackerElement)
		{
			case CatElement.Fire:
				target.ApplyStatusEffect(new ActiveStatusEffect(
					"burn",
					"Thiêu Đốt",
					4.0f,
					StatusEffectStackingRule.AccumulateStacks,
					10,
					1.0f,
					(owner, stacks) => {
						int dotDmg = Mathf.Max(1, Mathf.RoundToInt(2f * (1f + (stacks - 1) * 0.6f)));
						owner.Damage(dotDmg);
						owner.MyGameCard.RotWobble(0.2f);
					}
				));
				break;
			case CatElement.Ice:
				target.ApplyStatusEffect(new ActiveStatusEffect(
					"slow",
					"Làm Chậm",
					3.5f,
					StatusEffectStackingRule.RefreshDuration
				));
				break;
			case CatElement.Lightning:
				ChainLightningSystem.CastChainLightning(this, target, damage, 4, 0.25f);
				break;
		}
	}

	private void PerformSpecialHit(SpecialHit specialHit, Combatable target, int dmg)
	{
		Debug.Log(string.Format("Special hit by {0}: {1}", base.Name, specialHit.HitType));
		if (specialHit.HitType == SpecialHitType.Poison)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Poison>())
			{
				target.AddStatusEffect(new StatusEffect_Poison());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Crit)
		{
			dmg *= 2;
		}
		else if (specialHit.HitType == SpecialHitType.Stun)
		{
			target.RemoveStatusEffect<StatusEffect_Stunned>();
			target.AddStatusEffect(new StatusEffect_Stunned());
		}
		else if (specialHit.HitType == SpecialHitType.Bleeding)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Bleeding>())
			{
				target.AddStatusEffect(new StatusEffect_Bleeding());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Frenzy)
		{
			target.RemoveStatusEffect<StatusEffect_Frenzy>();
			target.AddStatusEffect(new StatusEffect_Frenzy());
		}
		else if (specialHit.HitType == SpecialHitType.Sick)
		{
			if (!target.HasEquipableWithId("plague_mask"))
			{
				target.RemoveStatusEffect<StatusEffect_Sick>();
				target.AddStatusEffect(new StatusEffect_Sick());
				AudioManager.me.PlaySound2D(AudioManager.me.GetSick, 1f, 0.5f);
			}
		}
		else if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			target.HealthPoints = Mathf.Clamp(target.HealthPoints + 2, 0, target.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.Heal)
		{
			target.HealthPoints = Mathf.Clamp(target.HealthPoints + 2, 0, target.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.LifeSteal)
		{
			this.HealthPoints = Mathf.Clamp(this.HealthPoints + dmg, 0, this.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.Invulnerable)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
			{
				target.AddStatusEffect(new StatusEffect_Invulnerable());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Anxious && !target.HasStatusEffectOfType<StatusEffect_Anxious>())
		{
			target.AddStatusEffect(new StatusEffect_Anxious());
		}
		bool flag = specialHit.Target == SpecialHitTarget.Target || specialHit.Target == SpecialHitTarget.RandomEnemy || specialHit.Target == SpecialHitTarget.AllEnemy;
		if (specialHit.Target == SpecialHitTarget.Self && (specialHit.HitType == SpecialHitType.Crit || specialHit.HitType == SpecialHitType.Stun || specialHit.HitType == SpecialHitType.Bleeding))
		{
			flag = true;
		}
		if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			flag = false;
		}
		if (flag)
		{
			target.Damage(dmg);
		}
	}

	private void ShowHitText(Combatable origin, Combatable effectTarget, Vector3 targetPosition, bool isHit, int damage, SpecialHitType? type = null)
	{
		bool flag = this.IsVeryEffective(origin.ProcessedAttackType, effectTarget.ProcessedAttackType);
		if (type != null)
		{
			if (type != null)
			{
				switch (type.GetValueOrDefault())
				{
				case SpecialHitType.Poison:
				case SpecialHitType.Bleeding:
				case SpecialHitType.Sick:
				case SpecialHitType.Anxious:
					effectTarget.CreateHitText(string.Format("{0}", damage), PrefabManager.instance.HitTextPrefab).SetVeryEffective(flag);
					AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
					return;
				case SpecialHitType.Stun:
					AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText("stun", PrefabManager.instance.CritHitText).SetVeryEffective(flag);
					return;
				case SpecialHitType.Heal:
					AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText("2", PrefabManager.instance.HealHitText);
					return;
				case SpecialHitType.HealLowest:
					AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText("2", PrefabManager.instance.HealHitText);
					return;
				case SpecialHitType.LifeSteal:
					AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
					AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText(string.Format("{0}", damage), PrefabManager.instance.BleedHitText).SetVeryEffective(flag);
					origin.CreateHitText(string.Format("{0}", damage), PrefabManager.instance.HealHitText);
					return;
				case SpecialHitType.Frenzy:
				case SpecialHitType.Invulnerable:
					effectTarget.CreateHitText("buff", PrefabManager.instance.HitTextPrefab);
					AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
					return;
				case SpecialHitType.Damage:
					AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText(string.Format("{0}", damage), null).SetVeryEffective(flag);
					return;
				case SpecialHitType.Crit:
					AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1.2f), 0.2f);
					effectTarget.CreateHitText(string.Format("{0}!", damage), PrefabManager.instance.CritHitText).SetVeryEffective(flag);
					return;
				}
			}
			effectTarget.CreateHitText("NYI", null);
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			return;
		}
		if (isHit)
		{
			if (damage == 0)
			{
				if (effectTarget.HasStatusEffectOfType<StatusEffect_Invulnerable>())
				{
					effectTarget.CreateHitText("block", PrefabManager.instance.BlockHitText);
				}
				else
				{
					effectTarget.CreateHitText("block", PrefabManager.instance.BlockHitText);
				}
				AudioManager.me.PlaySound2D(AudioManager.me.Block, Random.Range(0.8f, 1.2f), 0.2f);
				return;
			}
			effectTarget.CreateHitText(string.Format("{0}", damage), null).SetVeryEffective(flag);
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			return;
		}
		else
		{
			effectTarget.CreateHitText("miss", PrefabManager.instance.MissHitText).transform.position = targetPosition;
			if (WorldManager.instance.CurrentBoard.Id == "cities")
			{
				AudioManager.me.PlaySound2D(AudioManager.me.MissCities, Random.Range(0.8f, 1.2f), 0.5f);
				return;
			}
			AudioManager.me.PlaySound2D(AudioManager.me.Miss, Random.Range(0.8f, 1.2f), 0.5f);
			return;
		}
	}

	public List<AudioClip> GetAttackTypeHitSound()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return AudioManager.me.HitMelee;
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return AudioManager.me.HitRanged;
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return AudioManager.me.HitMagic;
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return AudioManager.me.HitFoot;
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return AudioManager.me.HitArmour;
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return AudioManager.me.HitAir;
		}
		return AudioManager.me.HitMelee;
	}

	public HitText CreateHitText(string txt, HitText prefab = null)
	{
		if (prefab == null)
		{
			prefab = PrefabManager.instance.NormalHitText;
		}
		HitText hitText = WorldManager.instance.CreateHitText(base.transform.position, txt, prefab);
		hitText.TargetCombatable = this;
		this.CurrentHitText = hitText;
		return hitText;
	}

	public virtual void Damage(int damage)
	{
		this.HealthPoints -= damage;
		this.HealthPoints = Mathf.Max(this.HealthPoints, 0);
		this.StunTimer = 0.05f;
		GameCamera.instance.Screenshake = 0.3f;
		this.MyGameCard.SetHitEffect(delegate
		{
			this.CheckDeath();
		});
		this.MyGameCard.RotWobble(0.5f);
		this.MyGameCard.transform.localScale *= 1.5f;
	}

	private void CheckDeath()
	{
		if (this.isDead)
		{
			return;
		}
		if (this.HealthPoints <= 0)
		{
			if (this is CatCardData)
			{
				return; // Mèo không thực sự chết, chỉ bị tê tái ở 0 HP
			}
			this.isDead = true;
			QuestManager.instance.SpecialActionComplete(this.Id + "_killed", null);
			this.Die();
		}
	}

	public virtual void Die()
	{
		if (this.MyConflict != null)
		{
			this.MyConflict.LeaveConflict(this);
		}
		this.MyGameCard.GetAllCardsInStack().Remove(this.MyGameCard);
		this.MyGameCard.DestroyCard(true, true);
	}

	public virtual void UpdateCombatableTargets()
	{
		this.combatableTargets.Clear();
		GameCard gameCard = this.MyGameCard.GetRootCard();
		while (gameCard != null)
		{
			Combatable combatable = gameCard.CardData as Combatable;
			if (combatable != null && gameCard.CardData != this && combatable.Team != this.Team)
			{
				this.combatableTargets.Add(combatable);
			}
			gameCard = gameCard.Child;
		}
	}

	public string GetCombatableDescription()
	{
		if (!string.IsNullOrEmpty(this._combatableDescription))
		{
			return this._combatableDescription;
		}
		string text = "";
		if (this.MyGameCard != null && !this.MyGameCard.IsDemoCard)
		{
			text = text + MewtationsLoc.Translate("label_health_info", new LocParam[]
			{
				LocParam.Create("health", this.HealthPoints.ToString()),
				LocParam.Create("maxhealth", this.ProcessedCombatStats.MaxHealth.ToString())
			}) + "\n";
		}
		int num = Mathf.RoundToInt(this.RealBaseCombatStats.CombatLevel);
		int num2 = Mathf.RoundToInt(this.ProcessedCombatStats.CombatLevel);
		if (num2 != num)
		{
			text = text + MewtationsLoc.Translate("label_base_combatlevel", new LocParam[] { LocParam.Create("level", num.ToString()) }) + "\n";
			text += MewtationsLoc.Translate("label_total_combatlevel", new LocParam[] { LocParam.Create("level", num2.ToString()) });
		}
		else
		{
			text += MewtationsLoc.Translate("label_combatlevel", new LocParam[] { LocParam.Create("level", num2.ToString()) });
		}
		string text2 = this.ProcessedCombatStats.SummarizeSpecialHits();
		if (text2.Length > 0)
		{
			text = text + "\n\n" + text2;
		}
		this._combatableDescription = text;
		return text;
	}

	public string GetCombatableDescriptionAdvanced()
	{
		string text = MewtationsLoc.Translate("label_combat_speed");
		string text2 = MewtationsLoc.Translate("label_hit_chance");
		string text3 = MewtationsLoc.Translate("label_damage");
		string text4 = MewtationsLoc.Translate("label_defence");
		CombatStats processedCombatStats = this.ProcessedCombatStats;
		string attackSpeedTranslation = processedCombatStats.GetAttackSpeedTranslation();
		string attackDamageTranslation = processedCombatStats.GetAttackDamageTranslation();
		string hitChanceTranslation = processedCombatStats.GetHitChanceTranslation();
		string defenceTranslation = processedCombatStats.GetDefenceTranslation();
		string text5 = MewtationsLoc.Translate("label_seconds_format", new LocParam[] { LocParam.Create("seconds", processedCombatStats.AttackSpeed.ToString()) });
		return string.Format("<size=80%>{0} {1} ({2})\n{3} {4} ({5}%)\n{6} {7} ({8})\n{9}: {10} ({11})</size>", new object[]
		{
			text,
			attackSpeedTranslation,
			text5,
			text2,
			hitChanceTranslation,
			processedCombatStats.HitChance * 100f,
			text3,
			attackDamageTranslation,
			processedCombatStats.AttackDamage,
			text4,
			defenceTranslation,
			processedCombatStats.Defence
		});
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying && this.InConflict)
		{
			foreach (Combatable combatable in this.MyConflict.GetCombatableTargets(this))
			{
				Gizmos.DrawLine(base.transform.position, combatable.transform.position);
			}
			Bounds bounds = this.MyConflict.GetBounds();
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}

	public static bool SpecialHitTypeIsAttack(SpecialHitType hitType)
	{
		return hitType != SpecialHitType.Heal && hitType != SpecialHitType.HealLowest && hitType != SpecialHitType.Invulnerable;
	}

	public void LogBaseCombatLevel()
	{
		Debug.Log(string.Format("Base combat level: {0}", this.BaseCombatStats.CombatLevel));
	}

	[Header("Combat")]
	public bool CanHaveInventory;

	public bool CanAttack = true;

	public List<string> PossibleEquipableIds = new List<string>();

	public bool InheritCombatStatsFromOtherCard;

	[Card]
	public string InheritCombatStatsFrom;

	public AttackType BaseAttackType;

	public CombatStats BaseCombatStats;

	private string _combatableDescription;

	[ExtraData("health")]
	public int HealthPoints = 3;

	private int previouseHealthPoints;

	protected List<Combatable> combatableTargets = new List<Combatable>();

	[HideInInspector]
	public AttackType CurrentAttackType;

	public BattlefieldContext MyConflict;

	private SpecialHit AttackSpecialHit;

	[HideInInspector]
	public HitText CurrentHitText;

	[HideInInspector]
	private bool isDead;

	protected StatusEffectPipeline _statusEffectPipeline;
	public StatusEffectPipeline StatusEffects
	{
		get
		{
			if (_statusEffectPipeline == null)
			{
				_statusEffectPipeline = new StatusEffectPipeline(this);
			}
			return _statusEffectPipeline;
		}
	}

	public void ApplyStatusEffect(ActiveStatusEffect effect)
	{
		StatusEffects.ApplyEffect(effect);
	}
}

