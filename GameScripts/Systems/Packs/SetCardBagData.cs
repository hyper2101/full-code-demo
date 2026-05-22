using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SetCardBag", menuName = "ScriptableObjects/SetCardBag", order = 1)]
public class SetCardBagData : ScriptableObject
{
	public virtual bool IsActive()
	{
		return this.Filter == null || this.Filter.IsMet();
	}

	public SetCardBagType SetCardBagType;

	public List<SimpleCardChance> Chances;

	[SerializeReference]
	public VariableFilter Filter;
}
