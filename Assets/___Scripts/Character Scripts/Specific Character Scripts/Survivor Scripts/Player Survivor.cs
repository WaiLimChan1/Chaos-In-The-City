using UnityEngine;
using CITC.GameManager;
using Fusion;

public class PlayerSurvivor : Survivor
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Variables
    private Building _currentBuilding;
    private bool _insideBuilding;
    //------------------------------------------------------------------------------------------------------------------------------------------------


    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public override string Name { get { return NetworkedPlayer.PlayerName; } }
    public bool IsLocalPlayer { 
        get 
        {
            if (Runner == null) return false;
            return Runner.LocalPlayer == Object.InputAuthority; 
        } 
    }
    public bool InsideBuilding { get { return _insideBuilding; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Outfit Variables
    protected override void RandomizeOutfit()
    {
        _outfit.RandomizeOutfit(0);
        _outfitNetworked.CopyFrom(_outfit);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        base.Spawned();

        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllPlayerSurvivors, this.gameObject);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Input Functions
    protected override void TakeMovementDirectionInput() 
    {
        //Calculate _movementDirection
        movementDirection = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W)) movementDirection.y += 1;
        if (Input.GetKey(KeyCode.S)) movementDirection.y -= 1;
        if (Input.GetKey(KeyCode.D)) movementDirection.x += 1;
        if (Input.GetKey(KeyCode.A)) movementDirection.x -= 1;
        movementDirection = movementDirection.normalized;
    }

    protected override void TakeMouseWorldPositionInput() 
    {
        //Calculate _mouseWorldPosition
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
    }

    protected override void TakeChosenWeaponSlotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) if (Weapon.CanUse(primaryGun1)) chosenWeaponSlot = ChosenWeaponSlot.PrimaryGun1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) if (Weapon.CanUse(primaryGun2)) chosenWeaponSlot = ChosenWeaponSlot.PrimaryGun2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) if (Weapon.CanUse(sideArm1)) chosenWeaponSlot = ChosenWeaponSlot.SideArm1;
        if (Input.GetKeyDown(KeyCode.Alpha4)) if (Weapon.CanUse(sideArm2)) chosenWeaponSlot = ChosenWeaponSlot.SideArm2;
        if (Input.GetKeyDown(KeyCode.E)) chosenWeaponSlot = ChosenWeaponSlot.Melee;
        if (Input.GetKeyDown(KeyCode.Space)) if (Weapon.CanUse(throwable)) chosenWeaponSlot = ChosenWeaponSlot.Throwable;

        chosenWeaponSlotNetworked = chosenWeaponSlot;
        if (ChosenWeapon == null) chosenWeaponSlot = ChosenWeaponSlot.Melee;
    }

    protected override void TakeAttackInput()
    {
        if (Input.GetKey(KeyCode.Mouse0)) triggeredAttack = true;
        else triggeredAttack = false;
    }

    protected override void TakePunchInput()
    {
        if (Input.GetKey(KeyCode.Mouse1)) triggeredPunch = true;
        else triggeredPunch = false;
    }

    protected override void TakeReloadInput() 
    {
        if (Input.GetKey(KeyCode.R)) triggeredReload = true;
        else triggeredReload = false;
    }

    protected override void TakePickUpInput() 
    {
        if (Input.GetKey(KeyCode.F)) triggeredPickUp = true;
        else triggeredPickUp = false;
    }

    protected override void TakeDropChosenWeaponInput()
    {
        if (Input.GetKey(KeyCode.G)) triggeredDropChosenWeapon = true;
        else triggeredDropChosenWeapon = false;
    }

    public override void BeforeUpdate()
    {
        if (!Object.HasInputAuthority) return;
        if (ChatBoxGameUI.Instance.IsTyping) return;
        base.BeforeUpdate();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Update Network Functions
    protected void DetectReleaseAttack(CharacterData characterData)
    {
        if (Runner.IsResimulation) return;
        if (triggeredAttackNetworked && !characterData.TriggeredAttack)
        {
            releasedAttack = true;
            Debug.Log(gameObject.name + " Released Attack");
        }
        else releasedAttack = false;
    }

    protected override void UpdateNetworkedInputData()
    {
        if (Runner.TryGetInputForPlayer<CharacterData>(Object.InputAuthority, out var characterData))
        {
            DetectReleaseAttack(characterData);

            movementDirectionNetworked = characterData.MovementDirection;
            mouseWorldPositionNetworked = characterData.MouseWorldPosition;

            chosenWeaponSlotNetworked = characterData.ChosenWeaponSlot;
            triggeredAttackNetworked = characterData.TriggeredAttack;
            triggeredPunchNetworked = characterData.TriggeredPunch;
            triggeredReloadNetworked = characterData.TriggeredReload;
            triggeredPickUpNetworked = characterData.TriggeredPickUp; if (triggeredPickUpNetworked == false) alreadyPickedUp = false; //Reset
            triggeredDropChosenWeaponNetworked = characterData.TriggeredDropChosenWeapon; if (triggeredDropChosenWeaponNetworked == false) alreadyDroppedChosenWeapon = false; //Reset
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Building building = collision.gameObject.GetComponentInParent<Building>();
        if (building != null && IsLocalPlayer)
        {
            building.RevealInterior();
            _currentBuilding = building;
            _insideBuilding = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Building building = collision.gameObject.GetComponentInParent<Building>();
        if (building != null && IsLocalPlayer)
        {
            building.HideInterior();
            _currentBuilding = null;
            _insideBuilding = false;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Damage Functions
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        if (_currentBuilding != null && IsLocalPlayer) _currentBuilding.HideInterior();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Gizmos Functions
    private void OnDrawGizmosZones()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GameDirectorManager.Instance.PLAY_ZONE);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GameDirectorManager.Instance.SPAWN_ZONE);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, GameDirectorManager.Instance.DESTROY_ZONE);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        OnDrawGizmosZones();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
