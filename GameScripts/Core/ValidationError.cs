using System;
using UnityEngine;

public class ValidationError
{
	public ValidationError(Object context, string error, string category)
	{
		this.Context = context;
		this.Error = error;
		this.Category = category;
	}

	public Object Context;

	public string Error;

	public string Category;
}
