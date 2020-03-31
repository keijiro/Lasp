using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    //
    // Custom editor (inspector) for SpectrumAnalyzer
    //
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpectrumAnalyzer))]
    sealed class SpectrumAnalyzerEditor : UnityEditor.Editor
    {
        SerializedProperty _channel;
        SerializedProperty _resolution;

        DeviceSelector _deviceSelector;

        static GUIContent [] _resolutionLabels = {
            new GUIContent("256"), new GUIContent("512"),
            new GUIContent("1024"), new GUIContent("2048")
        };

        static int [] _resolutionOptions = { 128, 256, 512, 1024, 2048 };

        public override bool RequiresConstantRepaint()
        {
            // Keep updated while playing.
            return EditorApplication.isPlaying && targets.Length == 1;
        }

        void OnEnable()
        {
            var finder = new PropertyFinder(serializedObject);
            _channel = finder["_channel"];
            _resolution = finder["_resolution"];

            _deviceSelector = new DeviceSelector(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Device selection (disabled during play mode)
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                _deviceSelector.ShowGUI();

            // Input settings
            EditorGUILayout.PropertyField(_channel);
            EditorGUILayout
              .IntPopup(_resolution, _resolutionLabels, _resolutionOptions);

            // Spectrum graph
            if (targets.Length == 1 && EditorApplication.isPlaying)
                SpectrumDrawer.DrawGraph(((SpectrumAnalyzer)target).SpectrumSpan);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
