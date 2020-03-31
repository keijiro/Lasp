using UnityEditor;

namespace Lasp.Editor
{
    //
    // Custom editor (inspector) for AudioLevelTracker
    //
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AudioLevelTracker))]
    sealed class AudioLevelTrackerEditor : UnityEditor.Editor
    {
        #region Private members

        SerializedProperty _channel;
        SerializedProperty _filterType;
        SerializedProperty _dynamicRange;
        SerializedProperty _autoGain;
        SerializedProperty _gain;
        SerializedProperty _smoothFall;
        SerializedProperty _fallSpeed;

        DeviceSelector _deviceSelector;
        PropertyBinderEditor _propertyBinderEditor;

        #endregion

        #region Labels

        static class Styles
        {
            public static Label DynamicRange = "Dynamic Range (dB)";
            public static Label Gain         = "Gain (dB)";
        }

        #endregion

        #region Editor implementation

        void OnEnable()
        {
            var finder = new PropertyFinder(serializedObject);

            _channel      = finder["_channel"];
            _filterType   = finder["_filterType"];
            _dynamicRange = finder["_dynamicRange"];
            _autoGain     = finder["_autoGain"];
            _gain         = finder["_gain"];
            _smoothFall   = finder["_smoothFall"];
            _fallSpeed    = finder["_fallSpeed"];

            _deviceSelector = new DeviceSelector(serializedObject);
            _propertyBinderEditor
              = new PropertyBinderEditor(finder["_propertyBinders"]);
        }

        public override bool RequiresConstantRepaint()
        {
            // Keep updated while playing.
            return EditorApplication.isPlaying && targets.Length == 1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Device selection (disabled during play mode)
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                _deviceSelector.ShowGUI();

            // Input settings
            EditorGUILayout.PropertyField(_channel);
            EditorGUILayout.PropertyField(_filterType);
            EditorGUILayout.PropertyField(_dynamicRange, Styles.DynamicRange);
            EditorGUILayout.PropertyField(_autoGain);

            // Show Gain when no peak tracking.
            if (_autoGain.hasMultipleDifferentValues ||
                !_autoGain.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_gain, Styles.Gain);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_smoothFall);

            // Show Fall Speed when Smooth Fall is on.
            if (_smoothFall.hasMultipleDifferentValues ||
                _smoothFall.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_fallSpeed);
                EditorGUI.indentLevel--;
            }

            // Draw the level meter during play mode.
            if (RequiresConstantRepaint())
            {
                EditorGUILayout.Space();
                LevelMeterDrawer.DrawMeter((AudioLevelTracker)target);
            }

            // Show Reset Peak Level button during play mode.
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                if (UnityEngine.GUILayout.Button("Reset Auto Gain"))
                    foreach (AudioLevelTracker t in targets) t.ResetAutoGain();
            }

            serializedObject.ApplyModifiedProperties();

            // Property binders
            if (targets.Length == 1) _propertyBinderEditor.ShowGUI();
        }

        #endregion
    }
}
