using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat.Encounters
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance;

        private Dictionary<int, EncounterData> _activeEncounters = new Dictionary<int, EncounterData>();
        private int _nextEncounterId = 1;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public int RegisterEncounter(EncounterData data)
        {
            int id = _nextEncounterId++;
            data.Id = id;
            _activeEncounters[id] = data;
            return id;
        }

        public EncounterData GetEncounter(int id)
        {
            if (_activeEncounters.TryGetValue(id, out EncounterData data))
            {
                return data;
            }
            return null;
        }

        public void RemoveEncounter(int id)
        {
            if (_activeEncounters.ContainsKey(id))
            {
                _activeEncounters.Remove(id);
            }
        }
    }
}
