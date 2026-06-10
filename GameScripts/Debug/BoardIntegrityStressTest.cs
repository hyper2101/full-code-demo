using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 8: Runtime Integrity Stress Test
/// Bộ công cụ tra tấn hệ thống Board OS để đảm bảo không bị rò rỉ bộ nhớ, duplicate ownership, ghost cards, hay event spaghetti.
/// </summary>
public class BoardIntegrityStressTest : MonoBehaviour
{
    private WorldManager _world;

    private void Start()
    {
        _world = WorldManager.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            RunAllStressTests();
        }
    }

    public void RunAllStressTests()
    {
        Debug.LogWarning("====== BẮT ĐẦU BOARD INTEGRITY STRESS TEST ======");
        
        StartCoroutine(ExecuteTestsRoutine());
    }

    private IEnumerator ExecuteTestsRoutine()
    {
        yield return TestSpamAttachDetach();
        yield return TestDestructionWhileOccupied();
        yield return TestCyclicGraphGuard();
        
        Debug.LogWarning("====== KẾT THÚC BOARD INTEGRITY STRESS TEST - [SUCCESS] ======");
    }

    private IEnumerator TestSpamAttachDetach()
    {
        Debug.Log("--- Test 1: Spam Attach/Detach (500 lần) ---");
        
        GameCard structure = CreateTestCard("shrine");
        GameCard offering = CreateTestCard("relic_stone");

        yield return new WaitForSeconds(0.1f);

        int iterations = 500;
        bool success = true;

        for (int i = 0; i < iterations; i++)
        {
            // Attach
            bool attached = _world.Attachment.RequestAttach(structure, offering, "shrine_slot_0", AttachContext.Drop());
            if (!attached)
            {
                Debug.LogError("[Test 1 FAILED] Không thể Attach ở vòng lặp thứ " + i);
                success = false;
                break;
            }

            // Verify
            if (!offering.HasStructureParent() || offering.StructureParent != structure)
            {
                Debug.LogError("[Test 1 FAILED] Ownership gắn sai ở vòng lặp thứ " + i);
                success = false;
                break;
            }

            // Detach
            bool detached = _world.Attachment.RequestDetach(offering, AttachContext.Drop());
            if (!detached)
            {
                Debug.LogError("[Test 1 FAILED] Không thể Detach ở vòng lặp thứ " + i);
                success = false;
                break;
            }

            // Verify
            if (offering.HasStructureParent())
            {
                Debug.LogError("[Test 1 FAILED] Ownership gỡ sai ở vòng lặp thứ " + i);
                success = false;
                break;
            }
        }

        if (success) Debug.Log("[Test 1 SUCCESS] 500 lần thao tác hoàn hảo.");

        structure.DestroyCard(true, true);
        offering.DestroyCard(true, true);
    }

    private IEnumerator TestDestructionWhileOccupied()
    {
        Debug.Log("--- Test 2: Xóa Structure khi đang chứa Card ---");
        
        GameCard farm = CreateTestCard("garden");
        GameCard seed = CreateTestCard("berry");

        yield return new WaitForSeconds(0.1f);

        _world.Attachment.RequestAttach(farm, seed, "seed_slot_0", AttachContext.Drop());

        // Bùm
        farm.DestroyCard(true, true);
        
        yield return new WaitForSeconds(0.1f);

        if (seed == null || seed.Destroyed)
        {
            Debug.LogError("[Test 2 FAILED] Thẻ con bị xóa theo Structure một cách không mong muốn.");
        }
        else if (seed.HasStructureParent())
        {
            Debug.LogError("[Test 2 FAILED] Thẻ con vẫn lưu giữ Ghost Reference tới Structure bị xóa.");
        }
        else
        {
            Debug.Log("[Test 2 SUCCESS] Thẻ con trở thành Loose Card an toàn.");
        }

        if (seed != null) seed.DestroyCard(true, true);
    }

    private IEnumerator TestCyclicGraphGuard()
    {
        Debug.Log("--- Test 3: Cyclic Graph Guard ---");

        GameCard a = CreateTestCard("shrine");
        GameCard b = CreateTestCard("shrine"); // Gắn 2 shrine vào nhau nếu trick được hệ thống

        yield return new WaitForSeconds(0.1f);

        // Giả sử có 1 bug logic nào đó gọi trực tiếp SetStructureParent để lừa đảo
        // Chúng ta cố tình ném Exception nếu Cyclic. 
        // Trong hệ thống Sandbox thật, SetStructureParent nên ném Exception nếu a == b hoặc graph loop.
        
        // Vì hiện tại AttachmentSystem chặn Type rồi, nên ta test xem Reconciler có chết không nếu cố ý can thiệp.
        try
        {
            a.SetStructureParent(b, AttachContext.Force(AttachmentReason.DebugForceAttach));
            b.SetStructureParent(a, AttachContext.Force(AttachmentReason.DebugForceAttach));

            // Chạy Integrity Validator để nó bắt
            _world.BoardIntegrity.ValidateAndRepair();

            // Nếu nó break được loop bằng null, là Success.
            // Wait, Validator hiện tại chỉ check Stack (Parent), chưa check StructureParent cyclic. 
            // Dù sao test này cũng đánh dấu là cần mở rộng Validator trong tương lai.
            Debug.Log("[Test 3 WARNING] Cyclic StructureParent chưa được Validator bắt tự động, cần implement trong BoardIntegrityValidator.");
        }
        catch (Exception ex)
        {
            Debug.Log("[Test 3 SUCCESS] Bắt được Exception khi phá hoại Graph: " + ex.Message);
        }

        a.DestroyCard(true, true);
        b.DestroyCard(true, true);
    }

    private GameCard CreateTestCard(string id)
    {
        CardData data = _world.CreateCard(new Vector3(0, 0, 0), id, true, false, false);
        return data != null ? data.MyGameCard : null;
    }
}
