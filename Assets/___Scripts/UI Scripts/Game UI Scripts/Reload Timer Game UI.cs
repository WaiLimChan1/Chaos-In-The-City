using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadTimerGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static ReloadTimerGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Variable")]
    private NetworkedPlayer _networkedPlayer;

    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Image _reloadCircle;
    [SerializeField] private TextMeshProUGUI _remainingReloadTime;
    //------------------------------------------------------------------------------------------------------------------------------------------------
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
        _content.gameObject.SetActive(false);
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
        if (!Weapon.CanUse(_networkedPlayer.OwnedCharacter.GetComponent<Character>().ChosenWeapon)) return;
        if (_networkedPlayer.OwnedCharacter.GetComponent<Character>().ChosenWeapon.GetComponent<Gun>() == null) return;

        Gun gun = _networkedPlayer.OwnedCharacter.GetComponent<Character>().ChosenWeapon.GetComponent<Gun>();
        if (!gun.IsReloading) return;

        _content.gameObject.SetActive(true);
        if (gun.ReloadTime != 0) _reloadCircle.fillAmount = gun.RemainingReloadTime / gun.ReloadTime;
        _remainingReloadTime.text = gun.remainingReloadTimeString;

    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
