using System;

public struct SubprintMatchInfo
{
	public SubprintMatchInfo(int matchedAt, int matchCount)
	{
		this.FullyMatchedAt = matchedAt;
		this.MatchCount = matchCount;
	}

	public int FullyMatchedAt;

	public int MatchCount;
}
