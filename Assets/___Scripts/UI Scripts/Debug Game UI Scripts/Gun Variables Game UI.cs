using TMPro;
using UnityEngine;

public class GunVariablesGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static GunVariablesGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [SerializeField] private TextMeshProUGUI _gunStatsText;
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
        _gunStatsText.text = "";
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
        if (!Weapon.CanUse(_networkedPlayer.OwnedCharacter.GetComponent<Character>().ChosenWeapon)) return;

        _gunStatsText.text = _networkedPlayer.OwnedCharacter.GetComponent<Character>().ChosenWeapon.StringForm;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
