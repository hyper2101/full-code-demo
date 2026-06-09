using UnityEngine;
using Mewtations.Core;

namespace Mewtations.Systems.Alchemy
{
    public class AlchemyFurnaceCardData : CardData
    {
        [Header("Furnace Properties")]
        public int Tier = 1;

        [SerializeField]
        private AlchemyFurnaceRuntime _runtime;
        public AlchemyFurnaceRuntime Runtime => _runtime;

        protected override void Awake()
        {
            base.Awake();
            _runtime = GetComponent<AlchemyFurnaceRuntime>();
            if (_runtime == null)
            {
                _runtime = gameObject.AddComponent<AlchemyFurnaceRuntime>();
            }
        }

        public override bool DetermineCanHaveCardsWhenIsRoot => false; // Container Entity

        protected override bool CanHaveCard(CardData otherCard)
        {
            return false; // Prevent stack
        }
    }
}
