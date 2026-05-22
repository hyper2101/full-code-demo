using System;
using System.Collections.Generic;
using UnityEngine;

public class ValidationResult
{
	public void AddError(Object context, string error, string category)
	{
		this.errors.Add(new ValidationError(context, error, category));
	}

	public List<ValidationError> errors = new List<ValidationError>();

	public string ValidationId = "";
}
