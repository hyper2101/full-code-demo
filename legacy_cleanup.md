# Mewtations: Dogma — Hướng dẫn & Danh sách Dọn dẹp Legacy (Legacy Systems Cleanup)

Tài liệu này ghi nhận toàn bộ các chức năng, cơ chế, class và thuật ngữ Stacklands cũ đang tồn đọng trong mã nguồn nhưng không còn phù hợp với hướng đi thiết kế mới của **Mewtations: Dogma**. 

Mục tiêu là hỗ trợ định vị, cô lập, tái cấu trúc hoặc xóa sạch (purge) các tàn dư này để tối ưu mã nguồn và tránh nhiễm bẩn kiến trúc (architectural contamination) trong tương lai.

---

## 1. Triết Lý Thiết Kế Mới của Mewtations: Dogma
Trước khi xây dựng bất kỳ tính năng nào hoặc dọn dẹp code, cần đối chiếu với nguyên tắc cốt lõi:
- **Tập trung vào:** Sự cá nhân hóa của Mèo (individuality), sự sinh tồn mang tính hệ tư tưởng (ideological survival), áp lực lãnh thổ (territorial pressure), nền kinh tế khan hiếm (scarcity economy), sự bất ổn định tâm lý (emotional instability), và turn-based combat mang tính chiến thuật.
- **Tránh xa:** Mô hình Stacklands cũ như tự động hóa hoàn toàn và tối ưu hóa vô hạn (cozy automation), trồng trọt/khai thác tài nguyên vô hạn, quần thể mèo hoạt động như các worker vô tri (generic pawns).

---

## 2. Danh Sách 7 Hệ Thống Legacy Cũ & Trạng Thái Cách Ly

### 2.1. Hệ sinh thái Con người (Villager Ecosystem)
- **Concept cũ:** Quản lý nhân khẩu bằng sinh đẻ, nhà ở Stacklands truyền thống.
- **Các Class tồn đọng:** `BaseVillager`, `Villager`, `OldVillager`, `TeenageVillager`, `Worker`, `RobotWorker`, `WorkerTransformHolder`, `WorkerBlueprint`, `HousingConsumer`, `Apartment`, `House`.
- **Trạng thái:** Mewtations sử dụng hệ thống chiêu mộ Cat qua Expedition hoặc Nghi lễ (Ritual). Toàn bộ dân cư cũ đang dư thừa.

### 2.2. Công nghiệp & Tự động hóa (Industrial Automation)
- **Concept cũ:** Các cơ chế tự động hóa kiểu Factorio/Mindustry (điện năng, băng chuyền).
- **Các Class tồn đọng:** `Conveyor`, `Battery`, `PassiveEnergyGenerator`, `IndustrialSmelter`, logic điện năng (`IEnergy`, `EnergyLogic`), mạch logic (Logic Gates), nam châm hút tài nguyên (`ResourceMagnet`).
- **Trạng thái:** Đã được gắn tag `[LegacySystem(DeprecatedAutomation)]`. Phần khung đồ thị (Topology) đã được bóc tách để dùng riêng. Phần máy móc vật lý đang bị bỏ ngỏ và cần xóa bỏ.

### 2.3. Hệ thống Thuế & Chỉ số Thành phố (Cities Economy)
- **Concept cũ:** Quản lý thành phố kiểu Wellbeing, CitiesManager.
- **Các Class tồn đọng:** `CitiesManager`, `Demand`, `DemandManager`, `DemandEvent`, `EnergyDemand`, UI: `DemandProgressBar`, `CitiesDashboard`.
- **Trạng thái:** Đã bị neutralized hoàn toàn bằng các cờ `LegacyRuntimeFlags.EnableCitiesSystem` và `EnableDemands`. Mewtations sử dụng hệ thống `TemptationSystem` và nợ thuế chó (`DogTax`) để thay thế.

### 2.4. Nhiệm vụ dạng Checklist (Narrative Checklists)
- **Concept cũ:** Hệ thống nhiệm vụ tuyến tính, tutorial/achievement checklist kiểu Stacklands.
- **Các Class tồn đọng:** `QuestManager`, `AllQuests`, `QuestGroup`.
- **Trạng thái:** Bị neutralized bởi cờ `LegacyRuntimeFlags.EnableQuestHooks`. Mewtations sử dụng hệ thống mở khóa cốt truyện phi tuyến tính qua `WorldStateTracker` và các Fragment/Chronicle.

### 2.5. Combat V1 (Real-time Bump Combat)
- **Concept cũ:** Đụng nhau tự động đánh thời gian thực, bắn mũi tên Conflict.
- **Các chức năng tồn đọng (trong `Combatable.cs`):** Vòng lặp timer tự động tấn công, gọi vẽ mũi tên Conflict (`DrawConflictArrows`), coroutine tính hit chance thời gian thực.
- **Trạng thái:** Đã bị đánh dấu `[Obsolete]` hoặc khóa lại bởi thuộc tính `IsPassiveCombatant`. Toàn bộ logic combat thực tế đã được chuyển sang `CombatV2` (Turn-based Combat).

### 2.6. Global Weather & Curses
- **Concept cũ:** Thời tiết toàn cục và nguyền rủa (Curses) theo tháng (Moon).
- **Các Class tồn đọng:** `WeatherManager`, `Wind`, lịch trình thiên tai hardcode theo Moon.
- **Trạng thái:** `EnvironmentalContext` đã được tạo ra để thay thế (theo từng Biome/Vùng thám hiểm). Hệ thống cũ vẫn nằm trong thư mục Legacy.

### 2.7. Các dạng Board cũ (Island / Death / Greed Board)
- **Concept cũ:** Tách biệt board chơi ra Đảo (Island), Lòng đất (Death), Greed.
- **Các Class tồn đọng:** Logic hardcode chuyển map trong `WorldManager`: "main", "island", "death", "cities", "greed".
- **Trạng thái:** Mewtations sử dụng Expedition Map (dạng Node) để di chuyển thám hiểm, không sử dụng board xếp bài riêng biệt.

---

## 3. Thay Thế Thuật Ngữ (Terminology Contamination)
Cần rà soát và chuyển đổi dần các thuật ngữ cũ trong code/giao diện:

| Thuật ngữ cũ | Thay thế trong Mewtations | Ghi chú |
| :--- | :--- | :--- |
| **Villager / Worker** | Individual / Citizen / Scavenger / Laborer | Nhấn mạnh tính cá thể độc lập |
| **Mana** | Spiritual Resource (Linh khí) | Tài nguyên dạng năng lượng dầu mỏ |
| **Happiness** | Stability / Desire Balance | Cân bằng tinh thần, ham muốn |
| **Biome** | Territory / Restricted District | Khu vực, lãnh địa bị chó kiểm soát |
| **Raid** | Retaliation / Inspection | Hoạt động tuần tra, tịch thu, đàn áp |
| **Automation** | Social Control | Sự áp bức xã hội thông qua tối ưu hóa |
| **Production Chain** | Infrastructure Network | Hệ thống cơ sở hạ tầng chắp vá |

---

## 4. Audit Cards.cs & Kế Hoạch Tách File
Hiện tại, `Cards.cs` đang ôm đồm quá nhiều hằng số đăng ký card từ Stacklands. Kế hoạch cụ thể như sau:
- **Di chuyển sang `LegacyCards.cs`:** Các hằng số liên quan đến hệ thống cũ:
  - Dân cư cũ: `villager`, `base_villager`, `old_villager`, `teenage_villager`, `worker`.
  - Chỉ số thành phố: `happiness`, `unhappiness`, `pollution`, `wellbeing_generator`.
  - Tiền tệ cũ: `dollar`, `creditcard`.
  - Hệ thống điện/năng lượng: `energy_consumer`, `energy_generator`, `energy_harvestable`, `consuming_energy_generator`, `passive_energy_consumer`, `passive_energy_generator`, `transmission_tower`.
  - Rác thải/Vệ sinh: `sewer`, `septic_tank`, `water_treatment_plant`.
  - Nhà máy cũ: `industrial_revolution`, `industrial_smelter`, `factory`, `factory_parts`, `toy_factory`, `smelter`.
  - Nhà cửa cũ: `royal`, `royal_building`, `angry_royal`, `city_hall`, `apartment`, `house`, `landmark`.
  - Kho bãi cũ: `food_warehouse`, `harvestable`, `farmland`, `garden`.
- **Giữ lại tại `Cards.cs` (hoặc chuyển sang `DogmaCards.cs`):** Các hằng số liên quan đến mèo, combat turn-based hiện tại (như `cat_basic`, `cat_corpse`, các unit chó enforcer).
- **Di chuyển sang `ExperimentalCards.cs`:** Các hằng số thử nghiệm hoặc prototype chưa ổn định.

---

## 5. Danh Sách File Cần Xóa Vật Lý (Hard Deletion Targets)
Dưới đây là các file và thư mục chứa 100% code legacy không còn giá trị biên dịch hoặc sử dụng. Cần thực hiện xóa bỏ vật lý trong các đợt refactoring lớn:

### 🗑️ Thư mục Localization & Texts
- `GameScripts/Core/SokTerms.cs` *(Chứa hàng ngàn hằng số text Stacklands cũ. Đã thay thế bằng `MewtationsLocTable.tsv`)*

### 🗑️ Thư mục Stacklands Automation
*Đường dẫn: `GameScripts/Legacy/Stacklands/Automation/`*
- `Conveyor.cs`, `Battery.cs`, `PassiveEnergyGenerator.cs`
- `IndustrialSmelter.cs`, `EnergyLogic.cs`, `ResourceMagnet.cs`

### 🗑️ Thư mục Cities Economy & Demands
*Đường dẫn: `GameScripts/Legacy/Stacklands/Cities/`*
- `CitiesManager.cs`
- `Demand.cs`, `DemandManager.cs`, `DemandEvent.cs`, `EnergyDemand.cs`
- `DemandProgressBar.cs`, `CitiesDashboard.cs`

### 🗑️ Thư mục Stacklands Linear Quests
*Đường dẫn: `GameScripts/Legacy/Stacklands/Quests/`*
- `QuestManager.cs`
- `AllQuests.cs`
- `QuestGroup.cs`

### 🗑️ Thư mục Worker & Demographics (Generic Pawns)
*Đường dẫn: `GameScripts/Cards/Data/`*
- `BaseVillager.cs`, `Villager.cs`, `OldVillager.cs`, `TeenageVillager.cs`
- `Worker.cs`, `RobotWorker.cs`, `WorkerTransformHolder.cs`, `WorkerBlueprint.cs`
- `HousingConsumer.cs`, `Apartment.cs`, `House.cs`

*(Lưu ý trước khi xóa: Cần đảm bảo `BaseVillager` đã được tách và hủy kế thừa hoàn toàn khỏi các class Mèo mới).*
