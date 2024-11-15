using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenEngineSmokeHandler : MonoBehaviour
{
    [Header("Components")]
    private Vehicle _vehicle;
    private ParticleSystem _particleSystemSmoke;
    private ParticleSystem.EmissionModule _particleSystemEmissionModule;

    [Header("Variables")]
    private float _maxParticleEmissionRate = 100;
    private float _particleEmissionRate = 0;
    private float _minVehicleHealthRatioForBrokenEngineSmoke = 0.5f;

    private void Awake()
    {
        _vehicle = GetComponentInParent<Vehicle>();
        _particleSystemSmoke = GetComponent<ParticleSystem>();

        _particleSystemEmissionModule = _particleSystemSmoke.emission;
        _particleSystemEmissionModule.rateOverTime = 0;
    }

    void Update()
    {
        if (_vehicle.Object == null || _vehicle.Object == default) return;

        if (_vehicle.HealthRatio <= _minVehicleHealthRatioForBrokenEngineSmoke) _particleEmissionRate = Mathf.Lerp(0, _maxParticleEmissionRate, 1 - _vehicle.HealthRatio);
        else _particleEmissionRate = 0;

        _particleSystemEmissionModule.rateOverTime = _particleEmissionRate;
    }
}