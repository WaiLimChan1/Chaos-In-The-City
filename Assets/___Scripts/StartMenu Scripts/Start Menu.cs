using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    // Mapping between region names and keys
    private static readonly Dictionary<string, string> regionMappings = new Dictionary<string, string>()
    {
        { "USA, East", "us" },
        { "USA, West", "usw" },
        { "Europe", "eu" },
        { "Asia", "asia" },
        { "Japan", "jp" },
        { "South America", "sa" },
        // Add more regions here as needed
    };

    // Method to get the region key based on the region name
    public static string GetRegionKey(string regionName)
    {
        if (regionMappings.TryGetValue(regionName, out string regionKey))
        {
            return regionKey;
        }

        return "us";
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Title")]
    [SerializeField] private TMP_Text _gameTitle;

    [Header("Singleplayer Interactive")]
    [SerializeField] private Button _playButton;

    [Header("Multiplayer Interactive")]
    [SerializeField] private TMP_Dropdown _chooseRegion;
    [SerializeField] private TMP_InputField _enterName;
    [SerializeField] private TMP_InputField _enterRoomCode;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRandomButton;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable()
    {
        GlobalManagers.Instance.NetworkRunnerController.OnStartedRunnerConnection += OnStartedRunnerConnection;
    }

    private void OnDisable()
    {
        GlobalManagers.Instance.NetworkRunnerController.OnStartedRunnerConnection -= OnStartedRunnerConnection;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnStartedRunnerConnection()
    {
        this.gameObject.SetActive(false);
    }

    private void StartGame(GameMode mode, string roomCode)
    {
        NetworkRunnerController NRC = GlobalManagers.Instance.NetworkRunnerController;

        string chosenRegionName = _chooseRegion.options[_chooseRegion.value].text;
        Debug.Log(GetRegionKey(chosenRegionName));
        NRC.StartGame(_enterName.text, GetRegionKey(chosenRegionName), mode, roomCode);

    }

    private void Start()
    {
        _playButton.onClick.AddListener(() => StartGame(GameMode.Single, _enterRoomCode.text));

        _joinRoomButton.onClick.AddListener(() => StartGame(GameMode.Client, _enterRoomCode.text));
        _createRoomButton.onClick.AddListener(() => StartGame(GameMode.Host, _enterRoomCode.text));
        _joinRandomButton.onClick.AddListener(() => StartGame(GameMode.AutoHostOrClient, string.Empty));
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}