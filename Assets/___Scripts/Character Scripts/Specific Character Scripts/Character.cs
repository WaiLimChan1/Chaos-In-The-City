using UnityEngine;
using Fusion;
using CITC.GameManager;
using System.Collections.Generic;
using static CITC.GameManager.SingleAnimationClipManager;

public class Character : NetworkBehaviour, IBeforeUpdate
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Enums
    public enum Status { IDLE, WALK }
    public enum ChosenWeaponSlot { PrimaryGun1, PrimaryGun2, SideArm1, SideArm2, Melee, Throwable, Null }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static
    public static bool CanUse(Character character) { return character != null && character != default && character.Runner != null; }

    public const int NUM_SORTING_ORDER = Weapon.NUM_OF_WEAPON_SORTING_ORDERS_BEHIND + OutfitManager.NUM_OF_OUTFIT_TYPES + Weapon.NUM_OF_WEAPON_SORTING_ORDERS_IN_FRONT;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    #region Character Variables
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Character Components")]
    private NetworkRigidbody2D _networkRigidBody2D;
    private Rigidbody2D _rigidbody2D;
    private float _rbDrag = 5.0f;
    private Collider2D _collider2D;
    [Networked] private NetworkedPlayer _networkedPlayer { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Character Stats")]
    [SerializeField] protected float _maxHealth;
    [SerializeField] [Networked] protected float _health { get; set; }
    [SerializeField] private float _movementSpeed = 10;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Character Local Variables")]
    private Status _status;
    private bool _isFacingLeft;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Character Networked Inputs")]
    [Networked] protected Vector2 movementDirectionNetworked { get; set; }
    protected Vector2 movementDirection;

    [Networked] protected Vector3 mouseWorldPositionNetworked { get; set; }
    protected Vector3 mouseWorldPosition;

    [Networked] protected ChosenWeaponSlot chosenWeaponSlotNetworked { get; set; }
    protected ChosenWeaponSlot chosenWeaponSlot;

    [Networked] protected bool triggeredAttackNetworked { get; set; }
    protected bool triggeredAttack;
    protected bool releasedAttack;

    [SerializeField] [Networked] protected bool triggeredPunchNetworked { get; set; }
    [SerializeField] protected bool triggeredPunch;

    [Networked] protected bool triggeredReloadNetworked { get; set; }
    protected bool triggeredReload;

    [Networked] protected bool triggeredPickUpNetworked { get; set; }
    protected bool triggeredPickUp;
    protected bool alreadyPickedUp; //To Achieve Key Down Effect For Pick Up
    
    [Networked] protected bool triggeredDropChosenWeaponNetworked { get; set; }
    protected bool triggeredDropChosenWeapon;
    protected bool alreadyDroppedChosenWeapon; //To Achieve Key Down Effect For Dropping
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public bool ActiveAnimation { get { return _outfitAC.gameObject.activeInHierarchy; } set { _outfitAC.gameObject.SetActive(value); } }
    public GameObject InterpolationTarget { get { return _networkRigidBody2D.InterpolationTarget.gameObject; } }
    public Vector3 InterpolationTargetPosition { get { return InterpolationTarget.transform.position; } }
    public Vector3 CameraPosition 
    { 
        get 
        {
            if (Vehicle.CanUse(currentVehicle)) return currentVehicle.GetPassengerSeatPosition(this);
            else return InterpolationTargetPosition; 
        } 
    }
    public bool ColliderIsTrigger { get { return _collider2D.isTrigger; } set { _collider2D.isTrigger = value; } }
    public NetworkedPlayer NetworkedPlayer { get { return _networkedPlayer; } set { _networkedPlayer = value; } }
    public virtual string Name { get { return ""; } }
    public int PlayerId { get { return NetworkedPlayer.PlayerId; } }
    public Color CharacterColor { get { return NetworkedPlayer.PlayerColor; } }
    public float HealthRatio { get { return _health / _maxHealth; } }
    public bool IsDead { get { return _health <= 0; } }
    public float MovementSpeed
    {
        get
        {
            float movementSpeed = Mathf.Max(_movementSpeed - _rigidbody2D.velocity.magnitude, 0);
            if (Weapon.CanUse(ChosenWeapon)) movementSpeed *= ChosenWeapon.SpeedMultiplier;
            return movementSpeed;
        }
    }
    public Vector2 MovementDirection { get { return movementDirectionNetworked; } }
    public ChosenWeaponSlot CurrentChosenWeaponSlot { get { return chosenWeaponSlot; } }
    public bool IsAttacking { get { return triggeredAttackNetworked; } }
    public bool IsPunching { get { return triggeredPunchNetworked; } }
    public bool IsReloading { get { return triggeredReloadNetworked; } }
    public bool IsPickingUp { get { return triggeredPickUpNetworked && !alreadyPickedUp; } }
    public bool IsDroppingChosenWeapon { get { return triggeredDropChosenWeaponNetworked && !alreadyDroppedChosenWeapon; } }

    public bool IsFacingLeft { get { return _isFacingLeft; } }
    public bool IsFacingRight { get { return !_isFacingLeft; } }

    protected void OnDrawGizmosInput()
    {
        //Drawing Position
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        //Drawing Movement Direction
        Gizmos.color = Color.black;
        Vector2 changeVector = movementDirectionNetworked * MovementSpeed * Runner.DeltaTime;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(changeVector.x, changeVector.y, 0) * 10);

        //Drawing Mouse Position
        if (triggeredAttackNetworked) Gizmos.color = Color.red;
        else if (triggeredPunchNetworked) Gizmos.color = Color.blue;
        else Gizmos.color = Color.black;
        Gizmos.DrawLine(WeaponHoldPosition, mouseWorldPositionNetworked);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Outfit
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Outfit Variables & Functions
    [UnityHeader("Character Outfit Variables")]
    public CharacterOutfitAnimationController _outfitAC { get; private set; }
    protected OutfitNetworked _outfitNetworked;
    protected OutfitManager.Outfit _outfit;

    protected virtual void RandomizeOutfit() { }

    private void UpdateOutfit()
    {
        if (_outfitNetworked == null || _outfitNetworked == default) return;
        _outfit.CopyFrom(_outfitNetworked);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Weapon
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Character Weapons")]
    [SerializeField] [Networked] protected Weapon primaryGun1 { get; set; }
    [SerializeField] [Networked] protected Weapon primaryGun2 { get; set; }
    [SerializeField] [Networked] protected Weapon sideArm1 { get; set; }
    [SerializeField] [Networked] protected Weapon sideArm2 { get; set; }
    [SerializeField] [Networked] protected Weapon melee { get; set; }
    [SerializeField] [Networked] protected Weapon throwable { get; set; }

    [SerializeField] protected float PickUpRadius;

    public Weapon PrimaryGun1 { get { return primaryGun1; } }
    public Weapon PrimaryGun2 { get { return primaryGun2; } }
    public Weapon SideArm1 { get { return sideArm1; } }
    public Weapon SideArm2 { get { return sideArm2; } }
    public Weapon Melee { get { return melee; } }
    public Weapon Throwable { get { return throwable; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Character Weapon Positions
    [Header("Character Weapon Position")]
    [SerializeField] private Transform _weaponHoldPosition;
    [SerializeField] private Transform _primaryWeaponInventoryPosition;
    [SerializeField] private Transform _sideArm1InventoryPosition;
    [SerializeField] private Transform _sideArm2InventoryPosition;
    [SerializeField] private Transform _meleeInventoryPosition;

    public Vector3 WeaponHoldPosition { get { return _weaponHoldPosition.position; } }
    public Vector3 PrimaryWeaponInventoryPosition { get { return _primaryWeaponInventoryPosition.position; } }
    public Vector3 SideArm1InventoryPosition { get { return _sideArm1InventoryPosition.position; } }
    public Vector3 SideArm2InventoryPosition { get { return _sideArm2InventoryPosition.position; } }
    public Vector3 MeleeInventoryPosition { get { return _meleeInventoryPosition.position; } }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Character Weapon Helper Functions
    public Weapon ChosenWeapon
    {
        get
        {
            if (chosenWeaponSlotNetworked == ChosenWeaponSlot.PrimaryGun1) return primaryGun1;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.PrimaryGun2) return primaryGun2;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.SideArm1) return sideArm1;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.SideArm2) return sideArm2;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.Melee) return melee;
            else if (chosenWeaponSlotNetworked == ChosenWeaponSlot.Throwable) return throwable;
            else return null;
        }
    }
    public Vector3 PickUpPosition { get { return WeaponHoldPosition;  } }
    public Vector3 NonInterpolatedWeaponHoldPosition { get { return transform.position + _weaponHoldPosition.localPosition; } }
    public float TargetDistance { get { return Vector2.Distance(mouseWorldPositionNetworked, NonInterpolatedWeaponHoldPosition); } }
    public virtual float WeaponRotationAngleDegrees
    {
        get
        {
            Vector3 directionToMouse = (mouseWorldPositionNetworked - NonInterpolatedWeaponHoldPosition).normalized;

            float rotationInRadians = Mathf.Atan2(directionToMouse.y, directionToMouse.x);
            float angleInDegrees = rotationInRadians * Mathf.Rad2Deg;

            return angleInDegrees;
        }
    }
    public Vector3 WeaponRotationDirection
    {
        get 
        {
            float angleInRadians = WeaponRotationAngleDegrees * Mathf.Deg2Rad;
            Vector3 weaponRotationDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
            return weaponRotationDirection.normalized;
        }
    }

    public ChosenWeaponSlot WhichGunSlot(Gun gun)
    {
        if (gun == primaryGun1) return ChosenWeaponSlot.PrimaryGun1;
        else if (gun == primaryGun2) return ChosenWeaponSlot.PrimaryGun2;
        else if (gun == sideArm1) return ChosenWeaponSlot.SideArm1;
        else if (gun == sideArm2) return ChosenWeaponSlot.SideArm2;
        else return ChosenWeaponSlot.Null;
    }

    public void ClearWeaponSlot(Survivor.ChosenWeaponSlot weaponSlot)
    {
        if (weaponSlot == ChosenWeaponSlot.PrimaryGun1 && primaryGun1 != null)
        {
            primaryGun1.WeaponDropped();
            primaryGun1 = null;
        }
        else if (weaponSlot == ChosenWeaponSlot.PrimaryGun2 && primaryGun2 != null)
        {
            primaryGun2.WeaponDropped();
            primaryGun2 = null;
        }
        else if (weaponSlot == ChosenWeaponSlot.SideArm1 && sideArm1 != null)
        {
            sideArm1.WeaponDropped();
            sideArm1 = null;
        }
        else if (weaponSlot == ChosenWeaponSlot.SideArm2 && sideArm2 != null)
        {
            sideArm2.WeaponDropped();
            sideArm2 = null;
        }
        else if (weaponSlot == ChosenWeaponSlot.Melee && melee != null)
        {
            melee.WeaponDropped();
            melee = null;
        }
        else if (weaponSlot == ChosenWeaponSlot.Throwable && throwable != null)
        {
            throwable.WeaponDropped();
            throwable = null;
        }
    }

    protected void DropAllWeapons()
    {
        ClearWeaponSlot(ChosenWeaponSlot.PrimaryGun1);
        ClearWeaponSlot(ChosenWeaponSlot.PrimaryGun2);
        ClearWeaponSlot(ChosenWeaponSlot.SideArm1);
        ClearWeaponSlot(ChosenWeaponSlot.SideArm2);
        ClearWeaponSlot(ChosenWeaponSlot.Melee);
        ClearWeaponSlot(ChosenWeaponSlot.Throwable);
    }

    protected void OnDrawGizmosWeapon()
    {
        //Drawing Item Pick Up Radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(PickUpPosition, PickUpRadius);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Punch
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Character Punch Information")]
    [SerializeField] protected SingleAnimationClipManager.SingleAnimationType PunchVFX;
    [SerializeField] protected SingleAnimationClipManager.SingleAnimationType GetPunchedVFX;

    [SerializeField] protected BoxCollider2D PunchAttackBox;
    [SerializeField] private float _punchDamage;
    [SerializeField] private float _punchKnockBackForce;
    [SerializeField] private float _punchCoolDownTime;
    private TickTimer _punchCoolDownTimer;

    [SerializeField] protected float _punchRange;

    protected virtual void PunchedVisualEffect()
    {
        LocalSpawnerManager.CreateVisualEffectInstance(PunchVFX, PunchAttackBox.bounds.center, 1, WeaponRotationAngleDegrees, sortingOrder: 1, parent: InterpolationTarget.transform);
    }

    protected virtual Character PunchedColliderCheck(Collider2D collider)
    {
        return collider.GetComponentInParent<Character>();
    }

    protected List<Character> PunchedCharacters()
    {
        List<Character> punchedCharacters = new List<Character>();

        Collider2D[] colliders = Physics2D.OverlapBoxAll(PunchAttackBox.bounds.center, PunchAttackBox.bounds.size, PunchAttackBox.gameObject.transform.eulerAngles.z, LayerMask.GetMask("Character Full Hitbox"));
        foreach (Collider2D collider in colliders)
        {
            Character character = PunchedColliderCheck(collider);
            if (!CanUse(character)) continue;
            if (character == this) continue;

            punchedCharacters.Add(character);
        }

        return punchedCharacters;
    }

    protected void HandlePunch()
    {
        PunchAttackBox.gameObject.transform.eulerAngles = new Vector3(0, 0, WeaponRotationAngleDegrees);

        if (Runner.IsResimulation) return;
        if (!IsPunching) return; //Not Punching
        if (!_punchCoolDownTimer.ExpiredOrNotRunning(Runner)) return; //Punch is on cool down

        PunchedVisualEffect();

        //Punched Characters
        List<Character> punchedCharacters = PunchedCharacters();
        foreach (Character punchedCharacter in punchedCharacters)
        {
            CharacterHitbox targetHitbox = punchedCharacter.GetComponentInChildren<CharacterFullHitbox>();
            if (targetHitbox == null) continue;

            Vector3 direction = (targetHitbox.transform.position - this.transform.position).normalized;
            targetHitbox.TakeHit(GetPunchedVFX, targetHitbox.transform.position, _punchDamage, _punchKnockBackForce * direction, punchedCharacter.InterpolationTarget.transform);
        }

        //Punched Vehicle
        Collider2D[] vehicleColliders = Physics2D.OverlapBoxAll(PunchAttackBox.bounds.center, PunchAttackBox.bounds.size, PunchAttackBox.gameObject.transform.eulerAngles.z, LayerMask.GetMask("Vehicle"));
        foreach (Collider2D collider in vehicleColliders)
        {
            Vehicle vehicle = collider.GetComponent<Vehicle>();
            if (!Vehicle.CanUse(vehicle)) continue;
            if (vehicle.IsAPassenger(this)) continue; //Can't punch your own car

            Vector3 direction = (vehicle.transform.position - this.transform.position).normalized;
            vehicle.TakeHit(_punchDamage, _punchKnockBackForce * direction, SingleAnimationType.GetPunchedVFX, vehicle.transform.position, vehicle.InterpolationTarget.transform);
        }
        _punchCoolDownTimer = TickTimer.CreateFromSeconds(Runner, _punchCoolDownTime);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Vehicle
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Vehicle Information")]
    [SerializeField] [Networked] protected Vehicle currentVehicle { get; set; }
    public Vehicle CurrentVehicle { get { return currentVehicle; } set { currentVehicle = value; } }
    private void CalculateVehicleLocalVariables()
    {
        if (Vehicle.CanUse(currentVehicle)) //In Vehicle
        {
            ColliderIsTrigger = true;
        }
        else //Not In Vehicle
        {
            ColliderIsTrigger = false;
        }
    }

    private void CalculateInVehicleLocalVisual()
    {
        if (Vehicle.CanUse(currentVehicle)) //In Vehicle
        {
            ActiveAnimation = false;
        }
        else //Not In Vehicle
        {
            //ActiveAnimation = true; //Handled By Animation Culling Manager
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        _networkRigidBody2D = GetComponent<NetworkRigidbody2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.drag = _rbDrag;
        _collider2D = GetComponent<Collider2D>();

        _outfitAC = GetComponentInChildren<CharacterOutfitAnimationController>();
        _outfitNetworked = GetComponentInChildren<OutfitNetworked>();
        _outfit = new OutfitManager.Outfit();

        if (Runner.IsServer)
        {
            RandomizeOutfit();
            _health = _maxHealth;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Input Functions
    protected virtual void TakeMovementDirectionInput() {}
    protected virtual void TakeMouseWorldPositionInput() {}

    protected virtual void TakeChosenWeaponSlotInput() {}
    protected virtual void TakeAttackInput() {}
    protected virtual void TakePunchInput() {}
    protected virtual void TakeReloadInput() {}
    protected virtual void TakePickUpInput() {}
    protected virtual void TakeDropChosenWeaponInput() {}

    public virtual void BeforeUpdate()
    {
        TakeMovementDirectionInput();
        TakeMouseWorldPositionInput();

        TakeChosenWeaponSlotInput();
        TakeAttackInput();
        TakePunchInput();
        TakeReloadInput();
        TakePickUpInput();
        TakeDropChosenWeaponInput();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Update Network Functions
    public CharacterData GetCharacterData()
    {
        CharacterData data = new CharacterData();

        data.MovementDirection = movementDirection;
        data.MouseWorldPosition = mouseWorldPosition;

        data.ChosenWeaponSlot = chosenWeaponSlot;
        data.TriggeredAttack = triggeredAttack;
        data.TriggeredPunch = triggeredPunch;
        data.TriggeredReload = triggeredReload;
        data.TriggeredPickUp = triggeredPickUp;
        data.TriggeredDropChosenWeapon = triggeredDropChosenWeapon;

        return data;
    }

    protected virtual void UpdateNetworkedInputData()
    {
        movementDirectionNetworked = movementDirection;
        mouseWorldPositionNetworked = mouseWorldPosition;

        chosenWeaponSlotNetworked = chosenWeaponSlot;
        triggeredAttackNetworked = triggeredAttack;
        triggeredPunchNetworked = triggeredPunch;
        triggeredReloadNetworked = triggeredReload;
        triggeredPickUpNetworked = triggeredPickUp;
        triggeredDropChosenWeaponNetworked = triggeredDropChosenWeapon;
    }

    private void CalculateLocalVariables()
    {
        if (movementDirectionNetworked.magnitude > 0) _status = Status.WALK;
        else _status = Status.IDLE;

        if (WeaponRotationAngleDegrees > 90 || WeaponRotationAngleDegrees < -90) _isFacingLeft = true;
        if (WeaponRotationAngleDegrees < 90 && WeaponRotationAngleDegrees > -90) _isFacingLeft = false;

        CalculateVehicleLocalVariables();
    }

    private void Move()
    {
        Vector2 changeVector = movementDirectionNetworked * MovementSpeed * Runner.DeltaTime;
        transform.position += new Vector3(changeVector.x, changeVector.y, 0);
    }

    protected virtual void PickUpItem() {}
    protected virtual void DropChosenWeapon() {}
    protected virtual void EnterVehicle() { }
    protected virtual void ExitVehicle() { }
    public override void FixedUpdateNetwork()
    {
        UpdateNetworkedInputData();

        if (Runner.IsServer || Object.HasInputAuthority)
        {
            Move();
            PickUpItem();
            DropChosenWeapon();
            EnterVehicle();
            ExitVehicle();
        }

        CalculateLocalVariables();
        HandlePunch();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Update
    public void FixedUpdate()
    {
        //Disable colliders if too many characters are around
        Collider2D[] nearbyCharacters = Physics2D.OverlapCircleAll(transform.position, 3.0f, LayerMask.GetMask("Character"));
        if (nearbyCharacters.Length > 20) _collider2D.enabled = false;
        else _collider2D.enabled = true;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Local Visual Functions
    public void LateUpdate()
    {
        UpdateOutfit();
        _outfitAC.UpdateAnimation(_status, _isFacingLeft, _outfit);
        CalculateInVehicleLocalVisual();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Damage Functions
    public void KnockBack(Vector2 force)
    {
        if (!CanUse(this)) return;
        _rigidbody2D.AddForce(force * GlobalConstants.FORCE_MULTIPLIER);
    }

    public void TakeDamage(float damage, Vector2 knockBackForce = default(Vector2))
    {
        _health -= damage;
        if (_health <= 0)
        {
            HandleDeath();
        }
        else
        {
            KnockBack(knockBackForce);
        }
    }

    public void HandleDeath()
    {
        Runner.Despawn(this.Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        LocalSpawnerManager.CreateVisualEffectInstance(SingleAnimationClipManager.SingleAnimationType.DeathBlood, this.transform.position);

        DropAllWeapons();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Gizmos Functions
    protected virtual void OnDrawGizmos()
    {
        OnDrawGizmosInput();
        OnDrawGizmosWeapon();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
