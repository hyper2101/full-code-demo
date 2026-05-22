using System;
using UnityEngine.InputSystem;

public class ControllerVibrator
{
	public void StartVibrate(VibratePattern pattern)
	{
		this.CurrentPattern = pattern;
		this.curVibrateTimer = 0f;
		this.curVibrateIndex = 0;
		Gamepad current = Gamepad.current;
		if (current == null)
		{
			return;
		}
		current.SetMotorSpeeds(this.CurrentPattern.LowFrequencies[this.curVibrateIndex], this.CurrentPattern.HighFrequencies[this.curVibrateIndex]);
	}

	public void UpdateVibrate(float deltaTime)
	{
		if (this.CurrentPattern == null)
		{
			return;
		}
		if (InputController.instance.CurrentScheme != ControlScheme.Controller || Gamepad.current == null)
		{
			this.StopVibrate();
			return;
		}
		Gamepad current = Gamepad.current;
		if (current != null)
		{
			current.SetMotorSpeeds(this.CurrentPattern.LowFrequencies[this.curVibrateIndex], this.CurrentPattern.HighFrequencies[this.curVibrateIndex]);
		}
		this.curVibrateTimer += deltaTime;
		if (this.curVibrateTimer >= this.CurrentPattern.Times[this.curVibrateIndex])
		{
			this.curVibrateTimer -= this.CurrentPattern.Times[this.curVibrateIndex];
			this.curVibrateIndex++;
			if (this.curVibrateIndex >= this.CurrentPattern.Times.Count)
			{
				this.StopVibrate();
			}
		}
	}

	public void StopVibrate()
	{
		this.CurrentPattern = null;
		Gamepad current = Gamepad.current;
		if (current == null)
		{
			return;
		}
		current.SetMotorSpeeds(0f, 0f);
	}

	public VibratePattern CurrentPattern;

	public int curVibrateIndex;

	private float curVibrateTimer;
}
