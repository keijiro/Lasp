using System;
using UnityEngine;
using System.Collections.Generic;

public class FftTester : MonoBehaviour
{
    [SerializeField] [Range(0, 32)] float _amplitude = 1.0f;
    [SerializeField] [Range(2, 64)] int _nFft = 11;
    [SerializeField] bool _holdAndFallDown = true;
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.3f;

    private float[] _fftIn, _fftAmplitude;
    private int _bands;
    private float _fall = 0;
    private GameObject[] _cubes;

    void Start()
    {
        Initialize();
    }

    void OnDestroy()
    {
        foreach (var obj in _cubes)
        {
            Destroy(obj);
        }
    }

    void Update()
    {
        Lasp.MasterInput.RetrieveFft(_fftIn, _nFft);
        UpdateFftCubes();
    }

    void Initialize()
    {
        _bands = _nFft;
        _fftIn = new float[_nFft];
        _fftAmplitude = new float[_nFft];
        _cubes = new GameObject[_nFft];
        CreateFftCubes();
    }
    void CreateFftCubes()
    {
        var scale = 2.0f / _nFft;
        for (var i = 0; i < _nFft; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(scale * i - 1, 0, 0);
            cube.transform.localScale = new Vector3(1 / (float) _nFft, 1, 1);
            _cubes[i] = cube;
        }
    }

    void UpdateFftCubes()
    {
        if (_bands != _nFft)
        {
            OnDestroy();
            Initialize();
        }

        for (var i = 0; i < _nFft; i++)
        {
            if (_holdAndFallDown)
            {
                var dt = Time.deltaTime;
                if (_holdAndFallDown)
                {
                    // Hold-and-fall-down animation.
                    _fall += Mathf.Pow(10, 1 + _fallDownSpeed * 2) * dt;
                    _fftAmplitude[i] -= _fall * dt;

                    // Pull up by input.
                    if (_fftAmplitude[i] < _fftIn[i])
                    {
                        _fftAmplitude[i] = _fftIn[i];
                        _fall = 0;
                    }
                }
                else
                {
                    _fftAmplitude[i] = _fftIn[i];
                }
            }
            else
            {
                _fftAmplitude[i] = _fftIn[i];
            }

            var height = Mathf.Clamp(_amplitude * _fftAmplitude[i] * (i + 1), 0, 1);
            var scale = _cubes[i].transform.localScale;
            scale.y = height;
            _cubes[i].transform.localScale = scale;
        }
    }
}
