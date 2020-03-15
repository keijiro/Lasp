using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lasp.Editor
{
    sealed class PropertySelector
    {
        #region Static members

        // Return a selector instance for a given type pair.
        public static PropertySelector GetInstance
          (SerializedProperty spTarget, SerializedProperty spPropertyType)
        {
            var key = spTarget.objectReferenceValue.GetType()
                      + spPropertyType.stringValue;

            // Try getting from the dictionary.
            PropertySelector selector;
            if (_instances.TryGetValue(key, out selector)) return selector;

            // New instance
            selector = new PropertySelector(spTarget, spPropertyType);
            _instances[key] = selector;
            return selector;
        }

        static Dictionary<string, PropertySelector> _instances
          = new Dictionary<string, PropertySelector>();

        #endregion

        #region Private constructor

        PropertySelector
          (SerializedProperty spTarget, SerializedProperty spPropertyType)
        {
            // Determine the target property type using reflection.
            _propertyType = Type.GetType(spPropertyType.stringValue);

            // Property name candidates query
            _candidates = spTarget.objectReferenceValue.GetType()
              .GetProperties(BindingFlags.Public | BindingFlags.Instance)
              .Where(prop => prop.PropertyType == _propertyType)
              .Select(prop => prop.Name).ToArray();
        }

        Type _propertyType;
        string [] _candidates;

        #endregion

        #region GUI implementation

        public bool ShowGUI(SerializedProperty spPropertyName)
        {
            // Clear the selection and show a message if there is no candidate.
            if (_candidates.Length == 0)
            {
                EditorGUILayout.HelpBox
                  ($"No {_propertyType.Name} property found.",
                   MessageType.None);
                spPropertyName.stringValue = null;
                return false;
            }

            // Index of the current selection
            var index = Array.IndexOf(_candidates, spPropertyName.stringValue);

            // Drop down list
            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup("Property", index, _candidates);
            if (EditorGUI.EndChangeCheck())
                spPropertyName.stringValue = _candidates[index];

            // Return true only when the selection is valid.
            return index >= 0;
        }

        #endregion
    }
}
