using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.LowLevel;

public class MovingTest
{
    [UnityTest]
<<<<<<< HEAD
    public IEnumerator Moving()
=======
    public IEnumerator MoveMultipleDirectionsTest()
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
    {
        yield return SceneManager.LoadSceneAsync("SampleScene");

        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);

<<<<<<< HEAD
        // 🔥 ĐỢI NETWORK + INIT
        yield return new WaitForSeconds(2f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Assert.IsNotNull(player, "Không tìm thấy Player");

=======
        GameObject player = GameObject.FindGameObjectWithTag("Player");

>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
        Vector3 startPos = player.transform.position;

        var keyboard = InputSystem.GetDevice<Keyboard>();

<<<<<<< HEAD
        Debug.Log("Start Pos: " + startPos);

        // 👉 đi phải
        yield return HoldKey(keyboard, Key.RightArrow, 1f);
        yield return new WaitForSeconds(0.5f);

        Vector3 afterRight = player.transform.position;
        Debug.Log("After Right: " + afterRight);

        // 👉 đi lên
        yield return HoldKey(keyboard, Key.UpArrow, 1f);
        yield return new WaitForSeconds(0.5f);

        Vector3 afterUp = player.transform.position;

        // 👉 đi trái
        yield return HoldKey(keyboard, Key.LeftArrow, 1f);
        yield return new WaitForSeconds(0.5f);

        Vector3 afterLeft = player.transform.position;

        // 👉 đi xuống
        yield return HoldKey(keyboard, Key.DownArrow, 1f);
        yield return new WaitForSeconds(0.5f);
=======
        // 👉 0.5s đi phải
        yield return HoldKey(keyboard, Key.RightArrow, 0.5f);

        Vector3 afterRight = player.transform.position;

        // 👉 0.5s đi lên
        yield return HoldKey(keyboard, Key.UpArrow, 0.5f);

        Vector3 afterUp = player.transform.position;

        // 👉 0.5s đi trái
        yield return HoldKey(keyboard, Key.LeftArrow, 0.5f);

        Vector3 afterLeft = player.transform.position;

        // 👉 0.5s đi xuống
        yield return HoldKey(keyboard, Key.DownArrow, 0.5f);
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4

        Vector3 afterDown = player.transform.position;

        // ===== ASSERT =====

<<<<<<< HEAD
        Assert.Greater(afterRight.x, startPos.x, "Không đi sang phải");
        Assert.Greater(afterUp.z, afterRight.z, "Không đi lên");
        Assert.Less(afterLeft.x, afterUp.x, "Không đi sang trái");
        Assert.Less(afterDown.z, afterLeft.z, "Không đi xuống");
    }
    IEnumerator HoldKey(Keyboard keyboard, Key key, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // giữ phím
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();

            yield return null;

            elapsed += 0.02f;
=======
        // đi phải → X tăng
        Assert.Greater(afterRight.x, startPos.x);

        // đi lên → Z tăng (tuỳ game bạn dùng XZ hay XY)
        Assert.Greater(afterUp.z, afterRight.z);

        // đi trái → X giảm
        Assert.Less(afterLeft.x, afterUp.x);

        // đi xuống → Z giảm
        Assert.Less(afterDown.z, afterLeft.z);
    }
    IEnumerator HoldKey(Keyboard keyboard, Key key, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();

            time += Time.deltaTime;
            yield return null;
>>>>>>> e3ee0d1448c9db405dd1417bb540b124d85142a4
        }

        // thả phím
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();
    }
}