using System;
using UnityEngine;

public class TimeSystem
{
    private WorldManager _world;

    public float MonthTimer;
    public int OldCurrentMonth;
    public float SpeedUp = 1f;

    // Aliases for Mewtations: Dogma
    public float DayTimer { get { return MonthTimer; } set { MonthTimer = value; } }
    public int CurrentDay { get { return CurrentMonth; } }
    public float DayTime { get { return MonthTime; } }

    public TimeSystem(WorldManager world)
    {
        _world = world;
    }

    public int CurrentMonth
    {
        get
        {
            if (_world.BoardMonths != null)
            {
                return _world.BoardMonths.GetCurrentMonth();
            }
            return 0;
        }
    }

    public float MonthTime
    {
        get
        {
            if (_world.CurrentRunOptions != null)
            {
                if (_world.CurrentRunOptions.MoonLength == MoonLength.Short)
                {
                    return 90f;
                }
                if (_world.CurrentRunOptions.MoonLength == MoonLength.Normal)
                {
                    return 120f;
                }
                if (_world.CurrentRunOptions.MoonLength == MoonLength.Long)
                {
                    return 200f;
                }
            }
            return 120f;
        }
    }

    public void IncrementMonth()
    {
        _world.BoardMonths.IncrementMonth();
        
        // Trigger day events for the new day
        if (_world.DayEvent != null)
        {
            _world.DayEvent.TriggerDayEvent(CurrentDay);
        }
    }

    public void IncrementDay()
    {
        IncrementMonth();
    }
}
