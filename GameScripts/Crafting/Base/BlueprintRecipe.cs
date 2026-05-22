using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlueprintRecipe : Blueprint
{
	public override void Init(GameDataLoader loader)
	{
		this.NeedsExactMatch = false;
		this.PopulateSubprints();
		base.Init(loader);
	}

	private void PopulateSubprints()
	{
		this.Subprints.Clear();
		string[] array = new string[] { "campfire", "stove" };
		float[] array2 = new float[] { 1f, 0.3f };
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			List<string> list = new List<string>();
			list.Add(text);
			list.AddRange(this.Ingredients);
			this.Subprints.Add(new Subprint
			{
				StatusTerm = CardData.CardToTermId(this) + "_status_0",
				ExtraResultCards = this.ResultItems.ToArray<string>(),
				RequiredCards = list.ToArray(),
				Time = this.CookingTime * array2[i]
			});
		}
	}

	[Header("Recipe")]
	public string[] Ingredients;

	public string[] ResultItems;

	public float CookingTime;
}
