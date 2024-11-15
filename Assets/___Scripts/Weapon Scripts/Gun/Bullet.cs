using CITC.GameManager;
using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [Header("Bullet Components")]
    private NetworkRigidbody2D _networkRigidBody2D;

    [Header("Bullet Information")]
    [SerializeField] private Character _owner;
    [SerializeField] [Networked] private float _scale { get; set; }
    [SerializeField] [Networked] private float _damage { get; set; }
    [SerializeField] [Networked] private float _headShotMultiplier { get; set; }
    [SerializeField] [Networked] private float _pierceAmount { get; set; }
    [SerializeField] [Networked] private float _knockBackForce { get; set; }
    [SerializeField] [Networked] private Vector3 _velocity { get; set; }
    [SerializeField] [Networked] private float _distanceToTravel { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public Character Owner { get { return _owner; } }
    public bool IsOwner(Character character) { return _owner == character; }
    public float Damage { get { return _damage; } }
    public float HeadShotDamage {  get { return _damage * _headShotMultiplier; } }
    public Vector2 KnockBackForce { get { return _velocity.normalized * _knockBackForce; } }
    public bool Finished { get { return _pierceAmount < 0 || _distanceToTravel <= 0; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllBullets, this.gameObject);

        _networkRigidBody2D = GetComponent<NetworkRigidbody2D>();
    }

    public void SetUp(Character owner, Vector3 position, float scale, float rotationAngleDegrees, float speed, float maxRange, float damage, float headShotMultiplier, int pierceAmount, float knockBackForce)
    {
        _owner = owner;

        transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngleDegrees);
        _scale = scale;
        _velocity = new Vector3(Mathf.Cos(rotationAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(rotationAngleDegrees * Mathf.Deg2Rad), 0).normalized * speed;
        _distanceToTravel = maxRange;

        _damage = damage;
        _headShotMultiplier = headShotMultiplier;
        _pierceAmount = pierceAmount;
        _knockBackForce = knockBackForce;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Update
    public override void FixedUpdateNetwork()
    {
        //Movement
        Vector3 changeVector = _velocity * Runner.DeltaTime;
        transform.position += changeVector;
        _distanceToTravel -= changeVector.magnitude;

        //Update
        transform.localScale = new Vector3(_scale, _scale, _scale);

        if (Finished) HandleDestruction();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void HitSomething()
    {
        _pierceAmount--;
    }

    public void HitSomethingSemiPenetrable()
    {
        _damage /= 2;
        _scale /= 2;
        _distanceToTravel /= 2;
    }

    public void HitSomethingImpenetrable()
    {
        _pierceAmount = -1;
    }

    public void HandleDestruction()
    {
        Runner.Despawn(this.Object);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
