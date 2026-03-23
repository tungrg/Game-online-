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
    public IEnumerator MoveMultipleDirectionsTest()
    {
        yield return SceneManager.LoadSceneAsync("SampleScene");

        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector3 startPos = player.transform.position;

        var keyboard = InputSystem.GetDevice<Keyboard>();

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

        Vector3 afterDown = player.transform.position;

        // ===== ASSERT =====

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
        }

        // thả phím
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();
    }
}