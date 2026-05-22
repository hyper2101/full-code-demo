using System;
using System.Collections.Generic;
using UnityEngine;

public class DayEventSystem
{
    private WorldManager _world;

    public DayEventSystem(WorldManager world)
    {
        _world = world;
    }

    public bool HasActiveEvent()
    {
        if (_world.AllCards == null) return false;

        foreach (GameCard card in _world.AllCards)
        {
            if (card != null && card.CardData != null && card.CardData.IsEventCard)
            {
                return true;
            }
        }
        return false;
    }

    public void TriggerDayEvent(int day)
    {
        if (day == 5)
        {
            Vector3 spawnPos = Vector3.zero;
            if (_world.CurrentBoard != null)
            {
                spawnPos = _world.CurrentBoard.MiddleOfBoard();
            }

            _world.CreateCard(spawnPos, "event_beast_tide", true, true, true);

            string title = "THÚ TRIỀU DỒNG DẬP!";
            string text = "💡 <b>CẢNH BÁO THÚ TRIỀU BÙNG PHÁT!</b>\n\n" +
                          "Linh khí triều tịch cuồn cuộn dâng trào, yêu thú sâu trong rừng rậm hoang dã bắt đầu bùng nổ chuyển động!\n\n" +
                          "Một tấm thẻ <b>Thú Triều Bạo Động (event_beast_tide)</b> đã xuất hiện tại trung tâm bảng đấu. " +
                          "Bản đồ thám hiểm sẽ xuất hiện nhiều Yêu Thú hung tợn hơn, nhưng bù lại linh dược rớt ra sẽ vô cùng phong phú!\n\n" +
                          "<i>Lời khuyên: Chuẩn bị 3+3 Trận hình Thần Miêu để sẵn sàng nghênh chiến!</i>";

            if (Mewtations.Dialogue.DialogueSystem.Instance != null)
            {
                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(
                    title, 
                    text, 
                    new List<string> { "Sẵn sàng chiến đấu!" }, 
                    (choiceIdx) => { }
                );
            }
        }
        else if (day == 15)
        {
            Vector3 spawnPos = Vector3.zero;
            if (_world.CurrentBoard != null)
            {
                spawnPos = _world.CurrentBoard.MiddleOfBoard();
            }

            _world.CreateCard(spawnPos, "event_tribulation", true, true, true);

            string title = "THIÊN KIẾP GIÁNG LÂM!";
            string text = "⚡ <b>THỬ THÁCH THIÊN KIẾP!</b>\n\n" +
                          "Sấm chớp cuồng phong từ chín tầng mây tụ hội, quy tắc thiên địa khảo nghiệm người nghịch thiên cải mệnh!\n\n" +
                          "Tấm thẻ <b>Thiên Kiếp Thử Thách (event_tribulation)</b> đã xuất hiện ở trung tâm đấu trường.\n\n" +
                          "Hãy chuẩn bị đầy đủ Linh Đan Đột Phá, thức ăn bồi bổ kích hoạt Ultimate Skill và trang bị mạnh mẽ nhất để chống chọi kiếp lôi!";

            if (Mewtations.Dialogue.DialogueSystem.Instance != null)
            {
                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(
                    title, 
                    text, 
                    new List<string> { "Nghịch thiên cải mệnh!" }, 
                    (choiceIdx) => { }
                );
            }
        }
    }
}
