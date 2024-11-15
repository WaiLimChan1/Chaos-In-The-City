using static CITC.GameManager.SingleAnimationClipManager;
using UnityEngine;

public class Melee : Weapon
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Melee Enums
    public enum MeleeStatus { HOLD, ATTACK }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Melee Component")]
    private MeleeAnimationController animationController;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Melee Attack Stats")]
    [SerializeField] protected MeleeAttackAnimationType meleeAttackAnimationType;
    [SerializeField] protected SingleAnimationType meleeHitTargetVFX;
    [SerializeField] protected float meleeAttackSpeed;
    [SerializeField] protected BoxCollider2D meleeAttackBox;
    [SerializeField] protected float meleeDamage;
    [SerializeField] protected float meleeKnockBackForce;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Melee Variables")]
    [SerializeField] private MeleeStatus _meleeStatus;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        base.Spawned();
        animationController = GetComponentInChildren<MeleeAnimationController>();
        animationController.ChangeAnimationClips(meleeAttackAnimationType);
        animationController.AnimationSpeed = meleeAttackSpeed;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Update Network Functions
    private void CancelAttack()
    {
        _meleeStatus = MeleeStatus.HOLD;
        animationController.UpdateStatus((int)_meleeStatus);
    }

    private void Attack()
    {
        if (!owner.IsAttacking) return; //Return if Owner is not attack
        if (_meleeStatus == MeleeStatus.ATTACK) return; //Already attacking
        _meleeStatus = MeleeStatus.ATTACK;
    }

    public void TriggerAttack()
    {
        if (!Character.CanUse(owner)) return;

        //Hitting Characters
        Collider2D[] colliders = Physics2D.OverlapBoxAll(meleeAttackBox.bounds.center, meleeAttackBox.bounds.size, meleeAttackBox.gameObject.transform.eulerAngles.z, LayerMask.GetMask("Character Full Hitbox"));
        foreach (Collider2D collider in colliders)
        {
            Character character = collider.GetComponentInParent<Character>();
            if (!Character.CanUse(character)) continue;
            if (character == owner) continue;

            CharacterHitbox targetHitbox = collider.GetComponent<CharacterFullHitbox>();
            if (targetHitbox == null) continue;

            Vector3 direction = (targetHitbox.transform.position - owner.gameObject.transform.position).normalized;
            targetHitbox.TakeHit(meleeHitTargetVFX, targetHitbox.transform.position, meleeDamage, meleeKnockBackForce * direction);
        }

        //Hitting Vehicles
        Collider2D[] vehicleColliders = Physics2D.OverlapBoxAll(meleeAttackBox.bounds.center, meleeAttackBox.bounds.size, meleeAttackBox.gameObject.transform.eulerAngles.z, LayerMask.GetMask("Vehicle"));
        foreach (Collider2D collider in vehicleColliders)
        {
            Vehicle vehicle = collider.GetComponent<Vehicle>();
            if (!Vehicle.CanUse(vehicle)) continue;
            if (vehicle.IsAPassenger(owner)) continue; //Melee owner is a passenger of the current vehicle

            Vector3 direction = (vehicle.transform.position - owner.gameObject.transform.position).normalized;
            vehicle.TakeHit(meleeDamage, meleeKnockBackForce * direction, SingleAnimationType.GetPunchedVFX, vehicle.transform.position, vehicle.InterpolationTarget.transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentWeaponStatus != WeaponStatus.Using) return;
        Attack();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Item State
    protected override void LateUpdateItemState()
    {
        CancelAttack();
        base.LateUpdateItemState();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Inventory State
    protected override void MoveToInventoryPosition()
    {
        spriteRenderer.sortingLayerName = "Game Play";
        spriteRenderer.flipY = false;
        spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder - 4;

        LerpToInventoryPosition(owner.MeleeInventoryPosition);

        if (owner.IsFacingRight) transform.localEulerAngles = new Vector3(0, 0, 45);
        if (owner.IsFacingLeft) transform.localEulerAngles = new Vector3(0, 180, 45);
    }

    protected override void LateUpdateInventoryState()
    {
        CancelAttack();
        base.LateUpdateInventoryState();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Using State
    private void UpdateAnimation()
    {
        animationController.UpdateStatus((int)_meleeStatus);
        if (animationController.NormalizedTime >= 1) _meleeStatus = MeleeStatus.HOLD;
    }

    protected override void UpdateSpriteRendererFlip()
    {
        //Update FlipY
        if (IsFacingRight) transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z);
        if (IsFacingLeft) transform.localEulerAngles = new Vector3(0, 180, 180 - transform.localEulerAngles.z);
    }

    protected override void UpdateInfoPositionsRotation()
    {

    }

    protected override void LateUpdateUsingState()
    {
        UpdateAnimation();
        base.LateUpdateUsingState();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
