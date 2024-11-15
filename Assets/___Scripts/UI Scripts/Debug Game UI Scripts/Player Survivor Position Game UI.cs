using CITC.GameManager;
using TMPro;
using UnityEngine;

public class PlayerSurvivorPositionGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static PlayerSurvivorPositionGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [SerializeField] private TextMeshProUGUI _playerSurvivorPositions;
    [SerializeField] private NetworkedPlayer _networkedPlayer;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void SetUp(NetworkedPlayer networkedPlayer)
    {
        _networkedPlayer = networkedPlayer;
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
        _playerSurvivorPositions.text = "";

        foreach (GameObject gameObject in GameObjectListManager.Instance.AllNetworkedPlayers.List)
        {
            //No Game Object
            if (gameObject == null || gameObject == default) continue;

            //No Networked Player
            NetworkedPlayer networkedPlayer = gameObject.GetComponent<NetworkedPlayer>();
            if (!NetworkedPlayer.CanUseNetworkedPlayer(networkedPlayer)) continue;

            _playerSurvivorPositions.text += $"{networkedPlayer.PlayerName} - ";


            //Networked Player Does Not Have Character
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(networkedPlayer))
            {
                _playerSurvivorPositions.text += "\n";
                continue;
            }

            //No Character
            Character character = networkedPlayer.OwnedCharacter.GetComponent<Character>();
            if (!Character.CanUse(character))
            {
                _playerSurvivorPositions.text += "\n";
                continue;
            }

            _playerSurvivorPositions.text += $"Position: ({(int)character.transform.position.x},  {(int)character.transform.position.y})\n";
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
