using UnityEngine;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Text;

public class Chat : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject Content;
    public GameObject messagePrefab;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private float messageSpacing = 2f;
    [SerializeField] private bool ensureEventSystem = true;

    private static Chat _instance;
    private static readonly ReliableKey ChatToServerKey = ReliableKey.FromInts(77, 1, 0, 0);
    private static readonly ReliableKey ChatBroadcastKey = ReliableKey.FromInts(77, 2, 0, 0);

    [Serializable]
    private class ChatClientPacket
    {
        public string senderName;
        public string message;
    }

    [Serializable]
    private class ChatBroadcastPacket
    {
        public string sender;
        public string message;
        public int colorIndex;
    }

    public static bool IsInputFocused
    {
        get
        {
            return _instance != null && _instance.inputField != null && _instance.inputField.isFocused;
        }
    }

    private static readonly string[] NameColors =
    {
        "#4CAF50", // Green
        "#2196F3", // Blue
        "#9C27B0", // Purple
        "#FBC02D"  // Yellow
    };

    void Awake()
    {
        _instance = this;

        if (ensureEventSystem)
        {
            EnsureEventSystem();
        }

        if (inputField == null)
        {
            inputField = GetComponentInChildren<TMP_InputField>(true);
        }

        if (chatScrollRect == null)
        {
            chatScrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        ConfigureChatLayout();

        RegisterInputCallbacks();
    }

    void OnEnable()
    {
        RegisterInputCallbacks();
    }

    void OnDisable()
    {
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnInputSubmit);
        }
    }

    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnInputSubmit);
        }

        if (_instance == this)
        {
            _instance = null;
        }
    }

    void RegisterInputCallbacks()
    {
        if (inputField == null)
        {
            return;
        }

        inputField.onSubmit.RemoveListener(OnInputSubmit);
        inputField.onSubmit.AddListener(OnInputSubmit);
    }

    void OnInputSubmit(string _)
    {
        SendMessage();
    }

    void Update()
    {
        if (inputField == null)
        {
            return;
        }

        if (!inputField.isFocused && IsOpenChatKeyPressed())
        {
            FocusInputField();
        }

        if (!inputField.isFocused && IsPrimaryPointerPressedThisFrame() && IsPointerInsideInputField())
        {
            FocusInputField();
        }
    }

    void FocusInputField()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

        inputField.ActivateInputField();
        inputField.Select();
        inputField.MoveTextEnd(false);
    }

    bool IsOpenChatKeyPressed()
    {
        bool pressed = false;

        if (Keyboard.current != null)
        {
            pressed |= Keyboard.current.slashKey.wasPressedThisFrame;
            pressed |= Keyboard.current.numpadDivideKey.wasPressedThisFrame;
        }

        pressed |= Input.GetKeyDown(KeyCode.Slash);
        pressed |= Input.GetKeyDown(KeyCode.KeypadDivide);

        return pressed;
    }

    bool IsPrimaryPointerPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return Input.GetMouseButtonDown(0);
    }

    bool IsPointerInsideInputField()
    {
        RectTransform inputRect = inputField.transform as RectTransform;
        if (inputRect == null)
        {
            return false;
        }

        Vector2 screenPosition = Input.mousePosition;
        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }

        return RectTransformUtility.RectangleContainsScreenPoint(inputRect, screenPosition, null);
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

        // StandaloneInputModule works with both legacy Input and Input System package setups.
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    public void SendMessage()
    {
        if (inputField == null)
        {
            Debug.LogWarning("Chat: inputField is not assigned.");
            return;
        }

        string textToSend = inputField.text != null ? inputField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(textToSend))
        {
            return;
        }

        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null && runner.IsRunning)
        {
            SendReliableChat(runner, textToSend);
        }
        else
        {
            AddMessageToChat("Player", textToSend, 0);
            Debug.LogWarning("Chat: NetworkRunner is not running, message displayed locally only.");
        }

        inputField.text = string.Empty;
        FocusInputField();
    }

    void AddMessageToChat(string message)
    {
        AddMessageToChat("Player", message, 0);
    }

    void AddMessageToChat(string sender, string message, int colorIndex)
    {
        if (Content == null || messagePrefab == null)
        {
            Debug.LogWarning("Chat: Content or messagePrefab is not assigned.");
            return;
        }

        var msg = Instantiate(messagePrefab, Content.transform, false);
        var messageText = msg.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            string safeSender = EscapeRichText(sender);
            string safeMessage = EscapeRichText(message);
            string nameColor = NameColors[Mathf.Abs(colorIndex) % NameColors.Length];
            messageText.text = $"<color={nameColor}>{safeSender}</color>: {safeMessage}";
            messageText.enableWordWrapping = true;
            messageText.overflowMode = TextOverflowModes.Overflow;

            messageText.ForceMeshUpdate();
            float preferredHeight = Mathf.Max(24f, messageText.preferredHeight + 6f);

            var layoutElement = msg.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = msg.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = false;
            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0f;

            var rootRect = msg.transform as RectTransform;
            if (rootRect != null)
            {
                rootRect.anchorMin = new Vector2(0f, 1f);
                rootRect.anchorMax = new Vector2(1f, 1f);
                rootRect.pivot = new Vector2(0.5f, 1f);
                rootRect.sizeDelta = new Vector2(0f, preferredHeight);
            }
        }
        else
        {
            Debug.LogWarning("Chat: messagePrefab has no TMP_Text child.");
        }

        ScrollToLatestMessage();
    }

    void ConfigureChatLayout()
    {
        if (Content == null)
        {
            return;
        }

        var contentTransform = Content.transform as RectTransform;
        if (contentTransform == null)
        {
            return;
        }

        var layout = Content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = Content.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = Mathf.Max(0f, messageSpacing);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = Content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = Content.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (chatScrollRect != null)
        {
            chatScrollRect.content = contentTransform;
            chatScrollRect.vertical = true;
            chatScrollRect.horizontal = false;
            chatScrollRect.scrollSensitivity = Mathf.Max(15f, chatScrollRect.scrollSensitivity);
        }
    }

    void ScrollToLatestMessage()
    {
        if (Content == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        var contentRect = Content.transform as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    public static void ReceiveNetworkMessage(string sender, string message, int colorIndex)
    {
        if (_instance != null)
        {
            _instance.AddMessageToChat(sender, message, colorIndex);
        }
    }

    public static void HandleReliableDataReceived(NetworkRunner runner, PlayerRef sender, ReliableKey key, ArraySegment<byte> data)
    {
        if (key == ChatToServerKey)
        {
            if (!runner.IsServer || data.Count <= 0)
            {
                return;
            }

            string jsonClient = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
            ChatClientPacket clientPacket = JsonUtility.FromJson<ChatClientPacket>(jsonClient);
            if (clientPacket == null || string.IsNullOrWhiteSpace(clientPacket.message))
            {
                return;
            }

            BroadcastChat(runner, sender, clientPacket.senderName, clientPacket.message);
            return;
        }

        if (key != ChatBroadcastKey || data.Count <= 0)
        {
            return;
        }

        string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
        ChatBroadcastPacket packet = JsonUtility.FromJson<ChatBroadcastPacket>(json);
        if (packet == null || string.IsNullOrEmpty(packet.message))
        {
            return;
        }

        ReceiveNetworkMessage(packet.sender, packet.message, packet.colorIndex);
    }

    private void SendReliableChat(NetworkRunner runner, string message)
    {
        string senderName = ResolveLocalSenderName(runner);

        if (runner.GameMode == GameMode.Shared || runner.IsServer)
        {
            BroadcastChat(runner, runner.LocalPlayer, senderName, message);
            return;
        }

        ChatClientPacket packet = new ChatClientPacket
        {
            senderName = senderName,
            message = message
        };

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);
        runner.SendReliableDataToServer(ChatToServerKey, payload);
    }

    private static void BroadcastChat(NetworkRunner runner, PlayerRef sender, string senderName, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        int displayNumber = ResolveDisplayNumber(runner, sender);
        int colorIndex = (displayNumber - 1) % NameColors.Length;
        string displayName = string.IsNullOrWhiteSpace(senderName) ? $"Player {displayNumber}" : senderName;

        ChatBroadcastPacket packet = new ChatBroadcastPacket
        {
            sender = displayName,
            message = message,
            colorIndex = colorIndex
        };

        string json = JsonUtility.ToJson(packet);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        ReceiveNetworkMessage(packet.sender, packet.message, packet.colorIndex);

        if (runner == null)
        {
            return;
        }

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (player == runner.LocalPlayer)
            {
                continue;
            }

            runner.SendReliableDataToPlayer(player, ChatBroadcastKey, payload);
        }
    }

    private string ResolveLocalSenderName(NetworkRunner runner)
    {
        string fromPlayerData = PlayerData.PlayerName != null ? PlayerData.PlayerName.Trim() : string.Empty;
        if (!string.IsNullOrWhiteSpace(fromPlayerData))
        {
            return fromPlayerData;
        }

        if (runner != null)
        {
            NetworkObject localObject;
            if (runner.TryGetPlayerObject(runner.LocalPlayer, out localObject) && localObject != null)
            {
                // var controller = localObject.GetComponent<TankController>();
                // if (controller != null && !string.IsNullOrWhiteSpace(controller.PlayerName))
                // {
                //     return controller.PlayerName.Trim();
                // }
            }

            int displayNumber = ResolveDisplayNumber(runner, runner.LocalPlayer);
            return $"Player {displayNumber}";
        }

        return "Player";
    }

    private static int ResolveDisplayNumber(NetworkRunner runner, PlayerRef targetPlayer)
    {
        if (runner == null)
        {
            return Mathf.Max(1, targetPlayer.RawEncoded);
        }

        int rank = 1;
        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (player == targetPlayer)
            {
                continue;
            }

            if (player.RawEncoded >= targetPlayer.RawEncoded)
            {
                continue;
            }

            NetworkObject playerObject;
            if (runner.TryGetPlayerObject(player, out playerObject) && playerObject != null)
            {
                rank++;
            }
        }

        return Mathf.Max(1, rank);
    }

}
