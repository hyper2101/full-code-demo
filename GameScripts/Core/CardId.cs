using System;

public class CardId : ICardId
{
	public string Id { get; set; }

	public CardId(string id)
	{
		this.Id = id;
	}

	public static explicit operator CardId(string s)
	{
		return new CardId(s);
	}
}
