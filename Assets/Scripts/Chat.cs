using UnityEngine;
using Fusion;

public class Chat : NetworkBehaviour
{
    public TMPro.TMP_InputField inputField;
    public GameObject Content;
    public GameObject messagePrefab;
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SendMessageRPC(string message)
    {
        var msg = Instantiate(messagePrefab, Content.transform);
        msg.GetComponentInChildren<TMPro.TMP_Text>().text = message;
    }
    public void SendMessage()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        if (Object == null || Object.IsValid == false) return;
        SendMessageRPC(inputField.text);
        inputField.text = "Enter text...";
    }

}
