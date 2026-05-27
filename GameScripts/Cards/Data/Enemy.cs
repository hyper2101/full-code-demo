using System;
using UnityEngine;

namespace Mewtations.Legacy.Stacklands
{
	[Obsolete("Legacy combat entity. Transitioning to CombatV2.", false)]
	public class Enemy : Mob
	{
		protected override void Move()
		{
			Vector3 vector;
			if (this.CurrentTarget != null)
			{
				vector = base.transform.position - this.CurrentTarget.transform.position;
				vector.y = 0f;
				vector.Normalize();
				Debug.DrawLine(base.transform.position, base.transform.position - vector, Color.red, 1f);
				vector = this.Wiggle(vector, 45f);
				Debug.DrawLine(base.transform.position, base.transform.position - vector, Color.green, 1f);
				vector = -vector * 4f;
			}
			else if (WorldManager.instance.CurrentBoard.Id == "cities")
			{
				vector = Vector3.zero;
			}
			else
			{
				Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * 4f;
				vector = new Vector3(vector2.x, 0f, vector2.y);
			}
			this.MyGameCard.Velocity = new Vector3?(new Vector3(vector.x, 0f, vector.z));
		}

		public override void UpdateCard()
		{
			CardData cardData2;
			if (this.Id == "wolf")
			{
				CardData cardData;
				if (base.HasCardOnTop("bone", out cardData))
				{
					cardData.MyGameCard.DestroyCard(false, true);
					this.MyGameCard.DestroyCard(false, true);
					WorldManager.instance.CreateCard(base.transform.position, "dog", true, false, true);
				}
			}
			else if (this.Id == "feral_cat" && base.HasCardOnTop("milk", out cardData2) && WorldManager.instance.IsSpiritDlcActive())
			{
				cardData2.MyGameCard.DestroyCard(false, true);
				this.MyGameCard.DestroyCard(false, true);
				WorldManager.instance.CreateCard(base.transform.position, "cat", true, false, true);
			}
			base.UpdateCard();
		}

		private Vector3 Wiggle(Vector3 vec, float angle)
		{
			return Quaternion.AngleAxis(UnityEngine.Random.Range(-angle, angle), Vector3.up) * vec;
		}
	}
}
