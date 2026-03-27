using Fusion;
using UnityEngine;

public class RunnerSetup : MonoBehaviour
{
    void Awake()
    {
        var runner = GetComponent<NetworkRunner>();

        runner.ProvideInput = true; // 🔥 CHÍNH LÀ CHỖ NÀY

        runner.AddCallbacks(GetComponent<PlayerInputHandler>());
    }
}