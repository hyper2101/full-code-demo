# Legacy Systems Registry

Tài liệu này liệt kê tất cả các chức năng, cơ chế, và class đang tồn tại trong mã nguồn nhưng **không nằm trong GDD của Mewtations**. 

Đây là các "tàn dư" của Stacklands. Chúng hiện tại đã bị cô lập (quarantined) hoặc đóng băng, nhưng chưa bị xóa vật lý. Danh sách này được tạo ra để các tác vụ sau này (hoặc các Agent sau này) có thể tham chiếu: hoặc tái sử dụng (re-purpose) cho một logic mới, hoặc xóa sổ hoàn toàn (purge) khi project đạt độ ổn định cao hơn.

---

## 1. Hệ sinh thái Con người (Villager Ecosystem)
GDD của Mewtations tập trung vào **Loài Mèo (Cats), Sinh tồn, Đột biến và Thám hiểm**. Do đó, toàn bộ concept về xã hội loài người của Stacklands đang bị dư thừa.
* **Các Class tồn đọng:**
  * `BaseVillager`, `Villager`, `OldVillager`, `TeenageVillager`
  * `Worker`, `RobotWorker`, `WorkerTransformHolder`, `WorkerBlueprint`
  * `HousingConsumer`, `Apartment`, `House`
* **Lý do bỏ ngỏ:** Mewtations không quản lý nhân khẩu học bằng cách sinh đẻ/nhà ở truyền thống. Số lượng tín đồ/mèo được thu thập qua Expedition hoặc Ritual.

## 2. Công nghiệp & Tự động hóa (Industrial Automation)
Các cơ chế tự động hóa kiểu Factorio/Mindustry không phù hợp với không khí "Cult/Sacrifice/Expedition" của Mewtations.
* **Các Class tồn đọng:**
  * `Conveyor`, `Battery`, `PassiveEnergyGenerator`, `IndustrialSmelter`
  * Logic về điện năng (`IEnergy`, `EnergyLogic`)
  * Các mạch Logic (Logic Gates) và nam châm hút tài nguyên (`ResourceMagnet`)
* **Trạng thái:** Đã được gắn tag `[LegacySystem(DeprecatedAutomation)]`. Phần khung đồ thị (Topology) đã được bóc tách ra dùng riêng. Phần máy móc vật lý đang bị bỏ ngỏ.

## 3. Hệ thống Thuế & Chỉ số Thành phố (Cities Economy)
Mewtations sử dụng `TemptationSystem` (Sự áp bức của Cat God / Trả giá) thay cho hệ thống Thuế và Quản lý Thành phố.
* **Các Class tồn đọng:**
  * `CitiesManager` (Quản lý trạng thái CityState, Wellbeing)
  * `Demand`, `DemandManager`, `DemandEvent`, `EnergyDemand`
  * UI: `DemandProgressBar`, `CitiesDashboard`
* **Trạng thái:** Đã bị neutralized bởi cờ `LegacyRuntimeFlags.EnableCitiesSystem` và `EnableDemands`. 

## 4. Nhiệm vụ dạng Checklist (Narrative Checklists)
Hệ thống nhiệm vụ của Stacklands mang tính chất tutorial/achievement checklist, không phù hợp với Environmental Storytelling và Narrative Graph của Mewtations.
* **Các Class tồn đọng:**
  * `QuestManager`, `AllQuests`, `QuestGroup`
* **Trạng thái:** Đã bị neutralized bởi cờ `LegacyRuntimeFlags.EnableQuestHooks`. Cấu trúc thay thế `WorldStateTracker` đã được tạo ra để quản lý cốt truyện mở.

## 5. Combat V1 (Real-time Bump Combat)
GDD yêu cầu Tactical Turn-based Combat. Tuy nhiên, logic combat thời gian thực (đụng nhau là tự đánh) vẫn còn trong mã nguồn.
* **Các chức năng tồn đọng (bên trong `Combatable.cs`):**
  * Vòng lặp timer tự động tấn công.
  * Tự động hiển thị mũi tên Conflict (DrawConflictArrows).
  * Các coroutine và Invoke tính toán hit chance thời gian thực.
* **Trạng thái:** Toàn bộ code đã bị đánh dấu `[Obsolete]` hoặc bị khóa lại bởi thuộc tính `IsPassiveCombatant`. Combatable giờ chỉ giữ vai trò chứa Data (HP, Stats, Faction).

## 6. Global Weather & Curses
Hệ thống thời tiết toàn cục và nguyền rủa (Curses) theo tháng của Stacklands. Mewtations sẽ áp dụng thời tiết theo từng Vùng Thám Hiểm (Biomes) và Corruption State.
* **Các Class tồn đọng:**
  * `WeatherManager`, `Wind`
  * Lịch trình thiên tai hardcode theo Moon (Tháng).
* **Trạng thái:** `EnvironmentalContext` đã được tạo ra để thay thế, nhưng hệ thống cũ vẫn nằm trong thư mục Legacy.

## 7. Các dạng Board (Bảng chơi) cũ
Stacklands có các "Board" đặc thù như Đảo (Island), Lòng đất (Death), Greed.
* **Các Class tồn đọng:**
  * Logic hardcode chuyển map trong `WorldManager`: "main", "island", "death", "cities", "greed".
* **Lý do bỏ ngỏ:** Mewtations sử dụng hệ thống Expedition Map (Bản đồ thám hiểm theo Node) để di chuyển giữa các vùng, không xài board xếp bài tách biệt theo kiểu Stacklands cũ.

---

> [!TIP]
> **Khuyến nghị cho tương lai:**
> Không nên xóa ngay lập tức (Hard Delete) các class trong mục 1 và mục 7. Một số logic về `Worker` có thể được tái cấu trúc thành `Cultist` hoặc `MutatedSlave`. Các logic về Board có thể được tái cấu trúc thành các `Region` trên bản đồ thám hiểm.
