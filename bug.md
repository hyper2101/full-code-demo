# Danh Sách Bug Gameplay Cần Xử Lý (Mewtations: Dogma)

Dưới đây là danh sách các lỗi gameplay (bao gồm lỗi do thiết kế cũ chưa được loại bỏ hoàn toàn và các lỗi trong code) được tổng hợp từ việc đối chiếu với GDD và quét mã nguồn hiện tại. Hãy đánh dấu (x) vào các mục đã hoàn thiện để theo dõi.

## 1. Lỗi Xung Đột GDD & Tàn Dư Legacy (Legacy Leaks & GDD Discrepancies)

- [x] **Lỗi Thiếu Áp lực Thám hiểm (Expedition Tension):** Vòng lặp thám hiểm hiện tại chưa tạo được cảm giác rủi ro (risk/reward). Cần áp dụng cơ chế mất 50% đồ trong rương khi rút lui thất bại (Theo GDD mục 8.5).
- [ ] **Lỗi Cơ Chế Sinh Tồn & Kinh Tế Vô Hạn:** Vẫn còn tàn dư của Stacklands (tài nguyên tự mọc lại, khai thác không giới hạn). Cần xóa bỏ các nguồn tài nguyên vô hạn và áp dụng cơ chế khan hiếm, cạn kiệt.
- [x] **Lỗi Thể lực & Chấn thương (Stamina & Fatigue):** Mèo chưa có hệ thống kiệt sức (Stamina chính và Stamina kiệt sức) theo GDD mục 5.3. Mèo hiện tại vẫn đang bị mô phỏng theo dạng "đói bụng" của Stacklands.
- [ ] **Lỗi Đột Biến (Mutation) Chỉ Làm Tăng Chỉ Số:** Mèo đột biến hiện tại chỉ được buff chỉ số nhẹ (passive modifiers) thay vì thay đổi hướng build và tạo ra role rõ ràng (Theo GDD mục 5.2).
- [x] **Tàn Dư Combat Thời Gian Thực (Real-time Combat Ghost):** Các vòng lặp tự động đánh (timer), tự động hiện mũi tên tấn công trong `Combatable.cs` vẫn còn, dù đã chuyển sang mô hình Turn-based. Cần dọn dẹp triệt để các mã bị đánh dấu `[Obsolete]` để tránh lỗi sai lượt.

## 2. Các Bug Trong Mã Nguồn (Code-level Bugs & TODOs)

- [x] **Chưa Khóa Toàn Bộ Nhiệm Vụ Cũ:** Lỗi rò rỉ quest của Stacklands làm phá vỡ tiến trình (Nằm tại `BlueprintSanitizer.cs:31` - `// TODO: Intercept Stacklands quest unlocks`). Cần chặn triệt để.
- [ ] **Chưa Kích Hoạt Hệ Thống Temptation / Dogma:** Lỗi logic tương tác phe phái. (`TemptationSystem.cs:17` - `// TODO: Trigger ideological decay or Cat God interaction`). Hậu quả của hệ tư tưởng đang không hoạt động.
- [x] **Lỗi Hiển thị UI Lượng Tài nguyên:** (ĐÃ LOẠI BỎ - ĐÂY LÀ LEGACY CODE CỦA HỆ THỐNG ENERGY KHÔNG CÒN SỬ DỤNG)
- [ ] **Nhiều Hàm Chưa Được Triển Khai (NotImplementedException):** Mã nguồn vẫn còn gọi nhiều exception chưa hoàn thiện (ví dụ ở `GameCard.cs`, `BoardMonths.cs`, `CardBag.cs`, `GameDataLoader.cs`). Cần xác định các hàm này thuộc hệ thống cũ để xóa đi hoặc bổ sung code mới nếu cần thiết.

## 3. Lỗ hổng Dependency (Missing Packages & Assets)

- [ ] **UnityEngine.InputSystem:** Code sử dụng UnityEngine.InputSystem. Nếu đưa vào project Unity mới, bắt buộc phải cài đặt package Input System từ Unity Package Manager, nếu không sẽ báo lỗi Compile.
- [ ] **UnityEngine.Rendering.PostProcessing:** Tương tự, cần cài đặt package Post Processing từ Unity Package Manager để tránh lỗi biên dịch.
- [ ] **Newtonsoft.Json:** Project có gọi Newtonsoft.Json. Mặc dù miễn phí, nhưng Unity mặc định không tích hợp sẵn (Trừ khi dùng package com.unity.modules.jsonserialize hoặc cài thêm DLL bên ngoài).
- [ ] **HarmonyLib:** Thư viện dùng để Mod game. Hiện tại thiếu file DLL của HarmonyLib trong project, sẽ gây lỗi biên dịch ở các script liên quan đến hệ thống Mod. (Có thể xem xét xóa bỏ nếu không làm Mod).

## 4. Bug Tính năng Miệng Thần Mèo (Cat God Mouth)

- [ ] **Mở Pack Thưởng Gây NullReferenceException:** Sau khi đánh bại `mob_void_spirit`, phần thưởng trả về là `godcat_pack`. Khi mở pack, `GodCatPackCard.cs` gọi lệnh sinh thẻ `item_low_spirit_stone`, nhưng ID này **không hề tồn tại** trong game (chỉ có class `LowSpiritStone`, thiếu ID `"item_low_spirit_stone"` đăng ký trong dữ liệu). Điều này gây lỗi crash không mở được phần thưởng.
- [ ] **Xung Đột Tọa Độ Spawn Gây Phá Hủy Đồ (Stacking Error):** Trong `CatGodMouth.cs`, khi % Báng bổ (Blasphemy) đạt 40% đồng thời với việc Nghi lễ hoàn thành (Devotion đủ), cả Enemy (`mob_void_spirit`) và Pack thưởng (`godcat_pack`) đều được lệnh spawn **tại cùng 1 tọa độ** (`Vector3.back * 1.5f`). Do cơ chế của engine game, quái vật sẽ rơi vào stack với Pack Thưởng và lập tức phá hủy nó do Pack không có máu, khiến người chơi mất trắng đồ trước cả khi đánh quái.
- [ ] **Ghi Đè Dialogue UI:** Cũng trong trường hợp kích hoạt kép (Báng bổ 40% và Devotion đủ), hàm `DialogueSystem.Instance.StartDialogue` bị gọi 2 lần liên tiếp trong cùng 1 frame. Kết quả là bảng "TÀ THẦN PHẪN NỘ" bị ghi đè ngay lập tức bởi bảng "NGHI LỄ HOÀN THÀNH". Người chơi bấm "Tiếp nhận" đồ nhưng khi tắt bảng sẽ bất ngờ bị Enemy cắn.
