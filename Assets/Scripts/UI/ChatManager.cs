using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }
    private List<string> chatMessages = new List<string>();
    public Chat chat;

    public void Awake()
    {
        Instance = this;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcReceiveChatMessage(string message)
    {
        string formattedMessage = $"Player {Time.time:HH:mm:ss}: {message}";
        chatMessages.Add(formattedMessage);
        //chat.chatContext.text += formattedMessage + "\n";
    }
    
    public void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        RpcReceiveChatMessage(message);
    }

}
