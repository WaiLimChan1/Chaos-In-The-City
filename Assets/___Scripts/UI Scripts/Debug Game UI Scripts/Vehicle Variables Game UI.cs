using CITC.GameManager;
using TMPro;
using UnityEngine;

public class VehicleVariablesGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static VehicleVariablesGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [SerializeField] private TextMeshProUGUI _vehicleVariablesText;
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
        _vehicleVariablesText.text = "";
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
        if (!Vehicle.CanUse(_networkedPlayer.OwnedCharacter.GetComponent<Character>().CurrentVehicle)) return;

        _vehicleVariablesText.text = _networkedPlayer.OwnedCharacter.GetComponent<Character>().CurrentVehicle.StringForm;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
