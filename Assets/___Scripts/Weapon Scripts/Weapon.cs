using CITC.GameManager;
using Fusion;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Enum
    public enum WeaponType { Primary, SideArm }
    public enum WeaponStatus { Item, Using, Inventory }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Functions
    public static bool CanUse(Weapon weapon) { return weapon != null && weapon != default; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public const int NUM_OF_WEAPON_SORTING_ORDERS_BEHIND = 3;
    public const int NUM_OF_WEAPON_SORTING_ORDERS_IN_FRONT = 2;

    public const float INVENTORY_LERP_RATIO = 5f;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Component")]
    protected NetworkRigidbody2D networkRigidbody2d;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Transform interpolationTarget;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Weapon Info Positions")]
    [SerializeField] protected Transform infoPositions;
    [SerializeField] protected Transform holdPosition;
    [SerializeField] protected Transform inventoryHoldPosition;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Weapon Variables")]
    [SerializeField] protected float ownerSpeedMultiplier = 1;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Weapon Dynamic Variables")]
    [SerializeField][Networked] protected Character owner { get; set; }
    [SerializeField][Networked] protected Vector3 positionNetworked { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public string WeaponName { get { return gameObject.name.Remove(gameObject.name.Length - "(Clone)".Length); } }
    public Sprite WeaponImage { get { return spriteRenderer.sprite; } }

    public Character Owner { get { return owner; } }
    public bool IsFacingRight { get { return (transform.eulerAngles.z < 90 || transform.eulerAngles.z > 270); } }
    public bool IsFacingLeft { get { return (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270); } }
    public float SpeedMultiplier { get { return ownerSpeedMultiplier; } }

    public virtual void WeaponDropped()
    {
        owner = null;
        positionNetworked = this.transform.position;
        GameObjectListManager.Instance.Anchor(GameObjectListManager.Instance.AllWeapons, this.gameObject);
    }

    public void WeaponPickedUp(Survivor newOwner)
    {
        owner = newOwner;
    }

    [SerializeField] public WeaponStatus CurrentWeaponStatus
    {
        get
        {
            if (!Character.CanUse(owner)) return WeaponStatus.Item;
            else
            {
                if (owner.ChosenWeapon != null && owner.ChosenWeapon == this) return WeaponStatus.Using;
                else return WeaponStatus.Inventory;
            }
        }
    }

    public virtual string ClipSizeString { get { return ""; } }
    public virtual string AmmoString { get { return ""; } }
    
    public virtual string StringForm
    {
        get
        {
            string stats = "";
            return stats;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllWeapons, this.gameObject);

        networkRigidbody2d = GetComponent<NetworkRigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetUp(Vector3 spawnPosition)
    {
        positionNetworked = spawnPosition;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    protected void Update()
    {
        //Item on the floor
        if (CurrentWeaponStatus == WeaponStatus.Item)
        {
            interpolationTarget.gameObject.SetActive(true);

            GameObjectListManager.Instance.Anchor(GameObjectListManager.Instance.AllWeapons, this.gameObject);

            gameObject.layer = LayerMask.NameToLayer("Item");
            
            spriteRenderer.sortingLayerName = "Item";
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.flipY = false;

            transform.position = positionNetworked;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        //In Survivor's inventory
        else if (CurrentWeaponStatus == WeaponStatus.Inventory)
        {
            interpolationTarget.gameObject.SetActive(false);

            this.transform.parent = owner.InterpolationTarget.transform;

            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        //Being used as a weapon
        else if (CurrentWeaponStatus == WeaponStatus.Using)
        {
            interpolationTarget.gameObject.SetActive(true);

            this.transform.parent = owner.InterpolationTarget.transform;

            gameObject.layer = LayerMask.NameToLayer("Weapon");
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Item State
    protected virtual void LateUpdateItemState()
    {

    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    


    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Inventory State
    protected void LerpToInventoryPosition(Vector3 targetPosition, float ratio = INVENTORY_LERP_RATIO)
    {
        Vector3 changeVector = targetPosition - inventoryHoldPosition.position;
        changeVector = Vector3.Lerp(Vector3.zero, changeVector, ratio * Time.deltaTime);
        transform.position += changeVector;
    }

    protected void LerpToInventoryAngle(Vector3 finalEulerAngles, float ratio = INVENTORY_LERP_RATIO)
    {
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, finalEulerAngles, ratio * Time.deltaTime);
        transform.localEulerAngles = new Vector3(finalEulerAngles.x, finalEulerAngles.y, transform.localEulerAngles.z);
    }

    protected virtual void MoveToInventoryPosition() {}

    protected virtual void LateUpdateInventoryState()
    {
        MoveToInventoryPosition();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Using State
    protected void MoveToOwner()
    {
        Vector3 changeVector = owner.WeaponHoldPosition - holdPosition.position;
        transform.position += changeVector;
    }

    protected virtual void UpdatePosition()
    {
        MoveToOwner();
    }

    protected virtual void UpdateRotation()
    {
        //Rotate based on Owner mouse position
        transform.eulerAngles = new Vector3(0, 0, owner.WeaponRotationAngleDegrees);
    }

    protected virtual void UpdateSpriteRendererFlip()
    {
        //Update FlipY
        if (IsFacingRight) spriteRenderer.flipY = false;
        if (IsFacingLeft) spriteRenderer.flipY = true;
    }

    protected void UpdateSpriteRendererSorting()
    {
        //Sorting Order
        spriteRenderer.sortingLayerName = "Game Play";
        spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder + OutfitManager.NUM_OF_OUTFIT_TYPES + Weapon.NUM_OF_WEAPON_SORTING_ORDERS_IN_FRONT - 1;
    }

    protected virtual void UpdateInfoPositionsRotation()
    {

        if (IsFacingRight) infoPositions.localEulerAngles = new Vector3(0, 0, 0);
        if (IsFacingLeft) infoPositions.localEulerAngles = new Vector3(180, 0, 0);
    }

    protected virtual void LateUpdateUsingState()
    {
        UpdatePosition();
        UpdateRotation();
        UpdateSpriteRendererFlip();
        UpdateSpriteRendererSorting();
        UpdateInfoPositionsRotation();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //LateUpdate
    private void LateUpdateStateLogic()
    {
        switch (CurrentWeaponStatus)
        {
            case WeaponStatus.Item:
                LateUpdateItemState();
                break;
            case WeaponStatus.Inventory:
                LateUpdateInventoryState();
                break;
            case WeaponStatus.Using:
                LateUpdateUsingState();
                break;
        }
    }

    private void LateUpdate()
    {
        LateUpdateStateLogic();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Destruction
    public virtual void HandleDestruction()
    {
        Runner.Despawn(this.Object);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
