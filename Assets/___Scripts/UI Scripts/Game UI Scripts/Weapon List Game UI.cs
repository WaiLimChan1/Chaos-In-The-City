using UnityEngine;

public class WeaponListGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static WeaponListGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private WeaponGameUI _primaryGun1UI;
    [SerializeField] private WeaponGameUI _primaryGun2UI;
    [SerializeField] private WeaponGameUI _sideArm1UI;
    [SerializeField] private WeaponGameUI _sideArm2UI;
    [SerializeField] private WeaponGameUI _meleeUI;
    [SerializeField] private WeaponGameUI _throwableUI;

    [Header("Variable")]
    private NetworkedPlayer _networkedPlayer;
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
    private void InactivateAllWeaponGameUIs()
    {
        _primaryGun1UI.Inactivate();
        _primaryGun2UI.Inactivate();
        _sideArm1UI.Inactivate();
        _sideArm2UI.Inactivate();
        _meleeUI.Inactivate();
        _throwableUI.Inactivate();
    }
    
    private void UpdateWeaponGameUIs(Character localPlayerCharacter)
    {
        _primaryGun1UI.UpdateUI(localPlayerCharacter.PrimaryGun1, "1");
        _primaryGun2UI.UpdateUI(localPlayerCharacter.PrimaryGun2, "2");
        _sideArm1UI.UpdateUI(localPlayerCharacter.SideArm1, "3");
        _sideArm2UI.UpdateUI(localPlayerCharacter.SideArm2, "4");
        _meleeUI.UpdateUI(localPlayerCharacter.Melee, "E");
        _throwableUI.UpdateUI(localPlayerCharacter.Throwable, "Space");
    }

    private void UpdateCurrentChosenWeaponSlotGameUI(Character localPlayerCharacter)
    {

        switch (localPlayerCharacter.CurrentChosenWeaponSlot)
        {
            case Character.ChosenWeaponSlot.PrimaryGun1:
                _primaryGun1UI.UpdateChosenUI();
                break;
            case Character.ChosenWeaponSlot.PrimaryGun2:
                _primaryGun2UI.UpdateChosenUI();
                break;
            case Character.ChosenWeaponSlot.SideArm1:
                _sideArm1UI.UpdateChosenUI();
                break;
            case Character.ChosenWeaponSlot.SideArm2:
                _sideArm2UI.UpdateChosenUI();
                break;
            case Character.ChosenWeaponSlot.Melee:
                _meleeUI.UpdateChosenUI();
                break;
            case Character.ChosenWeaponSlot.Throwable:
                _throwableUI.UpdateChosenUI();
                break;

        }
    }

    private void LateUpdate()
    {
        InactivateAllWeaponGameUIs();
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
        if (_networkedPlayer.OwnedCharacter.GetComponent<Character>() == null) return;

        Character localPlayerCharacter = _networkedPlayer.OwnedCharacter.GetComponent<Character>();

        UpdateWeaponGameUIs(localPlayerCharacter);
        UpdateCurrentChosenWeaponSlotGameUI(localPlayerCharacter);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
