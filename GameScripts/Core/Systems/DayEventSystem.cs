using System;
using System.Collections.Generic;
using UnityEngine;

public class DayEventSystem
{
    private WorldManager _world;

    public DayEventSystem(WorldManager world)
    {
        _world = world;
    }

    public bool HasActiveEvent()
    {
        if (_world.AllCards == null) return false;

        foreach (GameCard card in _world.AllCards)
        {
            if (card != null && card.CardData != null && card.CardData.IsEventCard)
            {
                return true;
            }
        }
        return false;
    }

    public void TriggerDayEvent(int day)
    {
        // Ví dụ sự kiện chu kỳ ngày
        if (day == 5)
        {
            // Debug.Log("Thú Triều Event!");
            // _world.CreateCard(_world.CurrentBoard.MiddleOfBoard(), "event_beast_tide", true, true, true);
        }
        else if (day == 15)
        {
            // Debug.Log("Thiên Kiếp Event!");
            // _world.CreateCard(_world.CurrentBoard.MiddleOfBoard(), "event_tribulation", true, true, true);
        }
    }
}
