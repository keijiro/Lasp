using UnityEngine;
using System.Collections.Generic;

public class FftTester : MonoBehaviour
{
    [SerializeField] [Range(0, 32)] float _amplitude = 1.0f;
    [SerializeField] [Range(2, 64)] int _nFft = 11;

    float[] _fft;
    private int _bands;
    private GameObject[] _objects;

    void Start()
    {
        Initialize();
    }

    void OnDestroy()
    {
        foreach (var obj in _objects)
        {
            Destroy(obj);
        }
    }

    void Update()
    {
        Lasp.MasterInput.RetrieveFft(_fft, _nFft);
        UpdateFftCubes();
    }

    void Initialize()
    {
        _bands = _nFft;
        _fft = new float[_nFft];
        _objects = new GameObject[_nFft];
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
            _objects[i] = cube;
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
            var height = Mathf.Clamp(_amplitude * _fft[i] * (i + 1) / 2, 0, 1);
            var scale = _objects[i].transform.localScale;
            scale.y = height;
            _objects[i].transform.localScale = scale;
        }
    }
}
