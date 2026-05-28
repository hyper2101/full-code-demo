using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Cards.Data
{
    public class ScalingPassiveTalisman : CardData
    {
        public override bool IsPassiveTalisman => true;
        
        public override void UpdateCard()
        {
            this.nameOverride = "Thiên Phú: Sát Ý Khởi Nguyên";
            this.descriptionOverride = "Thiên phú đặc biệt: Kể từ Turn thứ 6 của trận chiến, mỗi turn trôi qua tăng thêm 5% sát thương đầu ra (Cộng dồn).";
            
            if (this.MyGameCard != null)
            {
                this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.BlueprintIconFilled; // Tạm dùng icon này
                this.MyGameCard.ShowSpecialIcon = true;
            }
            
            base.UpdateCard();
        }
        
        protected override bool CanHaveCard(CardData otherCard)
        {
            return false;
        }
    }
}
