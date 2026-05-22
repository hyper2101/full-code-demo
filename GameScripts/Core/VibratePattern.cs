using System;
using System.Collections.Generic;

public class VibratePattern
{
	public VibratePattern Add(float time, float lf, float hf)
	{
		this.Times.Add(time);
		this.LowFrequencies.Add(lf);
		this.HighFrequencies.Add(hf);
		return this;
	}

	public List<float> Times = new List<float>();

	public List<float> LowFrequencies = new List<float>();

	public List<float> HighFrequencies = new List<float>();
}
