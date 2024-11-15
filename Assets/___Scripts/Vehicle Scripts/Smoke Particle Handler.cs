using UnityEngine;

public class WheelParticleHandler : MonoBehaviour
{
    [Header("Components")]
    private Vehicle _vehicle;
    private ParticleSystem _particleSystemSmoke;
    private ParticleSystem.EmissionModule _particleSystemEmissionModule;

    [Header("Variables")]
    private float _maxParticleEmissionRate = 100;
    private float _particleEmissionRate = 0;
    private float _particleEmissionRateLerpRatio = 5;

    private void Awake()
    {
        _vehicle = GetComponentInParent<Vehicle>();
        _particleSystemSmoke = GetComponent<ParticleSystem>();

        _particleSystemEmissionModule = _particleSystemSmoke.emission;

        _particleSystemEmissionModule.rateOverTime = 0;
    }

    void Update()
    {
        _particleEmissionRate = Mathf.Lerp(_particleEmissionRate, 0, Time.deltaTime * _particleEmissionRateLerpRatio);
        _particleSystemEmissionModule.rateOverTime = _particleEmissionRate;

        if (_vehicle.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
                _particleEmissionRate = _maxParticleEmissionRate;
            else
                _particleEmissionRate = Mathf.Abs(lateralVelocity);
        }
    }
}
