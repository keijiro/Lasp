using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {
        Lasp.LaspInitialize();
    }

    void OnDestroy()
    {
        Lasp.LaspFinalize();
    }

    void Update()
    {
        var peak = Lasp.LaspGetPeakLevel();

        transform.localScale = Vector3.one * peak;
    }
}
