using CITC.GameManager;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class HostileSurvivor : Survivor
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public const float WEAPON_ANGLE_LERP_RATIO = 1f;
    public const float CAN_SHOOT_DEGREE = 10f;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Hostile Survivor Variables")]
    [SerializeField] [Networked] protected Character TargetCharacter { get; set; }
    [SerializeField] [Networked] protected Weapon TargetWeapon { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Hostile Survivor Zone Variables")]
    [SerializeField] protected float SearchAndChaseZone;
    [SerializeField] protected float AttackZone;
    [SerializeField] protected float RunZone;
    [SerializeField] protected float MeleeZone;
    [SerializeField] protected float PunchZone;

    private void OnDrawGizmosZones()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SearchAndChaseZone);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AttackZone);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RunZone);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, MeleeZone);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, PunchZone);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Outfit Variables
    protected override void RandomizeOutfit()
    {
        _outfit.RandomizeOutfit(0);
        _outfit.MaskIndex = 1;
        _outfitNetworked.CopyFrom(_outfit);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Character Weapon Helper Functions
    private float currentAngle;
    public float TargetWeaponRotationAngleDegrees
    {
        get
        {
            Vector3 directionToMouse = (mouseWorldPositionNetworked - NonInterpolatedWeaponHoldPosition).normalized;
            float rotationInRadians = Mathf.Atan2(directionToMouse.y, directionToMouse.x);
            float angleInDegrees = rotationInRadians * Mathf.Rad2Deg;
            return angleInDegrees;
        }
    }

    public override float WeaponRotationAngleDegrees
    {
        get
        {
            return currentAngle;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        base.Spawned();

        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllHostileSurvivors, this.gameObject);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Find Target Character
    protected float TargetCharacterDistance { get { return Vector2.Distance(TargetCharacter.transform.position, transform.position); } }
    
    protected void FindTargetCharacter()
    {
        Collider2D[] characterColliderList = Physics2D.OverlapCircleAll(transform.position, SearchAndChaseZone, LayerMask.GetMask("Character"));
        List<Character> characterList = new List<Character>();
        foreach (Collider2D collider in characterColliderList)
        {
            Character character = collider.GetComponentInParent<Character>();
            if (!Character.CanUse(character)) continue;
            if (character == this) continue; //Skip if it is itself

            characterList.Add(character);
        }

        characterList.Sort((character1, character2) =>
        {
            float dist1 = Vector2.Distance(transform.position, character1.transform.position);
            float dist2 = Vector2.Distance(transform.position, character2.transform.position);
            return dist1.CompareTo(dist2);
        });

        //Found no character to target
        if (characterList.Count <= 0)
        {
            TargetCharacter = null;
            return;
        }

        //Closest Character is the target character
        TargetCharacter = characterList[0];
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Find Target Weapon

    //Sort the weapons by distance to the survivor. Closest weapons first
    protected void SortClosestWeaponsFirst(List<Weapon> weaponList)
    {
        //Sort the weapons by distance to the survivor. Closest weapons first
        weaponList.Sort((weapon1, weapon2) =>
        {
            float dist1 = Vector2.Distance(PickUpPosition, weapon1.transform.position);
            float dist2 = Vector2.Distance(PickUpPosition, weapon2.transform.position);
            return dist1.CompareTo(dist2);
        });
    }

    protected void FindTargetWeapon()
    {
        Collider2D[] itemColliderList = Physics2D.OverlapCircleAll(transform.position, SearchAndChaseZone, LayerMask.GetMask("Item"));
        List<Weapon> sortedWeaponList = new List<Weapon>();
        List<Weapon> primaryGunList = new List<Weapon>();
        List<Weapon> sideArmGunList = new List<Weapon>();
        List<Weapon> meleeList = new List<Weapon>();
        List<Weapon> throwableList = new List<Weapon>();

        //Find all of the weapons that the survivor has room to pick up
        foreach (Collider2D collider in itemColliderList)
        {
            //Not a weapon, continue
            Weapon weapon = collider.GetComponent<Weapon>();
            if (!Weapon.CanUse(weapon)) continue;

            //It is a gun
            Gun gun = collider.GetComponent<Gun>();
            if (Weapon.CanUse(gun) && gun.RemainingAmmo > 0)
            {
                if (gun.GetWeaponType == Weapon.WeaponType.Primary) //It is a Primary Gun
                {
                    if (Weapon.CanUse(primaryGun1) && Weapon.CanUse(primaryGun2)) continue; //Primary Gun slots are full.
                    primaryGunList.Add(weapon);
                    continue;
                }
                if (gun.GetWeaponType == Weapon.WeaponType.SideArm) //It is a secondary Gun
                {
                    if (Weapon.CanUse(sideArm1) && Weapon.CanUse(sideArm2)) continue; //Side Arm Gun slots are full.
                    sideArmGunList.Add(weapon);
                    continue;
                }
            }

            //It is a melee
            if (!Weapon.CanUse(this.melee))
            {
                Melee melee = collider.GetComponent<Melee>();
                if (Weapon.CanUse(melee))
                {
                    meleeList.Add(melee);
                    continue;
                }
            }

            //It is a throwable
            if (!Weapon.CanUse(this.throwable))
            {
                Throwable throwable = collider.GetComponent<Throwable>();
                if (Weapon.CanUse(throwable))
                {
                    throwableList.Add(throwable);
                    continue;
                }
            }
        }

        //Sort the weapons by distance to the survivor. Closest weapons first
        SortClosestWeaponsFirst(primaryGunList);
        SortClosestWeaponsFirst(sideArmGunList);
        SortClosestWeaponsFirst(meleeList);
        SortClosestWeaponsFirst(throwableList);

        //Add the weapons together into one list.
        sortedWeaponList.AddRange(primaryGunList);
        sortedWeaponList.AddRange(sideArmGunList);
        sortedWeaponList.AddRange(meleeList);
        sortedWeaponList.AddRange(throwableList);

        //Found no weapons to target
        if (sortedWeaponList.Count <= 0)
        {
            TargetWeapon = null;
            return;
        }

        //Closest Weapon is the target weapon
        TargetWeapon = sortedWeaponList[0];
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //AI Functions
    protected void FindTarget()
    {
        FindTargetCharacter();
        FindTargetWeapon();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Input Functions
    protected override void TakeMovementDirectionInput()
    {
        //Survivor will always go to weapon if survivor does not have a weapon
        if (Weapon.CanUse(TargetWeapon) && !Weapon.CanUse(primaryGun1) && !Weapon.CanUse(primaryGun2) && !Weapon.CanUse(sideArm1) && !Weapon.CanUse(sideArm2))
        {
            movementDirection = TargetWeapon.transform.position - transform.position;
            movementDirection = movementDirection.normalized;
        }
        //Chase target character if they are too far away and survivor has a gun
        else if (Character.CanUse(TargetCharacter) && TargetCharacterDistance > AttackZone && TargetCharacterDistance < SearchAndChaseZone)
        {
            movementDirection = (TargetCharacter.transform.position - transform.position);
            movementDirection = movementDirection.normalized;
        }
        //Chase target character if survivor is holding a melee
        else if (Character.CanUse(TargetCharacter) && chosenWeaponSlot == ChosenWeaponSlot.Melee && Weapon.CanUse(melee))
        {
            movementDirection = (TargetCharacter.transform.position - transform.position);
            movementDirection = movementDirection.normalized;
        }
        //Run away from target character if target character is too close and the survivor is not using a melee
        else if (Character.CanUse(TargetCharacter) && TargetCharacterDistance < RunZone)
        {
            movementDirection = -1 * (TargetCharacter.transform.position - transform.position);
            movementDirection = movementDirection.normalized;
        }
        //Go to target weapon
        else if (Weapon.CanUse(TargetWeapon))
        {
            movementDirection = TargetWeapon.transform.position - transform.position;
            movementDirection = movementDirection.normalized;
        }
        //Stand in the same place
        else
        {
            movementDirection = Vector3.zero;

            //movementDirection = Vector3.zero - transform.position;
            //movementDirection = movementDirection.normalized;
        }
    }

    protected override void TakeMouseWorldPositionInput()
    {
        if (Character.CanUse(TargetCharacter)) //Point at target character
        {
            mouseWorldPosition = TargetCharacter.transform.position;
        }
        else if (Weapon.CanUse(TargetWeapon)) //Point at target weapon
        {
            mouseWorldPosition = TargetWeapon.transform.position;
        }
        mouseWorldPosition.z = 0;

        //Calculate Angle
        currentAngle = Mathf.Lerp(currentAngle, TargetWeaponRotationAngleDegrees, WEAPON_ANGLE_LERP_RATIO * Runner.DeltaTime);
    }

    protected override void TakeChosenWeaponSlotInput()
    {
        if (Weapon.CanUse(primaryGun1)) chosenWeaponSlot = ChosenWeaponSlot.PrimaryGun1;
        else if (Weapon.CanUse(primaryGun2)) chosenWeaponSlot = ChosenWeaponSlot.PrimaryGun2;
        else if (Weapon.CanUse(sideArm1)) chosenWeaponSlot = ChosenWeaponSlot.SideArm1;
        else if (Weapon.CanUse(sideArm2)) chosenWeaponSlot = ChosenWeaponSlot.SideArm2;
        else if (Weapon.CanUse(throwable)) chosenWeaponSlot = ChosenWeaponSlot.Throwable;
        else chosenWeaponSlot = ChosenWeaponSlot.Melee;
    }

    protected override void TakeAttackInput()
    {
        //Target Character is within attack range and the survivor has chosen gun or throwable
        if (Character.CanUse(TargetCharacter) && TargetCharacterDistance < AttackZone && Mathf.Abs(WeaponRotationAngleDegrees - TargetWeaponRotationAngleDegrees) < CAN_SHOOT_DEGREE)
        {
            triggeredAttack = true;
        }
        else triggeredAttack = false;
    }

    protected override void TakePunchInput()
    {
        if (Character.CanUse(TargetCharacter) && TargetCharacterDistance < PunchZone)
        {
            triggeredPunch = true;
        }
        else triggeredPunch = false;
    }

    protected override void TakeReloadInput()
    {
        triggeredReload = false;
    }

    protected override void TakePickUpInput()
    {
        //Pick Up Target Weapon
        if (Weapon.CanUse(TargetWeapon))
        {
            if (Vector2.Distance(PickUpPosition, TargetWeapon.transform.position) <= PickUpRadius)
            {
                triggeredPickUp = true;
                alreadyPickedUp = false;
            }
        }
        else triggeredPickUp = false;
    }

    protected override void TakeDropChosenWeaponInput()
    {
        //Chosen Weapon is a gun and it ran out of ammo
        if (ChosenWeapon != null && Weapon.CanUse(ChosenWeapon.GetComponent<Gun>()) && ChosenWeapon.GetComponent<Gun>().RemainingAmmo == 0)
        {
            triggeredDropChosenWeapon = true;
            alreadyDroppedChosenWeapon = false;
        }
        else triggeredDropChosenWeapon = false;
    }

    public override void BeforeUpdate()
    {
        FindTarget();
        base.BeforeUpdate();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Pick Up Items
    protected override void PickUpItem()
    {
        if (!Runner.IsServer) return;
        if (!IsPickingUp) return;
        if (!Weapon.CanUse(TargetWeapon)) return;

        Collider2D[] itemColliders = Physics2D.OverlapCircleAll(PickUpPosition, PickUpRadius, LayerMask.GetMask("Item"));
        if (itemColliders.Length == 0) return;

        for (int i = 0; i < itemColliders.Length; i++)
        {
            GameObject currentGameObject = itemColliders[i].gameObject;
            if (currentGameObject == TargetWeapon.gameObject)
            {
                if (PickUpGun(currentGameObject)) return;
                else if (PickUpMelee(currentGameObject)) return;
                else if (PickUpThrowable(currentGameObject)) return;

                return;
            }

        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Gizmos Functions
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        OnDrawGizmosZones();
    }

}
