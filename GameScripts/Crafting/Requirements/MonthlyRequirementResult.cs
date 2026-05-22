using System;
using System.Collections.Generic;

public class MonthlyRequirementResult
{
	public MonthlyRequirementResult()
	{
		this.results = new Dictionary<string, MonthlyResult>();
	}

	public Dictionary<string, MonthlyResult> results;
}
