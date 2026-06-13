# Mewtations: Dogma — Hạng mục Công việc & Lỗi Cần Xử Lý (Tasks & Bugs)

Tài liệu này là nơi tập trung theo dõi các khoảng trống thiết kế gameplay (gaps), các hạng mục tàn dư chưa dọn dẹp xong, và toàn bộ bug phát hiện trong mã nguồn. Hãy đánh dấu (x) vào các mục đã hoàn thiện để theo dõi tiến độ dự án.

---

## 1. Trạng Thái Hệ Thống Hiện Tại (Current Project State)

### Kiến Trúc Đã Làm Tốt (Architecture)
- **Combat authority** đã tách rời hoàn toàn khỏi combat thời gian thực của Stacklands.
- `Combatable` đã được chuyển sang chế độ passive.
- `WorldManager` giảm phụ thuộc (dependency) legacy rất nhiều.
- Các hệ thống legacy cũ đã bị cách ly (quarantine) bằng Hooks & RuntimeFlags.
- Quyền sở hữu runtime (Runtime ownership) rõ ràng hơn.
- Loại bỏ các combat coroutine ẩn.
- Vòng lặp tấn công thời gian thực (Realtime attack flow) không còn hoạt động.

### Nền Tảng Kỹ Thuật (Technical Foundation)
- Nền tảng combat theo lượt (Turn-based combat foundation) hoạt động tốt.
- Hệ thống Save/Load nền tảng đã sẵn sàng.
- Vòng lặp thời gian và Moon cycle đã có framework hỗ trợ.
- Tactical combat đang dần hoàn thiện.

---

## 2. Khoảng Trống Thiết Kế & Hướng Phát Triển (Gameplay Gaps)

### 🔴 2.1. Nâng Cao Áp Lực Viễn Chinh (Expedition Tension)
Hiện tại game đã có combat, map/board, và hệ thống thám hiểm cơ bản, nhưng cần làm rõ rệt hơn cảm giác mạo hiểm:
- [x] **Encounter System cho Miệng Thần Mèo:** Đã tích hợp ThreatCardComponent và dùng tạm DogTaxEncounter thay thế cơ chế gọi quái vật lý. Đã dọn dẹp sạch `CatGodAnger` và `mob_void_spirit`.
- [x] **Special Map Pool (Viễn Chinh):** Đã sửa xử lý node SpecialMap để không gây kẹt.
- [ ] **LootProfile cho Thương Nhân:** Cần cấu hình LootProfile ngẫu nhiên theo giá trị giao dịch cho Camp Merchant thay vì gán cứng 1 Food/1 Gold như hiện tại.
- [x] **Debt Note tại Bệnh Viện Chó:** Đã tích hợp `AddDebtAmount` để tránh spam giấy nợ, gọi đúng qua hệ thống DogTax.
- [x] **Localization Hồi Ký (Memoir):** Đã chuyển toàn bộ text cứng (trả về, kiệt sức, bệnh viện) sang Localization ID.
- [x] **Reset Timer Đột Phá:** Đã sửa logic Timer trận pháp, tự động reset dựa vào số lượng thẻ đang stack (`_lastChildCount`).
- [x] **Sửa Lại Workflow Rút Lui (ReturnToBase):** Đã gỡ bỏ lệnh `RemoveFromStack()` để không phá vỡ hoạt động tại nhà khi mèo đi Viễn chinh.
- [ ] **Rủi ro khi Rút lui:** Cần áp dụng cơ chế mất 50% đồ trong rương khi rút lui thất bại hoặc Party Wipe (chỉ giữ lại đồ trong Insured Slots từ index 0 đến InsuredSlots - 1).

### 🔴 2.2. Đột Biến (Mutation) Chưa Phản Ánh Trực Tiếp Lối Chơi
Mutation cần đóng vai trò là "Identity" của game thay vì chỉ tăng chỉ số passive nhẹ:
- [ ] Thiết kế cơ chế tradeoff cho đột biến (ví dụ: tăng sức mạnh nhưng tích tụ thêm Corruption, giảm Stability).
- [ ] Tạo các hướng build nhân vật khác biệt rõ rệt dựa trên các loại đột biến đặc biệt.
- [ ] Liên kết đột biến với tương tác phe phái (Faction/Social reaction).

### 🔴 2.3. Sự Gắn Kết Cá Nhân với Mèo (Individuality & Memoir)
- [x] **Hệ thống Hồi ký (Memoir System):** Ghi lại lịch sử chiến đấu, đột phá và sự kiện riêng biệt để mèo không chỉ là worker vô tri.
- [x] Xây dựng ảnh hưởng của chấn thương dài hạn (Long-term injury) và kiệt sức (Fatigue/Stamina). *(Đã tích hợp qua IsExhausted/IsParalyzed)*
- [ ] Tạo tính cá nhân hóa độc nhất thông qua các kỹ năng hỗ trợ đặc trưng trong combat.

### 🔴 2.4. Xác Định Vai Trò Trại Chính (Settlement Identity)
Cần làm rõ Settlement hoạt động theo hướng nào:
- **Hướng A:** Vùng an toàn để phục hồi (heal, craft, chuẩn bị expedition, tế đàn).
- **Hướng B:** Vùng sinh tồn liên tục (thiếu thốn thức ăn, khủng hoảng nội bộ, áp lực từ các phe phái Chó).

### 🔴 2.5. Xác Định Trọng Tâm Tiến Trình (Progression Direction)
- Cần thống nhất cốt lõi giúp người chơi mạnh lên: Qua đột biến, nghi lễ tế thần, trang bị hay tầm ảnh hưởng phe phái? Tránh phân mảnh hệ thống.

---

## 3. Danh Sách Lỗi Kỹ Thuật & TODOs (Bugs & Source Code Errors)

### 🐛 3.1. Lỗi Xung Đột Hệ Thống Cũ (Legacy Leaks & Obsolete Bugs)
- [ ] **Cơ Chế Sinh Tồn & Kinh Tế Vô Hạn:** Vẫn còn tàn dư của Stacklands (tài nguyên tự mọc lại, khai thác không giới hạn). Cần xóa bỏ các nguồn tài nguyên vô hạn và áp dụng cơ chế khan hiếm, cạn kiệt.
- [x] **Tàn Dư Combat Thời Gian Thực:** Các vòng lặp tự động đánh (timer), tự động hiện mũi tên tấn công trong `Combatable.cs` vẫn còn, dù đã chuyển sang mô hình Turn-based. Đã dọn dẹp các mã bị đánh dấu `[Obsolete]` để tránh lỗi sai lượt.
- [x] **Chặn Quest Cũ:** Chặn rò rỉ quest của Stacklands tại `BlueprintSanitizer.cs:31` (`// TODO: Intercept Stacklands quest unlocks`).
- [ ] **Chưa Kích Hoạt Hệ Thống Temptation / Dogma:** Lỗi logic tương tác phe phái (`TemptationSystem.cs:17` - `// TODO: Trigger ideological decay or Cat God interaction`). Hậu quả của hệ tư tưởng đang không hoạt động.
- [ ] **Nhiều Hàm Chưa Được Triển Khai (NotImplementedException):** Mã nguồn vẫn còn gọi nhiều exception chưa hoàn thiện (ví dụ ở `GameCard.cs`, `BoardMonths.cs`, `CardBag.cs`, `GameDataLoader.cs`). Cần định vị các hàm này thuộc hệ thống cũ để xóa đi hoặc bổ sung code mới nếu cần thiết.

### 🐛 3.2. Lỗi Vận Hành Viễn Chinh & Tế Đàn (Expedition & Cat God Mouth)
- [x] **Mở Pack Thưởng Gây NullReferenceException:** Lỗi không có ID `"item_low_spirit_stone"` đăng ký trong dữ liệu của `GodCatPackCard.cs`.
- [x] **Xung Đột Tọa Độ Spawn Gây Phá Hủy Đồ:** Enemy và Pack thưởng spawn cùng tọa độ gây đè stack và mất đồ trong `CatGodMouth.cs`.
- [x] **Ghi Đè Dialogue UI:** Hội thoại báng bổ và hoàn thành nghi lễ kích hoạt đồng thời đè giao diện lẫn nhau.
- [x] **Threat 40% Không Hoạt Động:** Lệnh spawn `mob_void_spirit` bị comment và chưa đưa vào Encounter System xử lý.
- [x] **Lỗi Stack Logic Khi Trở Về (ReturnToBase):** Mèo bị dịch chuyển làm gián đoạn khai thác tại trại chính.
- [x] **Lỗi Node Special Map (Dead Node):** Node SpecialMap không có case xử lý gây kẹt game.
- [x] **Lỗi Thương Nhân (Camp Merchant) Gán Cứng:** Tạm thời hardcode trao đổi đồ ăn/vàng thay vì roll ngẫu nhiên từ LootProfile.
- [x] **Lỗi Bệnh Viện Chó Thiếu Debt Note:** Không gọi sinh giấy nợ sau khi điều trị tê liệt thành công.
- [x] **Lỗi Hardcode Văn Bản:** Các chuỗi text Tiếng Việt truyền trực tiếp trong code thay vì thông qua `MewtationsLocTable.tsv`.
- [x] **Lỗi Timer Trận Pháp Đột Phá:** Thêm/bớt card hỗ trợ đột phá giữa chừng không reset timer của `BreakthroughArrayCardData.cs`.

### 🐛 3.3. Lỗ hổng Thư viện (Missing Dependencies)
Khi đưa dự án vào môi trường Unity mới, bắt buộc phải cài đặt các package/DLL sau để tránh lỗi biên dịch:
- [ ] **UnityEngine.InputSystem** (Package Manager)
- [ ] **UnityEngine.Rendering.PostProcessing** (Package Manager)
- [ ] **Newtonsoft.Json** (DLL hoặc com.unity.modules.jsonserialize)
- [ ] **HarmonyLib** (Thiếu DLL của HarmonyLib dùng cho Mod game - có thể gỡ bỏ nếu không làm mod).

---

## 4. Danh Sách Tàn Dư Legacy Cần Dọn Dẹp (Chưa Gỡ Bỏ Ngay)

1. **Trọn bộ DLC "Greed" của Stacklands (Cursed Worlds):**
   - Không liên quan đến GDD hiện tại, chứa các class đòi cống nạp đồ và sinh quái (`GreedCutscenes`, `DemandManager`, cấu hình túi thẻ `Greed_...`).
   - Cần gỡ bỏ hoàn toàn khỏi `GameScreen.cs`, `RunOptions.cs` và `GameDataLoader.cs` để dọn dẹp kiến trúc.
2. **Chỉ số Greed & Corruption gắn với hệ quả vật lý trong Expedition:**
   - Hệ thống Viễn chinh vẫn đang tính toán `GreedLevel` và `CorruptionLevel` (ví dụ: cộng dồn dựa trên số xác mèo trên bàn cờ, cộng dồn khi chọn sai sự kiện).
   - Cần thiết kế lại hệ quả (Consequence) cho các chỉ số này bằng hệ thống Threat mới thay cho quái vật lý cũ đã bị xóa.

### 🔴 2.5. Đơn Giản Hóa Trading Post & Blueprint Unlock (Upcoming Session)
Dựa theo scope hiện tại, hệ thống mua bán sẽ tuân thủ nguyên tắc đơn giản, không NPC phức tạp:
- [ ] **Bán Blueprint trực tiếp:** Trading Post chọn vài blueprint hợp lệ để bán (chưa unlock, đúng progression tier).
- [ ] **Flow Mua Hàng:** Mua xong -> Trừ tiền -> Unlock recipe -> Spawn blueprint card rơi ra cạnh Trading Post -> Loại khỏi shop pool.
- [ ] **Blueprint Physical Spawn:** Mọi hình thức unlock recipe (từ shop, event, loot) đều phải gọi 1 luồng chung UnlockBlueprint() để spawn blueprint card vật lý ra bàn cờ. Đảm bảo tính tactile và giúp player awareness mà không cần popup UI.
- [ ] **Progression Tiers:** Giữ nhẹ nhàng với các mốc: Primitive, Village, Cultivation, Industrial, Forbidden.
- [ ] **Định Giá (Pricing):** Cố định đơn giản BlueprintValue = RawValue x 2.
- [ ] **Lưu Trữ Tối Giản:** Dùng HashSet<string> unlockedBlueprints để theo dõi tiến độ unlock.
- [ ] **RecipeBook UX:** Thêm icon "NEW" hoặc hiệu ứng glow nhẹ cho các recipe đã unlock nhưng chưa được craft lần nào.
