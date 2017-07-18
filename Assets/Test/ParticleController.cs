using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] Lasp.FilterType _filterType;
    [SerializeField] float _amplify = 0;

    ParticleSystem.EmissionModule _emission;
    ParticleSystem.ShapeModule _shape;

    float _originalEmission;
    float _originalRadius;

    void Start()
    {
        var ps = GetComponent<ParticleSystem>();

        _emission = ps.emission;
        _originalEmission = _emission.rateOverTime.constant;

        _shape = ps.shape;
        _originalRadius = _shape.radius;
    }

    void Update()
    {
        var rms = Lasp.AudioInput.CalculateRMSDecibel(_filterType) + _amplify;
        var level = 1 + rms * 0.1f;
        _emission.rateOverTime = Mathf.Clamp01(level * 5) * _originalEmission;
        _shape.radius = Mathf.Clamp01(level) * _originalRadius;
    }
}
