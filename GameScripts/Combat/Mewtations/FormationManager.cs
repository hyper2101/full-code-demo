using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat
{
    public class FormationManager
    {
        public List<CombatUnit> PlayerUnits = new List<CombatUnit>();
        public List<CombatUnit> EnemyUnits = new List<CombatUnit>();

        public void SetupPlayerTeam(List<Combatable> cats)
        {
            PlayerUnits.Clear();
            // Automatically assign cats to 3 front and then 3 back slots
            for (int i = 0; i < cats.Count && i < 6; i++)
            {
                PlayerUnits.Add(new CombatUnit(cats[i], true, i));
            }
        }

        public void SetupEnemyTeam(List<Combatable> enemies)
        {
            EnemyUnits.Clear();
            // Assign enemies to 3 front and then 3 back slots
            for (int i = 0; i < enemies.Count && i < 6; i++)
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
            // Grid arrangement:
            // 3 front (0, 1, 2), 3 back (3, 4, 5)
            // Left side is Player, Right side is Enemy
            float xOffset = isPlayer ? -200f : 200f;
            float rowOffset = (slotIndex >= 3) ? (isPlayer ? -120f : 120f) : 0f;

            int col = slotIndex % 3;
            float yPos = (col - 1) * 110f; // centered at y=0, spacing 110f

            return new Vector2(xOffset + rowOffset, yPos);
        }
    }
}
