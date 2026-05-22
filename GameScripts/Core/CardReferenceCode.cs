using System;

public class CardReferenceCode : ICardReference
{
	public string ReferencedCardId { get; set; }

	public CardReferenceCode(string cardId)
	{
		this.ReferencedCardId = cardId;
	}

	public override string ToString()
	{
		return "from code";
	}

	public string GetKey()
	{
		return "code_" + this.ReferencedCardId;
	}
}
