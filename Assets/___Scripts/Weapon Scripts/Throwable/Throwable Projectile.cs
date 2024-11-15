using CITC.GameManager;
using static CITC.GameManager.SingleAnimationClipManager;
using Fusion;
using UnityEngine;

public class ThrowableProjectile : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Throwable Projectile Components")]
    private NetworkRigidbody2D _networkRigidBody2D;
    private Rigidbody2D _rigidBody2D;
    [SerializeField] private GameObject _graphics;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Throwable Projectile Information")]
    [SerializeField] private Character _owner;
    [SerializeField] [Networked] private float _scale { get; set; }
    [SerializeField] [Networked] private float _damage { get; set; }
    [SerializeField] [Networked] private float _knockBackForce { get; set; }
    [SerializeField] [Networked] private float _explosionRadius { get; set; }
    [SerializeField] [Networked] private float _detonationTime { get; set; }
    [SerializeField][Networked] private float _timeWhenThrown { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Local Effect Static Variables")]
    private static int _explosionSortingOrder = -1;
    private static Vector2 _localGraphicsScaleRange = new Vector2(1.5f, 1f);
    private static Vector2 _angularVelocityRange = new Vector2(1000f, 0f);

    [Header("Local Effect Dynamic Variables")]
    private float _angularVelocity;
    private float _currentAngle;
    private bool _exploded; //Makes sure explosion only happens once for the client
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    private float RemainingTime  {  get { return Mathf.Max((_timeWhenThrown + _detonationTime) - Runner.SimulationTime, 0); } }


    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllThrowableProjectiles, this.gameObject);

        _networkRigidBody2D = GetComponent<NetworkRigidbody2D>();
        _rigidBody2D = GetComponent<Rigidbody2D>();

        _exploded = false;
    }

    public void SetUp(Character owner, Vector3 position, float scale, float rotationAngleDegrees, float throwForce, float explosionDamage, float explosionKnockBackForce, float explosionRadius, float detonationTime)
    {
        _owner = owner;

        transform.position = position;
        _scale = scale;

        _rigidBody2D.AddForce(new Vector3(Mathf.Cos(rotationAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(rotationAngleDegrees * Mathf.Deg2Rad), 0).normalized * throwForce * GlobalConstants.FORCE_MULTIPLIER);
        
        _damage = explosionDamage;
        _knockBackForce = explosionKnockBackForce;
        _explosionRadius = explosionRadius;
        _detonationTime = detonationTime;
        _timeWhenThrown = Runner.SimulationTime;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Update
    public void Explode()
    {
        if (_exploded) return;
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
            targetHitbox.TakeHit(SingleAnimationType.HeadShotBlood, targetHitbox.transform.position, _damage * keepRatio, knockBackDirection * _knockBackForce);
        }

        //Hitting Vehicles
        Collider2D[] vehicleColliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, LayerMask.GetMask("Vehicle"));
        foreach (Collider2D collider in vehicleColliders)
        {
            Vehicle vehicle = collider.GetComponent<Vehicle>();
            if (!Vehicle.CanUse(vehicle)) continue;

            Vector2 knockBackDirection = (vehicle.transform.position - transform.position).normalized;
            vehicle.TakeHit(_damage, knockBackDirection * _knockBackForce, SingleAnimationType.GetPunchedVFX, vehicle.transform.position, vehicle.InterpolationTarget.transform);
        }

        _exploded = true;

        HandleDestruction();
    }

    public override void FixedUpdateNetwork()
    {
        transform.localScale = new Vector3(_scale, _scale, 1);
        if (RemainingTime <= 0) Explode();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Late Update

    //Grenade graphics scale changes to create the illusion of height
    private void DetermineLocalGraphicsScale()
    {
        float localGraphicsScale = Mathf.Lerp(_localGraphicsScaleRange.x, _localGraphicsScaleRange.y, 1 - (RemainingTime / _detonationTime));
        _graphics.transform.localScale = new Vector3(localGraphicsScale, localGraphicsScale, 1);
    }

    //Spin The Grenade
    private void DetermineLocalGraphicsAngularRotation()
    {
        _angularVelocity = Mathf.Lerp(_angularVelocityRange.x, _angularVelocityRange.y, 1 - (RemainingTime / _detonationTime));
        _currentAngle += _angularVelocity * Time.deltaTime;
        _graphics.transform.rotation = Quaternion.Euler(0.0f, 0.0f, _currentAngle);
    }

    public void LateUpdate()
    {
        DetermineLocalGraphicsScale();
        DetermineLocalGraphicsAngularRotation();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------


    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void HandleDestruction()
    {
        Runner.Despawn(this.Object);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Gizmos Functions
    protected virtual void OnDrawGizmos()
    {
        //Explosion Radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
