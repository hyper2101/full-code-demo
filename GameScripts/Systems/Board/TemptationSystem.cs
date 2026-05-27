using UnityEngine;

namespace Mewtations.Systems.Board
{
    /// <summary>
    /// Replaces the old Stacklands "Greed" system.
    /// Acts as a metaphysical temptation pressure (Cat God influence, voluntary sacrifice, mutation risk)
    /// rather than a monthly economy tax.
    /// </summary>
    public class TemptationSystem : MonoBehaviour
    {
        private float _currentTemptationLevel;

        public void IncreaseTemptation(float amount)
        {
            _currentTemptationLevel += amount;
            // TODO: Trigger ideological decay or Cat God interaction
        }

        public void ProcessSacrifice()
        {
            // Reset or modify temptation based on voluntary sacrifice
        }
    }
}
