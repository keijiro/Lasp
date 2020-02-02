using System;
using UnityEngine;
using System.Collections.Generic;
using Lasp;

public class FftTester : MonoBehaviour
{
    [SerializeField] FftAveragingType _averagingType = Lasp.FftAveragingType.Logarithmic;
    [SerializeField] [Range(0, 32)] float _inputGain = 1.0f;
    [SerializeField] [Range(1, 64)] int _fftBands = 11;
    [SerializeField] bool _holdAndFallDown = true;
    [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.1f;

    private int _bands;
    private float _fall = 0;
    private float[] _fftIn, _fftOut;
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
        Lasp.MasterInput.RetrieveFft(_averagingType, _fftIn, _fftBands);
        UpdateFftCubes();
    }

    void Initialize()
    {
        _bands = _fftBands;
        _fftIn = new float[_fftBands];
        _fftOut = new float[_fftBands];
        _cubes = new GameObject[_fftBands];
        CreateFftCubes();
    }

    void CreateFftCubes()
    {
        var scale = 2.0f / _fftBands;
        for (var i = 0; i < _fftBands; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cubeWidth = 1 / (float) _fftBands;
            cube.transform.position = new Vector3(scale * i - 1 + cubeWidth, 0, 0);
            cube.transform.localScale = new Vector3(cubeWidth, 1, 1);
            _cubes[i] = cube;
        }
    }

    void UpdateFftCubes()
    {
        if (_bands != _fftBands)
        {
            OnDestroy();
            Initialize();
        }

        var gain = _averagingType == FftAveragingType.Linear ? _inputGain : _inputGain / 10.0f;
        for (var i = 0; i < _fftBands; i++)
        {
            var input = Mathf.Clamp01(gain * _fftIn[i] * (3 * i + 1));
            var dt = Time.deltaTime;
            if (_holdAndFallDown)
            {
                // Hold-and-fall-down animation.
                _fall += Mathf.Pow(10, 1 + _fallDownSpeed *2) * dt;
                _fftOut[i] -= _fall * dt;

                // Pull up by input.
                if (_fftOut[i] < input)
                {
                    _fftOut[i] = input;
                    _fall = 0;
                }
            }
            else
            {
                _fftOut[i] = input;
            }

            var height = Mathf.Clamp(_fftOut[i], 0, 1);
            var scale = _cubes[i].transform.localScale;
            scale.y = height;
            _cubes[i].transform.localScale = scale;
        }
    }
}