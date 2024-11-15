using UnityEngine;

public class SkidMarkHandler : MonoBehaviour
{
    [Header("Components")]
    private Vehicle _vehicle;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _vehicle = GetComponentInParent<Vehicle>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRenderer.emitting = false;
    }

    void Update()
    {
        if (_vehicle.IsTireScreeching(out float lateralVelocity, out bool isBraking))
            _trailRenderer.emitting = true;
        else
            _trailRenderer.emitting = false;
    }
}
