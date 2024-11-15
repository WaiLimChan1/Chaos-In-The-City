using CITC.GameManager;
using Fusion;
using UnityEngine;

public class Throwable : Weapon
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Projectile Information")]
    [SerializeField] protected float projectileScale;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Projectile Explosion Information")]
    [SerializeField] protected float explosionDamage;
    [SerializeField] protected float explosionKnockBackForce;
    [SerializeField] protected float explosionRadius;
    [SerializeField] protected float detonationTime;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Throwable Throw Stats")]
    [SerializeField] protected float maxProjectileThrowDistance;
    [SerializeField] protected float maxProjectileThrowForce;
    [SerializeField] protected float maxMovementThrowForce;
    [SerializeField] protected float throwCoolDownTime;
    [Networked] protected TickTimer throwCoolDownTimer { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Throwable Ammo Stats")]
    [SerializeField][Networked] protected int currentAmount { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public override string AmmoString { get { return $"{currentAmount}"; } }
    public float CalculatedProjectileThrowForce
    {
        get
        {
            float movementThrowForce = this.maxMovementThrowForce * Vector3.Dot(owner.WeaponRotationDirection, owner.MovementDirection);
            float throwForceKeepRatio = Mathf.Clamp01(owner.TargetDistance / maxProjectileThrowDistance);
            return (this.maxProjectileThrowForce + movementThrowForce) * throwForceKeepRatio;
        }
    }
    public int CurrentAmount { get { return currentAmount; } set { currentAmount = value; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Update Network Functions
    private void ThrowServerLogic()
    {
        //Server creates Throwable Projectiles
        if (!Runner.IsServer) return;

        NetworkObject explosiveNetworkObject = NetworkedSpawnerManager.Instance.SpawnExplosive();
        ThrowableProjectile explosive = explosiveNetworkObject.GetComponent<ThrowableProjectile>();
        explosive.SetUp(owner, transform.position, projectileScale, owner.WeaponRotationAngleDegrees, CalculatedProjectileThrowForce, explosionDamage, explosionKnockBackForce, explosionRadius, detonationTime);

        if (currentAmount <= 0) HandleDestruction();
    }

    private void Throw()
    {
        if (!owner.IsAttacking) return; //Return if Owner is not attack
        if (!throwCoolDownTimer.ExpiredOrNotRunning(Runner)) return; //Can't throw if throw cool down is on
        if (currentAmount <= 0) return; //No more ammmo

        throwCoolDownTimer = TickTimer.CreateFromSeconds(Runner, throwCoolDownTime);
        currentAmount--;

        //Server Logic
        ThrowServerLogic();
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentWeaponStatus != WeaponStatus.Using) return;

        Throw();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Item State
    protected override void LateUpdateItemState()
    {
        base.LateUpdateItemState();
        interpolationTarget.gameObject.SetActive(true);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Inventory State
    protected override void MoveToInventoryPosition()
    {
        this.gameObject.transform.position = owner.transform.position;
    }

    protected override void LateUpdateInventoryState()
    {
        base.LateUpdateInventoryState();
        interpolationTarget.gameObject.SetActive(false);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Using State
    protected override void LateUpdateUsingState()
    {
        base.LateUpdateUsingState();
        interpolationTarget.gameObject.SetActive(true);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Destruction
    public override void HandleDestruction()
    {
        if (Character.CanUse(owner)) owner.ClearWeaponSlot(Character.ChosenWeaponSlot.Throwable);
        base.HandleDestruction();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Gizmos Functions
    protected virtual void OnDrawGizmos()
    {
        //projectileThrowMaxDistance
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, maxProjectileThrowDistance);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
