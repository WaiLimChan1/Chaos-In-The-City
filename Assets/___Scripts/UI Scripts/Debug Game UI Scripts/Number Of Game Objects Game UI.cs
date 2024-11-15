using TMPro;
using UnityEngine;
using CITC.GameManager;
using System.Diagnostics;

public class NumberOfGameObjectsGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static NumberOfGameObjectsGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private NetworkedPlayer _networkedPlayer;


    private Stopwatch _performanceUpdateCounter;
    private float _performanceUpdateInterval = 0.1f; //In Seconds
    private float _fps;
    private float _ping;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void SetUp(NetworkedPlayer networkedPlayer)
    {
        _networkedPlayer = networkedPlayer;

        _performanceUpdateCounter = new Stopwatch();
        _performanceUpdateCounter.Start();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    private void LateUpdate()
    {
        GameObjectListManager gameObjectListManager = GameObjectListManager.Instance;
        if (gameObjectListManager == null) return;

        if (_performanceUpdateCounter != null && _performanceUpdateCounter.ElapsedMilliseconds >= _performanceUpdateInterval * 1000)
        {
            _fps = Mathf.Ceil(1.0f / Time.deltaTime);
            if (NetworkedPlayer.CanUseNetworkedPlayer(_networkedPlayer)) _ping = (int)_networkedPlayer.GetPing();
            _performanceUpdateCounter.Restart();
        }

        _text.text = "FPS: " + _fps + " \n";
        if (NetworkedPlayer.CanUseNetworkedPlayer(_networkedPlayer)) _text.text += "Ping: " + _ping + " ms" + "\n\n";

        _text.text += "Hostile Survivors: " + gameObjectListManager.AllHostileSurvivors.List.Count + "\n";
        _text.text += "Zombies: " + gameObjectListManager.AllZombies.List.Count + "\n";
        _text.text += "Weapons: " + gameObjectListManager.AllWeapons.List.Count + "\n";
        _text.text += "Bullets: " + gameObjectListManager.AllBullets.List.Count + "\n";
        _text.text += "Visual Effects: " + gameObjectListManager.AllVisualEffects.List.Count + "\n";
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
