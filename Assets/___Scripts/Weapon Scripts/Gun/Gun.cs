using CITC.GameManager;
using Fusion;
using UnityEngine;
using static CITC.GameManager.SingleAnimationClipManager;

public class Gun : Weapon
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Info Positions")]
    [SerializeField] private Transform _firePosition;
    [SerializeField] protected Transform bulletCasingSpawnPosition;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Type Stats")]
    [SerializeField] protected WeaponType weaponType; 
    [SerializeField] protected SingleAnimationType fireMuzzleFx;
    [SerializeField] protected BulletCasing.BulletCasingType bulletCasingType;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Bullet Information")]
    [SerializeField] protected float bulletDamage;
    [SerializeField] protected float bulletHeadShotMultiplier;
    [SerializeField] protected int bulletPierceAmount;
    [SerializeField] protected float bulletKnockBackForce;
    [SerializeField] protected float bulletSpeed;
    [SerializeField] protected float bulletMaxRange;
    [SerializeField] protected float bulletSpread;
    [SerializeField] protected float bulletScale;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Bullet Casing Information")]
    [SerializeField] protected bool spawnCasingWhileFiring;
    protected int bulletCasingCount;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Fire Stats")]
    [SerializeField] protected float fireSpeed;
    [Networked] protected TickTimer fireCoolDown { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Recoil Stats")]
    [SerializeField] protected float recoilDistance;
    [SerializeField] protected float recoilTime;
    protected TickTimer recoilTimer;
    protected Vector3 recoilOffset;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Reload Stats")]
    [SerializeField] protected bool singleBulletReload;
    [Networked] protected bool reloading { get; set; }
    [SerializeField] protected float reloadTime;
    [Networked] protected TickTimer reloadTimer { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Ammo Stats")]
    [SerializeField] protected int clipSize;
    [SerializeField] [Networked] protected int currentClipSize { get; set; }
    [SerializeField] [Networked] protected int extraAmmo { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Gun Unique Attributes")]
    [SerializeField] protected int bulletsPerShot;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Gun Lag Compensation Variables")]

    [Networked] protected int numBeginReloadTimesNetworked { get; set; }
    protected int numBeginReloadTimesLocal;

    [Networked] protected int numFireTimesNetworked { get; set; }
    protected int numFireTimesLocal;
    protected int numSpawnedBulletCasingLocal;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public WeaponType GetWeaponType { get { return weaponType; } }
    public bool IsReloading { get { return reloading; } }
    public float ReloadTime { get { return reloadTime; } }
    public float RemainingReloadTime {  get { return reloadTimer.RemainingTime(Runner) ?? 0f; } }
    public Vector2 FirePosition { get { return _firePosition.position; } }
    public int CurrentClipSize { get { return currentClipSize; } }
    public int ExtraAmmo { get { return extraAmmo; } }
    public int RemainingAmmo { get { return currentClipSize + extraAmmo; } }

    public float RandomFireAngleDegree
    {
        get
        {
            return owner.WeaponRotationAngleDegrees + Random.Range(-1 * bulletSpread / 2, bulletSpread / 2);
        }
    }

    public float Round(float value, int numDecimalPoints)
    {
        float multiplier = Mathf.Pow(10, numDecimalPoints);
        return ((int) (value * multiplier)) / multiplier;
    }

    public override string ClipSizeString { get { return $"{currentClipSize}/{clipSize}"; } }
    public override string AmmoString { get { return $"{extraAmmo}"; } }
    public string remainingReloadTimeString { get { int numDecimalPoints = 1; return $"{Round(RemainingReloadTime, numDecimalPoints)}s"; } }

    public override void WeaponDropped()
    {
        owner = null;
        positionNetworked = this.transform.position;
        GameObjectListManager.Instance.Anchor(GameObjectListManager.Instance.AllWeapons, this.gameObject);

        if (RemainingAmmo == 0)
        {
            Runner.Despawn(this.Object);
        }
    }

    public override string StringForm
    {
        get
        {
            int numDecimalPoints = 2;
            string stats = "";
            stats += "Bullet Casing Count: " + bulletCasingCount;
            stats += "\nFire Cool Down: " + Round(fireCoolDown.RemainingTime(Runner) ?? 0f, numDecimalPoints) + "s";
            stats += "\nRecoil: " + Round(recoilTimer.RemainingTime(Runner) ?? 0f, numDecimalPoints) + "s";
            stats += "\nReloading: " + reloading;
            stats += $"\nReloadTimer: {remainingReloadTimeString}";

            stats += $"\nClipSize: {ClipSizeString}    Ammo: {AmmoString}";

            //Proxy Lag Compensation Variables 
            //stats += $"\n # Begin Reload Local: {numBeginReloadTimesLocal}    Networked: {numBeginReloadTimesNetworked}";
            //stats += $"\n # Fire Local: {numFireTimesLocal}    Networked: {numFireTimesNetworked}";
            //stats += $"\n # Bullet Casings Local: {numSpawnedBulletCasingLocal}";
            return stats; 
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        base.Spawned();
        numBeginReloadTimesLocal = numBeginReloadTimesNetworked;
        numFireTimesLocal = numFireTimesNetworked;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Update Network Functions
    private void CreateBulletCasing()
    {
        LocalSpawnerManager.CreateBulletCasingInstance(bulletCasingType, bulletCasingSpawnPosition.position);
        numSpawnedBulletCasingLocal++;
    }

    private void EjectBulletCasings()
    {
        if (Runner.IsResimulation) return;
        if (spawnCasingWhileFiring) return;

        while (bulletCasingCount > 0)
        {
            CreateBulletCasing();
            bulletCasingCount--;
        }
    }

    private void BeginReloadLocalEffects()
    {
        if (Runner.IsResimulation) return;

        numBeginReloadTimesLocal++;
        EjectBulletCasings();
    }

    private void BeginReloadServerLogic()
    {
        if (!Runner.IsServer) return;
        
        numBeginReloadTimesNetworked++;
    }

    private void BeginReload()
    {
        if (extraAmmo <= 0) return; //Can't reload if there are no more ammo
        if (reloading) return; //Already reloading
        if (currentClipSize >= clipSize) return; //Can't reload if the clip is full

        reloading = true;
        reloadTimer = TickTimer.CreateFromSeconds(Runner, reloadTime);

        BeginReloadLocalEffects();
        BeginReloadServerLogic();
        if (!Runner.IsServer) numBeginReloadTimesNetworked++; //Client prediction
    }

    private void CancelReload()
    {
        reloading = false;
        reloadTimer = TickTimer.None;
    }

    private void Reload()
    {
        if (!reloading) return; //Not reloading
        if (!reloadTimer.ExpiredOrNotRunning(Runner)) return; //Still reloading

        reloading = false;

        //Reload Logic
        if (singleBulletReload) // Single Bullet reload
        {
            //Add one bullet from extraAmmo to currentClipSize
            currentClipSize++;
            extraAmmo--;

            //Reload again if currentClipSize isn't full
            if (currentClipSize < clipSize)
                BeginReload();
        }
        else //Entire clip reload
        {
            //Empty currentClip into extraAmmo
            extraAmmo += currentClipSize;

            //Fill up currentClip with ammo from extraAmmo
            if (extraAmmo < clipSize) currentClipSize = extraAmmo;
            else currentClipSize = clipSize;
            extraAmmo -= currentClipSize;
        }
    }

    protected virtual void Recoil()
    {
        recoilTimer = TickTimer.CreateFromSeconds(Runner, recoilTime);
        float angle = owner.WeaponRotationAngleDegrees;
        Vector3 recoilDirection = -1 * new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        recoilOffset = recoilDirection * recoilDistance;
        recoilOffset.z = 0;
    }

    private void FireLocalEffects()
    {
        if (Runner.IsResimulation) return;

        Recoil();

        if (!spawnCasingWhileFiring) bulletCasingCount++;
        else CreateBulletCasing();

        LocalSpawnerManager.CreateVisualEffectInstance(fireMuzzleFx, FirePosition, 1.0f, owner.WeaponRotationAngleDegrees, spriteRenderer.flipY, parent: infoPositions);

        numFireTimesLocal++;
    }

    private void FireServerLogic()
    {
        //Server creates bullets
        if (!Runner.IsServer) return;

        for (int i = 0; i < bulletsPerShot || i < 1; i++)
        {
            NetworkObject bulletNetworkObject = NetworkedSpawnerManager.Instance.SpawnBullet();
            Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();
            bullet.SetUp(owner, FirePosition, bulletScale, RandomFireAngleDegree, bulletSpeed, bulletMaxRange, bulletDamage, bulletHeadShotMultiplier, bulletPierceAmount, bulletKnockBackForce);
        }

        numFireTimesNetworked++;
    }

    private void Fire()
    {
        if (!owner.IsAttacking) return; //Return if Owner is not attack
        if (!fireCoolDown.ExpiredOrNotRunning(Runner)) return; //Can't fire if firing is in cool down
        if (currentClipSize <= 0) //If clip is empty, reload instead.
        {
            BeginReload();
            return;
        }
        if (singleBulletReload) reloading = false; //Firing stops reload for Shotgun single bullet reload
        if (reloading) return; //Can't fire while reloading
        fireCoolDown = TickTimer.CreateFromSeconds(Runner, fireSpeed);
        currentClipSize--;

        //Local Visual Effects
        FireLocalEffects();

        //Server Logic
        FireServerLogic();
        if (!Runner.IsServer) numFireTimesNetworked++; //Client prediction

        if (currentClipSize <= 0) BeginReload(); //Automatically begin reload if the current clip is empty after shooting
    }

    private void ClientRunProxyLogic()
    {
        if (Runner.IsServer) return; //Server does not need to run Lag Compensation Proxy Logic
        if (Runner.IsResimulation) return; //Not in resimulation
        if (owner.HasInputAuthority) return; //Not for Objects owned by local player.

        if (numFireTimesLocal < numFireTimesNetworked) FireLocalEffects();
        else if (numFireTimesLocal > numFireTimesNetworked) numFireTimesLocal = numFireTimesNetworked;

        if (numBeginReloadTimesLocal < numBeginReloadTimesNetworked) BeginReloadLocalEffects();
        else if (numBeginReloadTimesLocal > numBeginReloadTimesNetworked) numBeginReloadTimesLocal = numBeginReloadTimesNetworked;
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentWeaponStatus != WeaponStatus.Using) return;

        if (Runner.IsServer || owner.HasInputAuthority)
        {
            Fire();
            Reload();
            if (owner.IsReloading) BeginReload();
        }
        else ClientRunProxyLogic(); //Proxies
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Item State
    protected override void LateUpdateItemState()
    {
        CancelReload();
        base.LateUpdateItemState();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Inventory State
    protected override void MoveToInventoryPosition()
    {
        Character.ChosenWeaponSlot weaponSlot = owner.WhichGunSlot(this);
        if (weaponSlot == Character.ChosenWeaponSlot.Null) return; //Not Owned

        Character.ChosenWeaponSlot primaryLeftInventoryPosition = Character.ChosenWeaponSlot.PrimaryGun1;
        Character.ChosenWeaponSlot primaryRightInventoryPosition = Character.ChosenWeaponSlot.PrimaryGun2;
        Character.ChosenWeaponSlot sideArmLeftInventoryPosition = Character.ChosenWeaponSlot.SideArm1;
        Character.ChosenWeaponSlot sideArmRightInventoryPosition = Character.ChosenWeaponSlot.SideArm2;

        if (owner.IsFacingLeft)
        {
            primaryLeftInventoryPosition = Character.ChosenWeaponSlot.PrimaryGun2;
            primaryRightInventoryPosition = Character.ChosenWeaponSlot.PrimaryGun1;
        }

        spriteRenderer.sortingLayerName = "Game Play";
        spriteRenderer.flipY = false;

        if (weaponSlot == primaryLeftInventoryPosition)
        {
            spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder - 3;
            LerpToInventoryPosition(owner.PrimaryWeaponInventoryPosition);
            transform.localEulerAngles = new Vector3(0, 0, 315);
        }
        else if (weaponSlot == primaryRightInventoryPosition)
        {
            spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder - 2;
            LerpToInventoryPosition(owner.PrimaryWeaponInventoryPosition);
            transform.localEulerAngles = new Vector3(180, 0, 135);
        }
        else if (weaponSlot == sideArmLeftInventoryPosition)
        {
            if (owner.IsFacingLeft) spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder - 1;
            if (owner.IsFacingRight) spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder + OutfitManager.NUM_OF_OUTFIT_TYPES;
            LerpToInventoryPosition(owner.SideArm1InventoryPosition);
            if (owner.IsFacingLeft) LerpToInventoryAngle(new Vector3(0, 0, 270));
            if (owner.IsFacingRight) LerpToInventoryAngle(new Vector3(0, 180, 270));
        }
        else if (weaponSlot == sideArmRightInventoryPosition)
        {
            if (owner.IsFacingLeft) spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder + OutfitManager.NUM_OF_OUTFIT_TYPES;
            if (owner.IsFacingRight) spriteRenderer.sortingOrder = owner._outfitAC.StartingSortingOrder - 1;
            LerpToInventoryPosition(owner.SideArm2InventoryPosition);
            if (owner.IsFacingLeft) LerpToInventoryAngle(new Vector3(0, 0, 270));
            if (owner.IsFacingRight) LerpToInventoryAngle(new Vector3(0, 180, 270));
        }
    }

    protected override void LateUpdateInventoryState()
    {
        CancelReload();
        base.LateUpdateInventoryState();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Using State
    private void RecoilMoveToOwner()
    {
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, (recoilTime - recoilTimer.RemainingTime(Runner) ?? 0) / recoilTime);

        MoveToOwner();
        transform.position += recoilOffset;
    }

    protected override void UpdatePosition()
    {
        if (recoilTimer.ExpiredOrNotRunning(Runner)) MoveToOwner();
        else RecoilMoveToOwner();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
