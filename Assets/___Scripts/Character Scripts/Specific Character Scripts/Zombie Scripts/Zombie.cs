using CITC.GameManager;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Character
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Zombie Variables")]
    [SerializeField] [Networked] protected Character TargetCharacter { get; set; }

    [Networked] protected Vector3 holdPosition { get; set; }
    [Networked] protected TickTimer holdTime { get; set; }

    [SerializeField] private float _searchRadius;

    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Outfit Variables
    protected override void RandomizeOutfit()
    {
        _outfit.RandomizeOutfit(1);
        _outfit.BackpackIndex = -1;
        _outfit.MaskIndex = -1;

        _outfitNetworked.CopyFrom(_outfit);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    #region Punch
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Punch
    protected override void PunchedVisualEffect()
    {
        LocalSpawnerManager.CreateVisualEffectInstance(PunchVFX, PunchAttackBox.bounds.center, 1, WeaponRotationAngleDegrees - 45, sortingOrder: 1, parent: InterpolationTarget.transform);
    }

    protected override Character PunchedColliderCheck(Collider2D collider)
    {
        return collider.GetComponentInParent<Survivor>();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion




    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        base.Spawned();
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllZombies, this.gameObject);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //AI Functions
    protected void FindTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, _searchRadius, LayerMask.GetMask("Survivor Marker"));
        List<Character> characterList = new List<Character>();
        foreach (Collider2D collider in targets)
        {
            if (!collider.gameObject.CompareTag("Survivor")) continue;

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
    //Input Functions
    protected override void TakeMovementDirectionInput()
    {
        if (Character.CanUse(TargetCharacter))
        {
            movementDirection = TargetCharacter.transform.position - transform.position;
            movementDirection = movementDirection.normalized;
            return;
        }

        if (Runner.IsServer && holdTime.ExpiredOrNotRunning(Runner))
        {
            
            float randomPositionRange = 50;
            Vector3 randomPosition = new Vector3(Random.Range(-randomPositionRange, randomPositionRange), Random.Range(-randomPositionRange, randomPositionRange), 0);
            holdPosition = randomPosition;

            Vector2 holdTimeRange = new Vector2(15, 25);
            holdTime = TickTimer.CreateFromSeconds(Runner, Random.Range(holdTimeRange.x, holdTimeRange.y));
        }

        float minDistance = 1f;

        if ((holdPosition - transform.position).magnitude > minDistance)
        {
            movementDirection = holdPosition - transform.position;
            movementDirection = movementDirection.normalized;
        }
        else movementDirection = Vector3.zero;

        
    }

    protected override void TakeMouseWorldPositionInput()
    {
        if (Character.CanUse(TargetCharacter)) mouseWorldPosition = TargetCharacter.transform.position;
        else mouseWorldPosition = holdPosition;
        mouseWorldPosition.z = 0;
    }

    protected override void TakeChosenWeaponSlotInput()
    {
        chosenWeaponSlot = ChosenWeaponSlot.Melee;
    }

    protected override void TakeAttackInput()
    {
        triggeredAttack = false;
    }

    protected override void TakePunchInput()
    {
        if (Character.CanUse(TargetCharacter))
        {
            if (Vector3.Distance(transform.position, TargetCharacter.transform.position) <= _punchRange)
            {
                triggeredPunch = true;
                return;
            }
        }
        triggeredPunch = false;
    }

    protected override void TakeReloadInput()
    {
        triggeredReload = false;
    }

    protected override void TakePickUpInput()
    {
        triggeredPickUp = false;
    }

    protected override void TakeDropChosenWeaponInput()
    {
        triggeredDropChosenWeapon = false;
    }

    public override void BeforeUpdate()
    {
        FindTarget();
        base.BeforeUpdate();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    protected override void OnDrawGizmos()
    {
        //Search Radius
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, _searchRadius);

        //Punch Range
        //Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(transform.position, _punchRange);
    }
}
