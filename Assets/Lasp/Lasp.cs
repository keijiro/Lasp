using UnityEngine;
using System.Runtime.InteropServices;

public class Lasp : MonoBehaviour
{
    [DllImport("Lasp")]
    public static extern bool LaspInitialize();

    [DllImport("Lasp")]
    public static extern void LaspFinalize();

    [DllImport("Lasp")]
    public static extern float LaspGetPeakLevel();
}
