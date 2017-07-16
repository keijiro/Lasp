// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using UnityEngine;

namespace Lasp
{
    // An internal component that is used to automatically terminates the LASP
    // audio input stream.
    internal sealed class LaspTerminator : MonoBehaviour
    {
        public static void Create(System.Action callback)
        {
            var go = new GameObject("LASP Terminator (hidden)");
            go.AddComponent<LaspTerminator>()._callback = callback;
            go.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(go);
        }

        System.Action _callback;

        void OnDestroy()
        {
            _callback();
        }
    }
}
