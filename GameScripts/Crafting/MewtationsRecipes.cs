using System.Collections.Generic;
using UnityEngine;

public class MewtationsRecipes : MonoBehaviour
{
    private void Start()
    {
        // Hệ thống Stacklands thông thường load Blueprint từ ScriptableObjects
        // Trong prototype, ta có thể đăng ký động các Subprint (công thức) vào một Blueprint có sẵn.
        
        RegisterPrototypeRecipes();
    }

    private void RegisterPrototypeRecipes()
    {
        // Recipe 1: Chế tạo Linh Đan Hồi Sinh
        // Yêu cầu: 1 Lò luyện đan (smelter/campfire) + 2 Thảo dược (herb) + 1 Đá quý (gem)
        // Kết quả: item_revive_pill
        
        // Recipe 2: Hồi sinh Mèo
        // Yêu cầu: 1 Bàn thờ (altar/temple) + 1 Xác mèo (cat_corpse) + 1 Linh Đan Hồi Sinh (item_revive_pill)
        // Kết quả: cat_basic (sẽ giữ stat từ corpse)

        // Recipe 3: Nấu Thức Ăn Đặc Biệt (mở Ultimate)
        // Yêu cầu: 1 Bếp (stove/campfire) + 1 Thịt (raw_meat) + 1 Thảo dược (herb)
        // Kết quả: item_ultimate_food

        // Lưu ý: Các ID thực tế cần được khởi tạo ScriptableObject và gắn vào thư viện Blueprint của game.
        // Script này đóng vai trò placeholder cho việc setup dữ liệu trong Editor sau này.
        Debug.Log("[Mewtations] Đã load các công thức chế tạo mẫu.");
    }
}
