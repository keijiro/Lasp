using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public float input { get; set; }

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
        _emission.rateOverTime = input * _originalEmission;
        _shape.radius = input * _originalRadius;
    }
}
