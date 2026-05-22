using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatCorpseData : CardData
{
    [Header("Corpse Data")]
    [ExtraData("original_cat_id")]
    public string OriginalCatId; // Lưu ID thẻ mèo gốc để biết sẽ hồi sinh thành loại mèo nào

    [ExtraData("original_cat_name")]
    public string OriginalCatName; // Tên của mèo đã chết

    [ExtraData("original_cat_role")]
    public CatRole OriginalCatRole;

    [ExtraData("original_cat_element")]
    public CatElement OriginalCatElement;

    // TODO: Lưu thêm các chỉ số phụ và trạng thái đột phá để có thể khôi phục 100% sức mạnh

    public override bool DetermineCanHaveCardsWhenIsRoot
    {
        get
        {
            return true; // Có thể nhận thẻ bài khác lên trên (ví dụ thẻ Hồi Sinh)
        }
    }

    public override bool CanHaveCard(CardData otherCard)
    {
        // Có thể ghép với Linh Đan Hồi Sinh, hoặc thẻ Tế Lễ
        if (otherCard.Id == "item_revive_pill")
        {
            return true;
        }
        return base.CanHaveCard(otherCard);
    }
}
