using System;
using System.Collections.Generic;
using UnityEngine;

using Mewtations.Combat;

// TURN-BASED CORE SYSTEM
// DO NOT REMOVE DURING LEGACY COMBAT CLEANUP
namespace Mewtations.Combat.Battlefield
{
    public class FormationManager
    {
        public List<CombatUnit> PlayerUnits = new List<CombatUnit>();
        public List<CombatUnit> EnemyUnits = new List<CombatUnit>();

        public void SetupPlayerTeam(List<Combatable> cats)
        {
            PlayerUnits.Clear();
            // Automatically assign up to 5 cats to front/mid rows as default placement
            for (int i = 0; i < cats.Count && i < 5; i++)
            {
                PlayerUnits.Add(new CombatUnit(cats[i], true, i));
            }
        }

        public void SetupEnemyTeam(List<Combatable> enemies)
        {
            EnemyUnits.Clear();
            // Assign enemies up to 9 slots
            for (int i = 0; i < enemies.Count && i < 9; i++)
            {
                EnemyUnits.Add(new CombatUnit(enemies[i], false, i));
            }
        }

        public bool IsPlayerDefeated()
        {
            return PlayerUnits.FindAll(u => u.IsAlive).Count == 0;
        }

        public bool IsEnemyDefeated()
        {
            return EnemyUnits.FindAll(u => u.IsAlive).Count == 0;
        }

        /// <summary>
        /// Gets rendering world or local offsets for UI visualization
        /// </summary>
        public static Vector2 GetSlotUiPosition(bool isPlayer, int slotIndex)
        {
            // Grid arrangement: 3x3 (9 slots: 0 to 8)
            // Row 0 (Front): 0, 1, 2
            // Row 1 (Mid):   3, 4, 5
            // Row 2 (Back):  6, 7, 8
            int row = slotIndex / 3;
            int col = slotIndex % 3;

            // Player on Left, Enemy on Right
            // Row 0 is closest to the center, Row 2 is furthest away
            float xOffset;
            if (isPlayer)
            {
                xOffset = -120f - row * 115f;
            }
            else
            {
                xOffset = 120f + row * 115f;
            }

            float yPos = (col - 1) * 115f; // centered at y=0, spacing 115f

            return new Vector2(xOffset, yPos);
        }
    }
}
