using UnityEngine;

namespace GameScripts.Systems.Threat.UI
{
    public class WarningUI : MonoBehaviour
    {
        // TODO: Tham chiếu tới các UI Element con trên màn hình

        public void AddOrUpdateWarning(ThreatInstance threat)
        {
            // Thêm cảnh báo mới hoặc cập nhật số ngày đếm ngược
        }

        public void RemoveWarning(ThreatInstance threat)
        {
            // Xóa cảnh báo khi Threat chuyển sang Active
        }
    }
}
