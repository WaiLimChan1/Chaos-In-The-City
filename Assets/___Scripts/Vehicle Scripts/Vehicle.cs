using CITC.GameManager;
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static CITC.GameManager.SingleAnimationClipManager;

public class Vehicle : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static
    public static bool CanUse(Vehicle vehicle) { return vehicle != null && vehicle != default; }

    public const int NUM_SORTING_ORDER = 1;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    private Rigidbody2D _rigidbody2D;
    private NetworkRigidbody2D _networkRigidBody2D;
    private SpriteRenderer _spriteRenderer;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Seats")]
    [SerializeField] List<VehicleSeat> Seats;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Car Static Variables")]
    private float _driftFactor = 0.95f;
    private float _turnFactor = 5f;
    private float _noAccelerationDrag = 0.5f;
    private float _dragLerpRatio = 3;

    private float _minSpeedToDealDamage = 20;
    private float _collisionDamageMultiplier = 1.5f;
    private float _collisionKnockBackForceMultiplier = 0.1f;

    [Header("Car Explosion Static Variables")]
    private float _explosionRadius = 10;
    private int _explosionSortingOrder = -1;
    private float _explosionDamage = 500;
    private float _explosionKnockBackForce = 5f;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Car Settings")]
    [SerializeField] private float _accelerationFactor = 150f;
    [SerializeField] private float _maxSpeed = 200.0f;
    [SerializeField] private float _brakeDrag = 5;
    [SerializeField] private float _turnMinSpeedMultiplier = 100;
    [SerializeField] private float _minLateralVelocityForSkidMarks = 10f;
    [SerializeField] private float _maxHealth = 1000;
    [SerializeField] private float _maxGas = 1000;
    [SerializeField] private float _gasUse = 50;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Car Dynamic Variables")]
    private int _startingSortingOrder;
    private float _accelerationInput = 0;
    private float _steeringInput = 0;
    [SerializeField] [Networked] private float rotationAngle { get; set; }
    [SerializeField] [Networked] private float velocityVsUp { get; set; }
    [SerializeField] [Networked] private float _health { get; set; }
    [SerializeField] [Networked] private float _gas { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public GameObject InterpolationTarget { get { return _networkRigidBody2D.InterpolationTarget.gameObject; } }
    public float HealthRatio { get { return _health / _maxHealth; } }
    public bool IsDead { get { return _health <= 0; } }
    public float ForwardVelocity { get { return velocityVsUp; } }
    public float ForwardSpeed { get { return Mathf.Abs(ForwardVelocity); } }
    public float CollisionDamage { get { return (ForwardSpeed - _minSpeedToDealDamage / 2) * _collisionDamageMultiplier; } }
    public Vector2 KnockBackForce { get { return (ForwardSpeed - _minSpeedToDealDamage / 2) * _rigidbody2D.velocity.normalized * _collisionKnockBackForceMultiplier; } }
    public int StartingSortingOrder
    {
        get { return _startingSortingOrder; }
        set
        {
            if (_startingSortingOrder == value) return;
            _startingSortingOrder = value;
            _spriteRenderer.sortingOrder = _startingSortingOrder;

            //Update Passenger Sorting Order
            foreach (VehicleSeat seat in Seats)
                if (Character.CanUse(seat.character))
                    seat.character._outfitAC.StartingSortingOrder = _startingSortingOrder - Character.NUM_SORTING_ORDER;
        }
    }

    public Character Driver
    {
        get
        {
            foreach (VehicleSeat seat in Seats)
                if (seat.isDriver)
                    return seat.character;
            return null;
        }
        set
        {
            foreach (VehicleSeat seat in Seats)
                if (seat.isDriver)
                {
                    seat.character = value;
                    return;
                }
        }
    }

    public bool HasEmptySeat
    {
        get
        {
            foreach (VehicleSeat seat in Seats)
                if (seat.character == null)
                    return true;
            return false;
        }
    }

    public bool IsAPassenger(Character character)
    {
        if (!Character.CanUse(character)) return false;

        foreach (VehicleSeat seat in Seats)
            if (seat.character == character)
                return true;

        return false;
    }

    public Vector3 GetPassengerSeatPosition(Character character)
    {
        foreach (VehicleSeat seat in Seats)
            if (seat.character == character)
                return seat.transform.position;
        return Vector3.zero;
    }

    public bool EnterCar(Character character)
    {
        if (!HasEmptySeat) return false; //No more seats
        if (IsDead) return false; //Car is dead

        foreach (VehicleSeat seat in Seats)
            if (seat.character == null)
            {
                seat.character = character;
                return true;
            }

        return false;
    }

    public void ExitCar(Character character)
    {
        foreach (VehicleSeat seat in Seats)
            if (seat.character == character)
            {
                seat.character = null;
                return;
            }
    }

    public void ExitAllPassengers()
    {
        foreach (VehicleSeat seat in Seats)
            if (seat.character != null)
            {
                seat.character.CurrentVehicle = null;
                seat.character = null;
            }
    }

    public float GetLateralVelocity()
    {
        return Vector2.Dot(transform.right, _rigidbody2D.velocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        //Check if we are moving forward and if the player is hitting the brakes. In that case the tires should screech.
        if (_accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        //If we have a lot of side movement then the tires should be screeching
        if (Mathf.Abs(GetLateralVelocity()) > _minLateralVelocityForSkidMarks)
        {
            return true;
        }

        return false;
    }

    public string StringForm
    {
        get
        {
            string text = "";
            text += $"Health: {(int)_health}/{_maxHealth}";
            text += $"\nGas: {(int)_gas}/{_maxGas}";
            text += $"\nVelocity: {(int)velocityVsUp}/{_maxSpeed}";
            text += $"\nRigidbody Speed: {(int)_rigidbody2D.velocity.magnitude}";
            //text += $"\nDrag: {_rigidbody2D.drag}";
            //text += $"\nCollisionDamage: {CollisionDamage}";
            //text += $"\nKnockBackForce: {KnockBackForce}";

            //foreach (VehicleSeat seat in Seats)
            //{
            //    text += "\n" + seat.gameObject.name + ": ";
            //    if (Character.CanUse(seat.character)) text += seat.character.Name;
            //}
            return text;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllVehicles, this.gameObject);

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _networkRigidBody2D = GetComponent<NetworkRigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public override void Spawned()
    {
        _health = _maxHealth;
        _gas = _maxGas;
    }

    public void SetUp(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Fixed Network Update
    public void SetInputVector()
    {
        if (!Character.CanUse(Driver))
        {
            _steeringInput = 0;
            _accelerationInput = 0;
            return;
        }

        if (Runner.TryGetInputForPlayer<CharacterData>(Driver.Object.InputAuthority, out var characterData))
        {
            _steeringInput = characterData.MovementDirection.x;
            _accelerationInput = characterData.MovementDirection.y;
            if (_accelerationInput < 0) _steeringInput *= -1;
        }
    }

    void ApplyEngineForce()
    {
        velocityVsUp = Vector2.Dot(transform.up, _rigidbody2D.velocity);

        //Gas Calculations
        if (Mathf.Abs(_accelerationInput) > 0) _gas -= _gasUse * Runner.DeltaTime;
        if (_gas <= 0)
        {
            _gas = 0;
            _accelerationInput = 0;
        }

        if (velocityVsUp > _maxSpeed && _accelerationInput > 0) return; //Reached Max Spped
        if (_rigidbody2D.velocity.sqrMagnitude > _maxSpeed * _maxSpeed && _accelerationInput > 0) return; //Reached Max Speed

        //Add Drag if the car isn't accelerating
        if (_accelerationInput == 0)
            _rigidbody2D.drag = Mathf.Lerp(_rigidbody2D.drag, _noAccelerationDrag, Runner.DeltaTime * _dragLerpRatio);

        //Add Drag for breaking, if the car is accelerating in the opposite direction movement.
        else if ((_accelerationInput > 0 && velocityVsUp < 0) || (_accelerationInput < 0 && velocityVsUp > 0))
            _rigidbody2D.drag = Mathf.Lerp(_rigidbody2D.drag, _brakeDrag, Runner.DeltaTime * _dragLerpRatio);

        //No Drag if the car is moving and accelerating in the same direction
        else _rigidbody2D.drag = 0;

        Vector2 engineForceVector = transform.up * _accelerationInput * _accelerationFactor;

        _rigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        float minSpeedBeforeAllowTuringFactor = (_rigidbody2D.velocity.magnitude / _turnMinSpeedMultiplier);
        minSpeedBeforeAllowTuringFactor = Mathf.Clamp01(minSpeedBeforeAllowTuringFactor);

        float changeInRotationAngle = _steeringInput * _turnFactor * minSpeedBeforeAllowTuringFactor;
        if (_accelerationInput == 0 && velocityVsUp < 0) changeInRotationAngle *= -1; //If moving backwards, turn in opposite direction
        rotationAngle -= changeInRotationAngle;

        _rigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(_rigidbody2D.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(_rigidbody2D.velocity, transform.right);

        _rigidbody2D.velocity = forwardVelocity + rightVelocity * _driftFactor;
    }

    public void UpdatePassengerPosition()
    {
        foreach (VehicleSeat seat in Seats)
        {
            if (Character.CanUse(seat.character))
            {
                seat.character.transform.position = seat.transform.position;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        SetInputVector();

        ApplyEngineForce();
        ApplySteering();
        KillOrthogonalVelocity();

        UpdatePassengerPosition();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Collisions
    public void Explode()
    {
        LocalSpawnerManager.CreateVisualEffectInstance(SingleAnimationType.ExplosionFx, transform.position, _explosionRadius, sortingOrder: _explosionSortingOrder);

        //Hitting Characters
        Collider2D[] characterColliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, LayerMask.GetMask("Character Full Hitbox"));
        foreach (Collider2D collider in characterColliders)
        {
            Character character = collider.GetComponentInParent<Character>();
            if (!Character.CanUse(character)) continue;

            CharacterHitbox targetHitbox = collider.GetComponent<CharacterFullHitbox>();
            if (targetHitbox == null) continue;

            float distance = Vector2.Distance(targetHitbox.transform.position, transform.position);
            float keepRatio = Mathf.Clamp01((_explosionRadius - distance) / _explosionRadius);
            Vector2 knockBackDirection = (targetHitbox.transform.position - transform.position).normalized;
            targetHitbox.TakeHit(SingleAnimationType.HeadShotBlood, targetHitbox.transform.position, _explosionDamage * keepRatio, knockBackDirection * _explosionKnockBackForce);
        }

        //Hitting Vehicles
        Collider2D[] vehicleColliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, LayerMask.GetMask("Vehicle"));
        foreach (Collider2D collider in vehicleColliders)
        {
            Vehicle vehicle = collider.GetComponent<Vehicle>();
            if (!Vehicle.CanUse(vehicle)) continue;

            if (vehicle == this) continue; //Hitting itself

            Vector2 knockBackDirection = (vehicle.transform.position - transform.position).normalized;
            vehicle.TakeHit(_explosionDamage, knockBackDirection * _explosionKnockBackForce, SingleAnimationType.GetPunchedVFX, vehicle.transform.position, vehicle.InterpolationTarget.transform);
        }
    }

    public void HandleDeath()
    {
        Explode();
        ExitAllPassengers();
    }

    public void KnockBack(Vector2 force)
    {
        if (!CanUse(this)) return;
        _rigidbody2D.AddForce(force * GlobalConstants.FORCE_MULTIPLIER);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            HandleDeath();
        }
    }

    //Take Damage, Knock Back, and create Visual Effect
    public void TakeHit(float damage, Vector2 knockBack = default(Vector2), SingleAnimationType singleAnimationType = SingleAnimationType.None, Vector3 localEffectsPosition = default(Vector3), Transform visualEffectParent = null)
    {
        TakeDamage(damage);
        KnockBack(knockBack);
        LocalSpawnerManager.CreateVisualEffectInstance(singleAnimationType, localEffectsPosition, parent: this.transform);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (Runner.IsResimulation) return; //Is resimulation
        if (IsDead) return; //Already Dead

        //Have to use Forward Speed because _rigidbody2d.velocity.magnitude is unreliable for collisions.
        //The collision is triggered after the velocity has already been affected by the physics system.
        if (ForwardSpeed < _minSpeedToDealDamage) return; //Not fast enough

        //Collided With Character
        Character character = collision.gameObject.GetComponent<Character>();
        if (Character.CanUse(character))
        {
            if (character.IsDead) return; //Character is already dead

            CharacterHitbox characterHitbox = collision.gameObject.GetComponentInChildren<CharacterFullHitbox>();
            if (characterHitbox == null) return; //No Character Hit Box

            this.TakeHit(CollisionDamage, -1 * KnockBackForce, SingleAnimationType.GetPunchedVFX, collision.contacts[0].point, visualEffectParent: InterpolationTarget.transform);
            characterHitbox.TakeHit(SingleAnimationType.GetPunchedVFX, characterHitbox.transform.position, CollisionDamage, KnockBackForce, characterHitbox.transform);
            return;
        }

        //Collided with Vehicle
        Vehicle otherVehicle = collision.gameObject.GetComponent<Vehicle>();
        if (Vehicle.CanUse(otherVehicle))
        {
            this.TakeHit(CollisionDamage, -1 * KnockBackForce, SingleAnimationType.GetPunchedVFX, collision.contacts[0].point, visualEffectParent: InterpolationTarget.transform);
            otherVehicle.TakeHit(CollisionDamage, -1 * KnockBackForce, SingleAnimationType.GetPunchedVFX, collision.contacts[0].point, visualEffectParent: InterpolationTarget.transform);
            return;
        }

        //Collided With Building
        Building building = collision.gameObject.GetComponentInParent<Building>();
        if (building != null)
        {
            this.TakeHit(CollisionDamage, -1 * KnockBackForce, SingleAnimationType.GetPunchedVFX, collision.contacts[0].point, visualEffectParent: InterpolationTarget.transform);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Runner.IsResimulation) return; //Is resimulation

        //Collided With Bullet
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null && !bullet.Finished)
        {
            if (IsAPassenger(bullet.Owner)) return; //Bullet owner is a passenger

            this.TakeHit(bullet.Damage, bullet.KnockBackForce, SingleAnimationType.GetPunchedVFX, bullet.transform.position, visualEffectParent: InterpolationTarget.transform);

            if (!IsDead) bullet.HitSomethingSemiPenetrable(); //Hitting Vehicle
            else bullet.HitSomethingImpenetrable(); //Hitting Vehicle Corpse

        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Local Visual Effect
    private void LateUpdate()
    {
        if (Object == null || Object == default) return;
        if (IsDead)
        {
            _spriteRenderer.color = Color.black;
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
