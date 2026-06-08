# Bảng Phân Tích Sự Phụ Thuộc Của Dân Làng (Villager Dependencies)

Để có cái nhìn tổng quan trước khi thực hiện "Cat Civilization Replacement", dưới đây là bảng thống kê các chức năng, thẻ và hệ thống đang liên kết (hard-coded) với `BaseVillager`, `Worker`, `Kid` và các khái niệm liên quan đến con người.

## 1. Hệ thống Đời người (Life Stage & Sinh sản)
| Card / Script | Chức năng phụ thuộc | Mức độ nguy hiểm | Hướng xử lý |
| --- | --- | --- | --- |
| `House.cs` | Cho phép `BaseVillager` và `Kid` sinh đẻ hoặc lớn lên. Đếm số lượng dân làng trên board để giới hạn đẻ. | Rất Cao | Chuyển đổi logic sang đếm `CatCardData` (giới hạn sinh sản của Mèo). |
| `Kid.cs` | Cơ chế lớn lên thành `BaseVillager`. | Trung bình | Xóa bỏ class này sau khi migration `cat_kitten` hoàn tất. |
| `BaseVillager.cs` | Chứa `UpdateLifeStage()` (già đi thành `OldVillager` -> chết thành `Corpse`). | Cao | Chuyển logic lão hóa sang cơ chế Tu Tiên (Thọ nguyên) của Mèo. |

## 2. Hệ thống Lao động & Đào tạo (Economy & Labor)
| Card / Script | Chức năng phụ thuộc | Mức độ nguy hiểm | Hướng xử lý |
| --- | --- | --- | --- |
| `WorkerType.cs` | Enum quy định loại thợ: `Normal`, `Educated`, `Robot`. | Rất Cao | Thay thế bằng interface `ILaborCapable` hoặc `CatSpecialization`. |
| `Academy.cs` | Biến đổi `WorkerType.Normal` thành `WorkerType.Educated`. | Cao | Sửa thành nơi Đào tạo/Tu luyện cho `CatCardData`. |
| `Subprint.cs` | Logic công thức craft yêu cầu ID `any_educated_worker`. | Cao | Cập nhật file Recipe JSON/CSV để đổi thành `cat_laborer` hoặc `ILaborCapable`. |
| `Worker.cs` | Đại diện cho dân làng có nghề nghiệp. Cung cấp chỉ số `WorkerAmount`. | Cao | Chuyển data sang `CatCardData` (Mèo cũng là Worker). |

## 3. Hệ thống Cư trú & Phúc lợi (Housing & Wellbeing)
| Card / Script | Chức năng phụ thuộc | Mức độ nguy hiểm | Hướng xử lý |
| --- | --- | --- | --- |
| `HousingConsumer.cs`| Bắt buộc entity phải gọi `GetWorkerType()` để được xếp nhà. | Rất Cao | Tách `HousingConsumer` khỏi `WorkerType`. Đổi sang kiểm tra `CatCardData`. |
| `Apartment.cs` | Nhận `HousingConsumer`, kiểm tra xem nó là Robot hay Dân thường để cho ở. | Trung bình | Sửa thành Chung cư / Ổ cho Mèo. |
| `Happiness.cs` / `Unhappiness.cs` | Lan truyền hoặc giảm trừ điểm khi ở gần `BaseVillager`. | Cao | Đổi target thành Mèo hoặc biến nó thành modifier ẩn. |

## 4. Tương tác Thẻ Bài (Card Interactions)
| Card / Script | Chức năng phụ thuộc | Mức độ nguy hiểm | Hướng xử lý |
| --- | --- | --- | --- |
| `Boat.cs` | Đếm và nhốt `BaseVillager` lên thuyền để ra khơi. | Trung bình | Đổi điều kiện thuyền chở Mèo (`CatCardData`). |
| `FishingSpot.cs` | Chỉ cho phép `BaseVillager` có nghề `fisher` (Ngư dân) câu cá. | Trung bình | Đổi thành Mèo có trait/nghề câu cá. |
| `NamingStone.cs` | Cho phép đổi tên `BaseVillager`, `Animal`, `Kid`. | Thấp | Mở rộng cho phép đổi tên `CatCardData`. |

## Tổng kết
Cái xương sống lớn nhất và nguy hiểm nhất cần xử lý chính là `WorkerType` và `HousingConsumer`. Hầu hết chuỗi công thức craft (Subprint) và nhà ở đang gắn chặt vào `WorkerType` của Stacklands. Việc thay thế toàn bộ bằng `ILaborCapable` hoặc Mèo là mấu chốt để "thanh tẩy" DNA cũ.
