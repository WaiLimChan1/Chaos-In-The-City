using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static GameUIManager Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Variable")]
    private NetworkedPlayer _localNetworkedPlayer;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public NetworkedPlayer LocalNetworkedPlayer { get { return _localNetworkedPlayer; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void SetUp(NetworkedPlayer localNetworkedPlayer)
    {
        _localNetworkedPlayer = localNetworkedPlayer;

        //Actual UI
        HealthBarGameUI.Instance.SetUp(localNetworkedPlayer);
        WeaponListGameUI.Instance.SetUp(localNetworkedPlayer);
        ReloadTimerGameUI.Instance.SetUp(localNetworkedPlayer);
        MinimapGameUI.Instance.SetUp(localNetworkedPlayer);
        ChatBoxGameUI.Instance.SetUp(localNetworkedPlayer);

        //Debug UI
        //GunVariablesGameUI.Instance.SetUp(localNetworkedPlayer);
        //PlayerSurvivorPositionGameUI.Instance.SetUp(localNetworkedPlayer);
        VehicleVariablesGameUI.Instance.SetUp(localNetworkedPlayer);
        NumberOfGameObjectsGameUI.Instance.SetUp(localNetworkedPlayer);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
