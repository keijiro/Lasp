using UnityEngine;

namespace Lasp
{
    internal sealed class LaspUpdater : MonoBehaviour
    {
        public static void Create()
        {
            var go = new GameObject("LASP Updater");
            go.AddComponent<LaspUpdater>();
            go.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(go);
        }

        void OnDestroy()
        {
            LaspInput.Terminate();
        }
    }
}
