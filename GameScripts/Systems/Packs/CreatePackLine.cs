using System;
using System.Collections.Generic;
using UnityEngine;

public class CreatePackLine : MonoBehaviour
{
	public void CreateBoosterBoxes(List<string> boosters, BoardCurrency currency)
	{
		Object.Instantiate<SellBox>(PrefabManager.instance.SellBoxPrefab).transform.SetParent(base.transform, true);
		foreach (string text in boosters)
		{
			BoosterpackData boosterData = WorldManager.instance.GetBoosterData(text);
			if (!(boosterData == null))
			{
				BuyBoosterBox buyBoosterBox = Object.Instantiate<BuyBoosterBox>(PrefabManager.instance.BoosterBoxPrefab);
				buyBoosterBox.BoosterId = text;
				buyBoosterBox.Cost = boosterData.Cost;
				buyBoosterBox.BoardCurrency = currency;
				buyBoosterBox.transform.SetParent(base.transform, true);
				WorldManager.instance.AllBoosterBoxes.Add(buyBoosterBox);
			}
		}
		this.SetPositions();
		this.TotalWidth = (float)boosters.Count * this.Distance + 0.375f;
	}

	private void SetPositions()
	{
		int num = 0;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < base.transform.childCount; j++)
		{
			Transform child = base.transform.GetChild(j);
			if (child.gameObject.activeInHierarchy)
			{
				float num3 = (float)num2 * this.Distance - (float)(num - 1) * this.Distance * 0.5f;
				child.localPosition = new Vector3(num3, 0f, 0f);
				num2++;
			}
		}
	}

	public float Distance;

	public float TotalWidth;
}
