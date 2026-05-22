using System;

public class CardReferenceCard : ICardReference
{
	public string ReferencedCardId { get; set; }

	public CardReferenceCard(string referencedCardId, string originCardId)
	{
		this.OriginCardId = originCardId;
		this.ReferencedCardId = referencedCardId;
	}

	public override string ToString()
	{
		return "card " + this.OriginCardId;
	}

	public string GetKey()
	{
		return "card_" + this.OriginCardId + "_" + this.ReferencedCardId;
	}

	public string OriginCardId;
}
