using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conflict
{
	public static Conflict CreateFromSavedConflict(SavedConflict savedConflict)
	{
		Conflict conflict = new Conflict();
		conflict.Id = savedConflict.Id;
		conflict.ConflictStartPosition = savedConflict.StartPosition;
		GameCard cardWithUniqueId = WorldManager.instance.GetCardWithUniqueId(savedConflict.InitiatorCardId);
		if (cardWithUniqueId == null)
		{
			return null;
		}
		conflict.Initiator = cardWithUniqueId.Combatable;
		foreach (string text in savedConflict.InvolvedCards)
		{
			GameCard cardWithUniqueId2 = WorldManager.instance.GetCardWithUniqueId(text);
			if (cardWithUniqueId2 != null)
			{
				conflict.JoinConflict(cardWithUniqueId2.Combatable);
			}
		}
		return conflict;
	}

	public static Conflict StartConflict(Combatable initiator)
	{
		Conflict conflict = new Conflict();
		conflict.Id = Guid.NewGuid().ToString().Substring(0, 10);
		conflict.Initiator = initiator;
		Vector3 position = initiator.MyGameCard.transform.position;
		conflict.ConflictStartPosition = new Vector3(position.x, -position.z * 0.001f, position.z);
		conflict.JoinConflict(initiator);
		return conflict;
	}

	public bool CanLeaveConflict(Combatable b)
	{
		return this.Initiator.Team == b.Team;
	}

	public void JoinConflict(Combatable b)
	{
		if (b.InConflict)
		{
			Debug.LogError(string.Format("{0} is already in a conflict", b));
			return;
		}
		if (this.Participants.Contains(b))
		{
			Debug.LogError(string.Format("{0} is already part of this conflict", b));
			return;
		}
		if (b.MyGameCard.HasChild)
		{
			foreach (Combatable combatable in b.ChildrenMatchingPredicate((CardData x) => x is Combatable).Cast<Combatable>().ToList<Combatable>())
			{
				this.Participants.Add(combatable);
				combatable.MyConflict = this;
				combatable.MyGameCard.RemoveFromStack();
			}
		}
		this.Participants.Add(b);
		b.MyConflict = this;
		b.MyGameCard.RemoveFromStack();
	}

	public void SetParticipantTeamIndex(Combatable a, int index)
	{
		index = Mathf.Clamp(index, 0, this.GetTeamSize(a.Team));
		Combatable participantWithTeamIndex = this.GetParticipantWithTeamIndex(a.Team, index);
		if (participantWithTeamIndex == null)
		{
			return;
		}
		int num = this.Participants.IndexOf(a);
		int num2 = this.Participants.IndexOf(participantWithTeamIndex);
		this.Participants[num] = participantWithTeamIndex;
		this.Participants[num2] = a;
	}

	private Combatable GetParticipantWithTeamIndex(Team team, int teamIndex)
	{
		for (int i = 0; i < this.Participants.Count; i++)
		{
			if (this.Participants[i].Team == team && this.GetIndexInTeam(this.Participants[i]) == teamIndex)
			{
				return this.Participants[i];
			}
		}
		return null;
	}

	public void LeaveConflict(Combatable b)
	{
		this.RemoveParticipant(b);
	}

	public void UpdateConflict()
	{
		this.conflictTime += Time.deltaTime;
		this.TimeSinceLastAttack += Time.deltaTime * WorldManager.instance.TimeScale;
		if (!this.BothTeamsExist())
		{
			this.StopConflict();
		}
		foreach (Conflict conflict in WorldManager.instance.GetAllConflicts())
		{
			if (conflict != this && this.OverlapsWith(conflict))
			{
				Debug.Log("Joined conflicts because of overlap");
				this.JoinWithConflict(conflict);
				break;
			}
		}
		this.UpdateConflictArrows();
		this.UpdateConflictOutline();
		this.PushDraggables();
	}

	public void PushDraggables()
	{
		Bounds bounds = this.GetBounds();
		int num = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, this.hits);
		for (int i = 0; i < num; i++)
		{
			Draggable component = this.hits[i].gameObject.GetComponent<Draggable>();
			if (!(component == null) && component.CanBePushed())
			{
				Draggable draggable = component;
				GameCard gameCard = draggable as GameCard;
				if (gameCard != null)
				{
					draggable = gameCard.GetRootCard();
				}
				Vector3 vector = bounds.center - draggable.TargetPosition;
				vector.y = 0f;
				draggable.TargetPosition -= vector.normalized * 2f * Time.deltaTime;
			}
		}
	}

	private void UpdateConflictArrows()
	{
		bool flag = true;
		if (WorldManager.instance.Time.SpeedUp != 0f)
		{
			flag = false;
		}
		bool flag2 = false;
		using (List<Combatable>.Enumerator enumerator = this.Participants.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.MyGameCard.BeingHovered)
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			this.timeSinceLastHover = 0f;
		}
		else
		{
			this.timeSinceLastHover += Time.deltaTime;
		}
		if (this.timeSinceLastHover < 0.1f)
		{
			flag = false;
		}
		if (flag)
		{
			foreach (Combatable combatable in this.Participants)
			{
				combatable.DrawConflictArrows(true);
			}
		}
	}

	private void UpdateConflictOutline()
	{
		Bounds bounds = this.GetBounds();
		Extensions.Perlin(this.conflictTime * 10f) * 0.01f;
		Vector2 vector = new Vector2(bounds.size.x, bounds.size.z);
		Vector3 center = bounds.center;
		if (!this.init)
		{
			this.init = true;
			this.currentPosition = center;
		}
		this.currentPosition = Vector3.Lerp(this.currentPosition, center, Time.deltaTime * 16f);
		this.currentSize = Vector3.Lerp(this.currentSize, vector, Time.deltaTime * 16f);
		DrawManager.instance.DrawShape(new ConflictRectangle
		{
			Size = this.currentSize,
			Center = this.currentPosition
		});
	}

	private void JoinWithConflict(Conflict otherConflict)
	{
		List<Combatable> list = new List<Combatable>(this.Participants);
		this.StopConflict();
		foreach (Combatable combatable in list)
		{
			otherConflict.JoinConflict(combatable);
		}
	}

	private bool OverlapsWith(Conflict otherConflict)
	{
		Bounds bounds = this.GetBounds();
		Bounds bounds2 = otherConflict.GetBounds();
		return bounds.Intersects(bounds2);
	}

	private void RemoveParticipant(Combatable b)
	{
		if (!this.Participants.Contains(b) || b.MyConflict != this)
		{
			Debug.LogError(string.Format("{0} is not part of this conflict", b));
			return;
		}
		this.Participants.Remove(b);
		b.MyConflict = null;
		b.ExitConflict();
		if (this.Initiator == b && this.Participants.Count > 0)
		{
			if (this.GetTeamSize(b.Team) > 0)
			{
				this.Initiator = this.GetCombatableWithIndexInTeam(b.Team, 0);
			}
			else
			{
				this.Initiator = this.GetCombatableWithIndexInTeam(this.GetOppositeTeam(b.Team), 0);
			}
			if (this.Initiator == null)
			{
				Debug.Log("Initiator is null");
			}
		}
	}

	public void SwapParticipant(Combatable oldParticipant, Combatable newParticipant)
	{
		int num = this.Participants.IndexOf(oldParticipant);
		oldParticipant.MyConflict.JoinConflict(newParticipant);
		oldParticipant.MyConflict.LeaveConflict(oldParticipant);
		this.Participants.Remove(newParticipant);
		this.Participants.Insert(num, newParticipant);
		foreach (Combatable combatable in this.Participants)
		{
			combatable.NotifyParticipantUpdate(oldParticipant, newParticipant);
		}
	}

	public void StopConflict()
	{
		for (int i = this.Participants.Count - 1; i >= 0; i--)
		{
			Combatable combatable = this.Participants[i];
			this.RemoveParticipant(combatable);
		}
	}

	public List<Combatable> GetFriendlyParticipants(Combatable combatable)
	{
		return this.Participants.FindAll((Combatable x) => x.Team == combatable.Team);
	}

	public List<Combatable> GetEnemyParticipants(Combatable combatable)
	{
		return this.Participants.FindAll((Combatable x) => x.Team == this.GetOppositeTeam(combatable.Team));
	}

	public Combatable GetTarget(Combatable b)
	{
		int num;
		int num2;
		this.DetermineTargetRange(b, out num, out num2);
		int num3 = Random.Range(num, num2);
		if (this.GetTeamSize(this.GetOppositeTeam(b.Team)) == 0)
		{
			return null;
		}
		if (b.Team == Team.Player && num2 - num > 1 && Random.value > 0.5f)
		{
			Combatable combatable = this.GetCombatableWithIndexInTeam(this.GetOppositeTeam(b.Team), num);
			for (int i = num; i <= num2 - 1; i++)
			{
				Combatable combatableWithIndexInTeam = this.GetCombatableWithIndexInTeam(this.GetOppositeTeam(b.Team), i);
				if (combatableWithIndexInTeam.HealthPoints < combatable.HealthPoints)
				{
					combatable = combatableWithIndexInTeam;
				}
			}
			num3 = combatable.MyConflict.GetIndexInTeam(combatable);
		}
		return this.GetCombatableWithIndexInTeam(this.GetOppositeTeam(b.Team), num3);
	}

	public List<Combatable> GetCombatableTargets(Combatable b)
	{
		List<Combatable> list = new List<Combatable>();
		int num;
		int num2;
		this.DetermineTargetRange(b, out num, out num2);
		if (this.GetTeamSize(this.GetOppositeTeam(b.Team)) == 0)
		{
			return list;
		}
		for (int i = num; i < num2; i++)
		{
			list.Add(this.GetCombatableWithIndexInTeam(this.GetOppositeTeam(b.Team), i));
		}
		return list;
	}

	private void DetermineTargetRange(Combatable b, out int min, out int max)
	{
		float num = (float)this.GetTeamSize(b.Team);
		float num2 = (float)this.GetIndexInTeam(b);
		float num3 = (float)this.GetTeamSize(this.GetOppositeTeam(b.Team)) / num;
		float num4 = num2 * num3;
		float num5 = (num2 + 1f) * num3;
		min = Mathf.FloorToInt(num4);
		max = Mathf.CeilToInt(num5);
	}

	public int GetIndexInTeam(Combatable b)
	{
		int num = 0;
		foreach (Combatable combatable in this.Participants)
		{
			if (combatable == b)
			{
				return num;
			}
			if (combatable.Team == b.Team)
			{
				num++;
			}
		}
		return -1;
	}

	private Combatable GetCombatableWithIndexInTeam(Team team, int index)
	{
		if (index < 0 || index >= this.GetTeamSize(team))
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = 0;
		foreach (Combatable combatable in this.Participants)
		{
			if (combatable.Team == team && index == num)
			{
				return combatable;
			}
			if (combatable.Team == team)
			{
				num++;
			}
		}
		return null;
	}

	private Team GetOppositeTeam(Team team)
	{
		if (team == Team.Enemy)
		{
			return Team.Player;
		}
		return Team.Enemy;
	}

	public int GetTeamSize(Team team)
	{
		int num = 0;
		using (List<Combatable>.Enumerator enumerator = this.Participants.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Team == team)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool BothTeamsExist()
	{
		return this.GetTeamSize(Team.Player) > 0 && this.GetTeamSize(Team.Enemy) > 0;
	}

	public Bounds GetBounds()
	{
		float combatOffset = WorldManager.instance.CombatOffset;
		return new Bounds(this.ClampStartPosition(this.ConflictStartPosition) - new Vector3(0f, 0f, combatOffset) * 0.5f, this.GetConflictSize());
	}

	private Vector3 GetConflictSize()
	{
		float num = (float)Mathf.Max(this.GetTeamSize(Team.Player), this.GetTeamSize(Team.Enemy)) * WorldManager.instance.HorizonalCombatOffset;
		float height = this.Initiator.MyGameCard.GetHeight();
		float combatOffset = WorldManager.instance.CombatOffset;
		return new Vector3(num + WorldManager.instance.ConflictWidthIncrease, 0.05f, height + combatOffset + WorldManager.instance.ConflictHeightIncrease);
	}

	public static float GetConflictHeight()
	{
		return WorldManager.instance.CombatOffset + GameCard.CardHeight;
	}

	private Vector3 ClampStartPosition(Vector3 p)
	{
		Vector3 conflictSize = this.GetConflictSize();
		float num = conflictSize.x * 0.5f;
		float num2 = conflictSize.z * 0.5f;
		Bounds tightWorldBounds = this.Initiator.MyGameCard.MyBoard.TightWorldBounds;
		float num3 = 0.1f;
		p.x = Mathf.Clamp(p.x, tightWorldBounds.min.x + num + num3, tightWorldBounds.max.x - num - num3);
		p.z = Mathf.Clamp(p.z, tightWorldBounds.min.z + num2 * 0.5f + num3 + WorldManager.instance.CombatOffset, tightWorldBounds.max.z + num2 * 0.5f - num3);
		return p;
	}

	public Vector3 GetPositionInConflict(Combatable b)
	{
		float num = (float)this.GetTeamSize(b.Team);
		float num2 = (float)this.GetIndexInTeam(b) - (num - 1f) * 0.5f;
		Vector3 vector = new Vector3(num2 * WorldManager.instance.HorizonalCombatOffset, 0f, 0f);
		Vector3 vector2 = this.ClampStartPosition(this.ConflictStartPosition);
		if (this.Initiator.Team != b.Team)
		{
			return vector2 + vector;
		}
		return vector2 + vector + new Vector3(0f, 0f, -WorldManager.instance.CombatOffset);
	}

	public List<Combatable> Participants = new List<Combatable>();

	public Combatable Initiator;

	public Vector3 ConflictStartPosition;

	public string Id = "";

	public float TimeSinceLastAttack;

	private float conflictTime;

	private float timeSinceLastHover;

	private Collider[] hits = new Collider[20];

	private bool init;

	private Vector3 currentPosition;

	private Vector2 currentSize;
}
