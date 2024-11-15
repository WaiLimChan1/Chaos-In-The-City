using CITC.GameManager;
using UnityEngine;

public class BulletCasing : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Sprites
    public enum BulletCasingType { Pistol, AssaultRifle, SniperRifle, Shotgun };
    [Header("Bullet Casing Sprites")]
    [SerializeField] private Sprite _pistolBulletCasing;
    [SerializeField] private Sprite _assaultRifleBulletCasing;
    [SerializeField] private Sprite _sniperRifleBulletCasing;
    [SerializeField] private Sprite _shotgunBulletCasing;
    //------------------------------------------------------------------------------------------------------------------------------------------------


    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Bullet Casing Components")]
    private SpriteRenderer _spriteRenderer;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //All of the SerializeFielded values will NOT change. This way there is no conflict with object pooling. They should all be static const when project is done
    //The non SerializedFielded values must be reset during object pooling. 
    [Header("Bullet Casing STATIC Variables")]
    [SerializeField] private float lifeTime;
    [SerializeField] private Vector2 initialVelocityAngleRange;
    [SerializeField] private float initialSpeed;
    [SerializeField] private float downwardAcceleration;
    [SerializeField] private Vector2 angularVelocityRange;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Bullet Casing Dynamic Variables")]
    private float _remainingLifeTime;
    private Vector3 _velocity;
    private float _currentAngle;
    private float _angularVelocity;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    //public bool outOfBound { get { return Vector3.Distance(this.transform.position, Camera.main.transform.position) >= Main.Instance.outOfBoundDistance; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    private void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllBulletCasings, this.gameObject);

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void SetUpType(BulletCasingType bulletCasingType)
    {
        _spriteRenderer.sprite = _pistolBulletCasing;

        if (bulletCasingType == BulletCasingType.Pistol) _spriteRenderer.sprite = _pistolBulletCasing;
        else if (bulletCasingType == BulletCasingType.AssaultRifle) _spriteRenderer.sprite = _assaultRifleBulletCasing;
        else if (bulletCasingType == BulletCasingType.SniperRifle) _spriteRenderer.sprite = _sniperRifleBulletCasing;
        else if (bulletCasingType == BulletCasingType.Shotgun) _spriteRenderer.sprite = _shotgunBulletCasing;
    }

    public void SetUp(BulletCasingType bulletCasingType, Vector3 position)
    {
        SetUpType(bulletCasingType);
        transform.position = position;

        _remainingLifeTime = lifeTime;

        float initalVelocityAngle = Random.Range(initialVelocityAngleRange.x, initialVelocityAngleRange.y);
        _velocity = new Vector3(Mathf.Cos(initalVelocityAngle * Mathf.Deg2Rad), Mathf.Sin(initalVelocityAngle * Mathf.Deg2Rad), 0).normalized * initialSpeed;

        _currentAngle = 0;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, _currentAngle);
        _angularVelocity = Random.Range(angularVelocityRange.x, angularVelocityRange.y);
    } 
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Logic
    private void Update()
    {
        //Movement
        _velocity = new Vector3(_velocity.x, _velocity.y - downwardAcceleration * Time.deltaTime, 0);
        transform.position += _velocity * Time.deltaTime;

        //Rotation
        _currentAngle += _angularVelocity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, _currentAngle);

        //LifeTime
        _remainingLifeTime -= Time.deltaTime;
        if (_remainingLifeTime <= 0) HandleDestruction();
        //if (outOfBound) HandleDestruction();
    }

    private void HandleDestruction()
    {
        Destroy(gameObject);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
