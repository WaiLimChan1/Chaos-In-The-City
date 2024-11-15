using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatBoxGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public enum ChatBoxState
    {
        INVISIBLE,
        READING,
        TYPING
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static ChatBoxGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _chatBoxPanel;
    [SerializeField] private GameObject _chatBoxMessagePrefab;
    [SerializeField] private TMP_InputField _chatBoxInputField;

    [Header("Variable")]
    private NetworkedPlayer _networkedPlayer;
    private ChatBoxState chatBoxState;

    [Header("Message")]
    private int maxMessages = 25;
    [SerializeField] private List<TextMeshProUGUI> messageList = new List<TextMeshProUGUI>();
    private float chatBoxDisplayTime = 5;
    private float chatBoxDisplayTimer;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void SetUp(NetworkedPlayer networkedPlayer)
    {
        _networkedPlayer = networkedPlayer;
        chatBoxState = ChatBoxState.INVISIBLE;
        chatBoxDisplayTimer = 0;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public bool IsTyping { get { return chatBoxState == ChatBoxState.TYPING; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Logic Functions
    public string ConvertFloatSecondsToTimeFormat(float totalSeconds)
    {
        float hours = Mathf.Floor(totalSeconds / 3600.0f);
        float minutes = Mathf.Floor((totalSeconds % 3600.0f) / 60.0f);
        float seconds = totalSeconds % 60.0f;

        return string.Format("{0:D2}:{1:D2}:{2:00}", (int)hours, (int)minutes, (int)seconds);
    }

    public void AddMessageToChatBox(float totalSeconds, string playerName, Color playerColor, string messageText)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].gameObject);
            messageList.RemoveAt(0);
        }

        TextMeshProUGUI chatMessage = Instantiate(_chatBoxMessagePrefab, _chatBoxPanel.transform).GetComponent<TextMeshProUGUI>();
        chatMessage.text = $"[{ConvertFloatSecondsToTimeFormat(totalSeconds)}] ({playerName}): {messageText}";
        chatMessage.color = playerColor;
        messageList.Add(chatMessage);

        chatBoxDisplayTimer = chatBoxDisplayTime;
        chatBoxState = ChatBoxState.READING;
    }
    
    private void LateUpdate()
    {
        if (!NetworkedPlayer.CanUseNetworkedPlayer(_networkedPlayer))
        {
            _content.gameObject.SetActive(false);
            return;
        }

        KeyCode triggerChat = KeyCode.Return;

        if (chatBoxState == ChatBoxState.INVISIBLE)
        {
            _content.gameObject.SetActive(false);

            if (Input.GetKeyDown(triggerChat)) chatBoxState = ChatBoxState.TYPING;
        }
        else if (chatBoxState == ChatBoxState.READING)
        {
            _content.gameObject.SetActive(true);
            _chatBoxInputField.gameObject.SetActive(false);

            chatBoxDisplayTimer -= Time.deltaTime;
            if (chatBoxDisplayTimer <= 0) chatBoxState = ChatBoxState.INVISIBLE;

            if (Input.GetKeyDown(triggerChat)) chatBoxState = ChatBoxState.TYPING;
        }
        else if (chatBoxState == ChatBoxState.TYPING)
        {
            _content.gameObject.SetActive(true);
            _chatBoxInputField.gameObject.SetActive(true);

            chatBoxDisplayTimer = chatBoxDisplayTime;

            _chatBoxInputField.ActivateInputField();
            if (Input.GetKeyDown(triggerChat))
            {
                if (_chatBoxInputField.text != "")
                {
                    _networkedPlayer.Rpc_SendChatBoxMessage(_chatBoxInputField.text);
                    _chatBoxInputField.text = "";
                }
                chatBoxState = ChatBoxState.READING;
            }
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}