using UnityEngine;

namespace Mewtations.Legacy.Stacklands
{
    public class RefinementMaterialCardData : CardData
    {
        [ExtraData("material_tier")]
        public int MaterialTier = 1; // 1 = Mortal, 2 = Earth, 3 = Heaven, 4 = Saint, 5 = Immortal

        public override void UpdateCard()
        {
            base.UpdateCard();
            
            // Format string using TSV logic
            if (this.MyGameCard != null)
            {
                string descKey = SokLoc.Translate("refinement_material_description");
                this.descriptionOverride = string.Format(descKey, MaterialTier);
            }
        }
    }
}
