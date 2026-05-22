using System;
using System.Collections.Generic;

public class CardIdWithEquipment : ICardId
{
	public string Id { get; set; }

	public CardIdWithEquipment(string id, List<string> equipment)
	{
		this.Id = id;
		this.Equipment = equipment;
	}

	public List<string> Equipment = new List<string>();
}
