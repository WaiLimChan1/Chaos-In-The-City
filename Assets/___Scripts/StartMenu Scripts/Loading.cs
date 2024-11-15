using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [Header("Loading Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private Button _cancelButton;

    private NetworkRunnerController _networkRunnerController;
    private float _maxWaitTime = 100;
    private float _timeCounter;

    private void Awake()
    {
        _networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;
        _cancelButton.onClick.AddListener(_networkRunnerController.ShutDownRunner);
        _content.SetActive(false);
    }

    private void OnEnable()
    {
        _networkRunnerController.OnStartedRunnerConnection += OnStartedRunnerConnection;
    }

    private void OnDisable()
    {
        _networkRunnerController.OnStartedRunnerConnection -= OnStartedRunnerConnection;
    }

    void DetermineDescriptionText()
    {
        _descriptionText.text = _networkRunnerController.LocalPlayerName + " is ";

        GameMode localGameMode = _networkRunnerController.LocalGameMode;
        if (localGameMode == GameMode.Client) _descriptionText.text += "Joining Room " + _networkRunnerController.RoomCode;
        else if (localGameMode == GameMode.Host) _descriptionText.text += "Hosting Room " + _networkRunnerController.RoomCode;
        else if (localGameMode == GameMode.AutoHostOrClient) _descriptionText.text += "Joining Random Room";
    }

    private void OnStartedRunnerConnection()
    {
        _content.SetActive(true);
        DetermineDescriptionText();
        _timeCounter = _maxWaitTime;
    }

    private void HandleLoadingTimerLogic()
    {
        if (!_content.activeInHierarchy) return;
        _loadingText.text = "Loading... (" + (int)_timeCounter + "s)";
        _timeCounter -= Time.deltaTime;
        if (_timeCounter < 0) _timeCounter = 0;
        if (_timeCounter <= 0) _cancelButton.onClick.Invoke();
    }

    private void Update()
    {
        HandleLoadingTimerLogic();
    }
}