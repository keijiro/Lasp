using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lasp.Editor
{
    sealed class ComponentSelector
    {
        #region Static members

        // Return a selector instance for a given target.
        public static ComponentSelector GetInstance(SerializedProperty spTarget)
        {
            var component = spTarget.objectReferenceValue as Component;

            // Special case: The target is not specified.
            if (component == null) return _nullInstance;

            var gameObject = component.gameObject;

            // Try getting from the dictionary.
            ComponentSelector selector;
            if (_instances.TryGetValue(gameObject, out selector))
                return selector;

            // New instance
            selector = new ComponentSelector(gameObject);
            _instances[gameObject] = selector;
            return selector;
        }

        // Clear the cache contents.
        // It's recommended to invoke when the inspector is initiated.
        public static void InvalidateCache() => _instances.Clear();

        static Dictionary<GameObject, ComponentSelector> _instances
          = new Dictionary<GameObject, ComponentSelector>();

        static ComponentSelector _nullInstance = new ComponentSelector(null);

        #endregion

        #region Private constructor

        ComponentSelector(GameObject gameObject)
          => _candidates = gameObject?.GetComponents<Component>()
             .Select(c => c.GetType().Name).ToArray();

        string [] _candidates;

        #endregion

        #region GUI implementation

        public bool ShowGUI(SerializedProperty spTarget)
        {
            if (_candidates == null) return false;

            var component = (Component)spTarget.objectReferenceValue;
            var gameObject = component.gameObject;

            // Current selection
            var index = Array.IndexOf(_candidates, component.GetType().Name);

            // Component selection drop down
            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup("Component", index, _candidates);
            if (EditorGUI.EndChangeCheck())
                spTarget.objectReferenceValue =
                  gameObject.GetComponent(_candidates[index]);

            return true;
        }

        #endregion
    }
}
