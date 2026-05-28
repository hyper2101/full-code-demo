using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Mewtations.Combat.Core;

namespace Mewtations.Legacy.Stacklands
{
	[Obsolete("Legacy combat entity. Transitioning to CombatV2.", false)]
	public class Enemy : Mob
	{
		public float HoldProgress = 0f;
		public const float MaxHoldTime = 2.0f;

		public override bool CanMove
		{
			get
			{
				return false;
			}
		}

		protected override void Move()
		{
			// Strictly static - no movement or velocity updates allowed
			if (this.MyGameCard != null)
			{
				this.MyGameCard.Velocity = null;
			}
		}

		public override void UpdateCard()
		{
			// Ensure enemy has no pathfinding targets or aggression on the board
			this.IsAggressive = false;
			this.MoveTimer = 0f;
			this.CurrentTarget = null;
			if (this.MyGameCard != null)
			{
				this.MyGameCard.Velocity = null;
			}

			// Handle hover + hold interaction (left mouse / primary input)
			bool isHovered = (WorldManager.instance != null && WorldManager.instance.HoveredCard == this.MyGameCard);
			bool isHolding = isHovered && InputController.instance != null && InputController.instance.GetInput(0);

			if (isHolding)
			{
				HoldProgress += Time.deltaTime;
				if (HoldProgress >= MaxHoldTime)
				{
					HoldProgress = 0f;
					TriggerEngagement();
				}
				else
				{
					// Visual Feedback: Subtle shake and pulse during progress
					float shakeAmount = 0.03f * (HoldProgress / MaxHoldTime);
					if (this.MyGameCard != null)
					{
						this.MyGameCard.transform.position += new Vector3(UnityEngine.Random.Range(-shakeAmount, shakeAmount), 0f, UnityEngine.Random.Range(-shakeAmount, shakeAmount));
						this.MyGameCard.RotWobble(0.2f);
					}
				}
			}
			else
			{
				HoldProgress = Mathf.Max(0f, HoldProgress - Time.deltaTime * 2f);
			}

			base.UpdateCard();
		}

		private void TriggerEngagement()
		{
			if (WorldManager.instance == null || TurnBasedCombatManager.Instance == null) return;

			// Gather all active player cats from the board
			List<Combatable> players = WorldManager.instance.AllCards
				.Where(c => c != null && c.CardData is CatCardData && !c.Destroyed)
				.Select(c => c.CardData as Combatable)
				.ToList();

			// Encounter generation represents this enemy card as a node trigger
			List<Combatable> enemies = new List<Combatable> { this };

			// Pause world simulation and start turn-based combat preparation
			TurnBasedCombatManager.Instance.StartCombat(players, enemies, (result) =>
			{
				if (result == CombatResult.Victory)
				{
					if (this.MyGameCard != null)
					{
						this.MyGameCard.DestroyCard(true, true);
					}
				}
				else
				{
					// Push players away on retreat/defeat
					foreach (var p in players)
					{
						if (p != null && p.MyGameCard != null)
						{
							p.MyGameCard.RemoveFromStack();
							p.MyGameCard.TargetPosition = p.MyGameCard.transform.position + new Vector3(UnityEngine.Random.Range(-2.5f, -1.5f), 0f, UnityEngine.Random.Range(-2.5f, -1.5f));
						}
					}
				}
			});
		}
	}
}
