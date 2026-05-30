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

## 3. L? h?ng Dependency (Missing Packages & Assets)

- [ ] **UnityEngine.InputSystem:** Code s? d?ng UnityEngine.InputSystem. N?u dua v�o project Unity m?i, b?t bu?c ph?i c�i d?t package Input System t? Unity Package Manager, n?u kh�ng s? b�o l?i Compile.
- [ ] **UnityEngine.Rendering.PostProcessing:** Tuong t?, c?n c�i d?t package Post Processing t? Unity Package Manager d? tr�nh l?i bi�n d?ch.
- [ ] **Newtonsoft.Json:** Project c� g?i Newtonsoft.Json. M?c d� mi?n ph�, nhung Unity m?c d?nh kh�ng t�ch h?p s?n (Tr? khi d�ng package com.unity.modules.jsonserialize ho?c c�i th�m DLL b�n ngo�i).
- [ ] **HarmonyLib:** Thu vi?n d�ng d? Mod game. Hi?n t?i thi?u file DLL c?a HarmonyLib trong project, s? g�y l?i bi�n d?ch ? c�c script li�n quan d?n h? th?ng Mod. (C� th? xem x�t x�a b? n?u kh�ng l�m Mod).
