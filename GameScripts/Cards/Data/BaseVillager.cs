using System;
using System.Linq;
using UnityEngine;

namespace Mewtations.Legacy.Stacklands
{
	[Obsolete("Legacy humanoid runtime entity. Undergoing controlled decay.", false)]
	public class BaseVillager : Combatable
	{
		public LifeStage MyLifeStage
		{
			get
			{
				if (!WorldManager.instance.CurseIsActive(CurseType.Death))
				{
					return LifeStage.Adult;
				}
				return this.DetermineLifeStageFromAge(this.Age);
			}
		}

		public override bool HasInventory
		{
			get
			{
				return !(this.Id == "trained_monkey");
			}
		}

		protected override bool CanHaveCard(CardData otherCard)
		{
			if (!(otherCard is BaseVillager) && otherCard.MyCardType != CardType.Resources && otherCard.MyCardType != CardType.Equipable)
			{
				Food food = otherCard as Food;
				if (food == null || !food.CanBePlacedOnVillager)
				{
					return otherCard.Id == "naming_stone";
				}
			}
			return true;
		}

		public override void UpdateCardText()
		{
			this.descriptionOverride = SokLoc.Translate(this.DescriptionTerm);
			this.descriptionOverride += "\n\n";
			if (WorldManager.instance.CurseIsActive(CurseType.Death))
			{
				this.descriptionOverride = this.descriptionOverride + "<i>" + SokLoc.Translate("label_villager_age_description", new LocParam[] { LocParam.Plural("age", this.Age + 1) }) + "<i>\n";
			}
			this.descriptionOverride = this.descriptionOverride + "<i>" + base.GetCombatableDescription() + "</i>";
			if (AdvancedSettingsScreen.AdvancedCombatStatsEnabled || GameCanvas.instance.CurrentScreen is CardopediaScreen)
			{
				this.descriptionOverride = this.descriptionOverride + "\n\n<i>" + base.GetCombatableDescriptionAdvanced() + "</i>";
			}
			bool flag = !this.ChangesCardOnStage || !string.IsNullOrEmpty(this.CustomName);
			string text = this.NameTerm;
			if (flag)
			{
				if (this.MyLifeStage == LifeStage.Adult)
				{
					text = this.NameTerm;
				}
				else if (this.MyLifeStage == LifeStage.Teenager)
				{
					text = this.NameTerm + "_young";
				}
				else if (this.MyLifeStage == LifeStage.Elderly)
				{
					text = this.NameTerm + "_old";
				}
				else if (this.MyLifeStage == LifeStage.Dead)
				{
					text = this.NameTerm + "_old";
				}
			}
			this.nameOverride = SokLoc.Translate(text);
			if (!string.IsNullOrEmpty(this.CustomName))
			{
				if (flag)
				{
					if (this.MyLifeStage == LifeStage.Adult)
					{
						this.nameOverride = this.CustomName;
						return;
					}
					if (this.MyLifeStage == LifeStage.Teenager)
					{
						this.nameOverride = SokLoc.Translate("label_villager_young", new LocParam[] { LocParam.Create("villager", this.CustomName) });
						return;
					}
					if (this.MyLifeStage == LifeStage.Elderly)
					{
						this.nameOverride = SokLoc.Translate("label_villager_old", new LocParam[] { LocParam.Create("villager", this.CustomName) });
						return;
					}
					if (this.MyLifeStage == LifeStage.Dead)
					{
						this.nameOverride = SokLoc.Translate("label_villager_old", new LocParam[] { LocParam.Create("villager", this.CustomName) });
						return;
					}
				}
				else
				{
					this.nameOverride = this.CustomName;
				}
			}
		}

		public override void UpdateCard()
		{
			if (WorldManager.instance.TimeScale > 0f && !WorldManager.instance.InAnimation)
			{
				this.UpdateLifeStage();
			}
			base.UpdateCard();
		}

		public virtual int GetRequiredFoodCount()
		{
			if (!WorldManager.LegacyFoodTaxEnabled) return 0;
			if (this.Id == "trained_monkey")
			{
				return 0;
			}
			if (this.Id == "dog")
			{
				return 1;
			}
			return 2;
		}

		public override void Die()
		{
			WorldManager.instance.KillVillager(this, null, null);
			if (WorldManager.instance.CardQuery.GetCardCount<BaseVillager>() == 2 && this.MyLifeStage == LifeStage.Elderly)
			{
				WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("death_middle_villager");
			}
			if (this.MyConflict != null)
			{
				this.MyConflict.LeaveConflict(this);
			}
		}

		public float GetActionTimeModifier(string actionId, CardData baseCard)
		{
			float num = 1f;
			ActionTimeParams actionTimeParams = new ActionTimeParams(this, actionId, baseCard);
			foreach (ActionTimeBase actionTimeBase in WorldManager.instance.actionTimeBases)
			{
				if (actionTimeBase.Matches(actionTimeParams))
				{
					num = actionTimeBase.BaseSpeed;
				}
			}
			foreach (ActionTimeModifier actionTimeModifier in WorldManager.instance.actionTimeModifiers)
			{
				if (actionTimeModifier.Matches(actionTimeParams))
				{
					num *= actionTimeModifier.SpeedModifier;
				}
			}
			return num;
		}

		public override void OnEquipItem(Equipable equipable)
		{
			if (equipable.Id == "royal_crown")
			{
				WorldManager.instance.Cutscene.QueueCutscene(GreedCutscenes.GreedWearCrown());
				this.MyGameCard.Unequip(equipable);
				return;
			}
			if (this.CanOverrideCardFromEquipment && !string.IsNullOrEmpty(equipable.VillagerTypeOverride) && equipable.VillagerTypeOverride != this.Id)
			{
				WorldManager.instance.ChangeToCard(this.MyGameCard, equipable.VillagerTypeOverride);
			}
			base.OnEquipItem(equipable);
		}

		private Equipable GetOverrideEquipable()
		{
			if (!this.CanOverrideCardFromEquipment)
			{
				return null;
			}
			return base.GetAllEquipables().FirstOrDefault<Equipable>((Equipable x) => !string.IsNullOrEmpty(x.VillagerTypeOverride));
		}

		public override void OnUnequipItem(Equipable equipable)
		{
			if (this.CanOverrideCardFromEquipment)
			{
				if (this.GetOverrideEquipable() == null && this.Id != "villager")
				{
					(WorldManager.instance.ChangeToCard(this.MyGameCard, "villager") as Villager).UpdateLifeStage();
				}
				else if (this.GetOverrideEquipable() != null && this.GetOverrideEquipable().VillagerTypeOverride != this.Id)
				{
					(WorldManager.instance.ChangeToCard(this.MyGameCard, this.GetOverrideEquipable().VillagerTypeOverride) as BaseVillager).UpdateLifeStage();
				}
			}
			base.OnUnequipItem(equipable);
		}

		public void UpdateLifeStage()
		{
			if (!WorldManager.LegacyFoodTaxEnabled) return;
			if (!this.ChangesCardOnStage)
			{
				return;
			}
			string text = this.DetermineCardFromStage(this.MyLifeStage);
			if (text != null && text != this.Id)
			{
				WorldManager.instance.ChangeToCard(this.MyGameCard, text);
			}
		}

		public string DetermineCardFromStage(LifeStage stage)
		{
			if (this.Id == "teenage_villager" || this.Id == "villager" || this.Id == "old_villager")
			{
				if (stage == LifeStage.Teenager)
				{
					return "teenage_villager";
				}
				if (stage == LifeStage.Adult)
				{
					return "villager";
				}
				if (stage == LifeStage.Elderly)
				{
					QuestManager.instance.SpecialActionComplete("villager_old", null);
					return "old_villager";
				}
			}
			if (this.Id == "puppy" || this.Id == "dog" || this.Id == "old_dog")
			{
				if (stage == LifeStage.Teenager)
				{
					return "puppy";
				}
				if (stage == LifeStage.Adult)
				{
					return "dog";
				}
				if (stage == LifeStage.Elderly)
				{
					return "old_dog";
				}
			}
			if (this.Id == "kitten" || this.Id == "cat" || this.Id == "old_cat")
			{
				if (stage == LifeStage.Teenager)
				{
					return "kitten";
				}
				if (stage == LifeStage.Adult)
				{
					return "cat";
				}
				if (stage == LifeStage.Elderly)
				{
					return "old_cat";
				}
			}
			return null;
		}

		public LifeStage DetermineLifeStageFromAge(int age)
		{
			if (age < 2)
			{
				return LifeStage.Teenager;
			}
			if (age <= 6)
			{
				return LifeStage.Adult;
			}
			if (age <= 8)
			{
				return LifeStage.Elderly;
			}
			return LifeStage.Dead;
		}

		public bool WillChangeLifeStage()
		{
			return this.DetermineLifeStageFromAge(this.Age) != this.DetermineLifeStageFromAge(this.Age + 1);
		}

		public bool CanOverrideCardFromEquipment;

		[ExtraData("age")]
		public int Age;

		public bool ChangesCardOnStage;

		[HideInInspector]
		public bool AteUncookedFood;

		public bool CanBreed = true;
	}
}
