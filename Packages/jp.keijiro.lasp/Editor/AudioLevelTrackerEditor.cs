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
        SerializedProperty _smoothFall;
        SerializedProperty _fallSpeed;

        DeviceSelector _deviceSelector;
        DynamicRangeEditor _dynamicRange;
        PropertyBinderEditor _propertyBinderEditor;

        #endregion

        #region Editor implementation

        void OnEnable()
        {
            var finder = new PropertyFinder(serializedObject);

            _channel      = finder["_channel"];
            _filterType   = finder["_filterType"];
            _smoothFall   = finder["_smoothFall"];
            _fallSpeed    = finder["_fallSpeed"];

            _deviceSelector = new DeviceSelector(serializedObject);
            _dynamicRange = new DynamicRangeEditor(serializedObject);
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
            _dynamicRange.ShowGUI();
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

            // Reset peak button
            _dynamicRange.ShowResetPeakButton(targets);

            serializedObject.ApplyModifiedProperties();

            // Property binders
            if (targets.Length == 1) _propertyBinderEditor.ShowGUI();
        }

        #endregion
    }
}
