// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using UnityEngine;
using UnityEditor;

namespace Lasp
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LevelMonitor))]
    public class LevelMonitorEditor : Editor
    {
        SerializedProperty _filterType;
        SerializedProperty _dynamicRange;
        SerializedProperty _autoGain;
        SerializedProperty _gain;
        SerializedProperty _holdAndFallDown;
        SerializedProperty _fallDownSpeed;
        SerializedProperty _outputEvent;

        static GUIContent _labelAutoGain = new GUIContent("Auto Gain Control");
        static GUIContent _labelDynamicRange = new GUIContent("Dynamic Range");
        static GUIContent _labelDynamicRangeWide = new GUIContent("Dynamic Range (dB)");
        static GUIContent _labelGain = new GUIContent("Gain (dB)");
        static GUIContent _labelSpeed = new GUIContent("Speed");

        void OnEnable()
        {
            _filterType = serializedObject.FindProperty("_filterType");
            _dynamicRange = serializedObject.FindProperty("_dynamicRange");
            _autoGain = serializedObject.FindProperty("_autoGain");
            _gain = serializedObject.FindProperty("_gain");
            _holdAndFallDown = serializedObject.FindProperty("_holdAndFallDown");
            _fallDownSpeed = serializedObject.FindProperty("_fallDownSpeed");
            _outputEvent = serializedObject.FindProperty("_outputEvent");
        }

        public override bool RequiresConstantRepaint()
        {
            // Keep updated while playing.
            return Application.isPlaying && targets.Length == 1;
        }

        public override void OnInspectorGUI()
        {
            var wide = EditorGUIUtility.labelWidth > 132;

            serializedObject.Update();

            EditorGUILayout.PropertyField(_filterType);
            EditorGUILayout.PropertyField(_dynamicRange, wide ? _labelDynamicRangeWide : _labelDynamicRange);
            EditorGUILayout.PropertyField(_autoGain, _labelAutoGain);

            if (_autoGain.hasMultipleDifferentValues || !_autoGain.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_gain, _labelGain);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_holdAndFallDown);

            if (_holdAndFallDown.hasMultipleDifferentValues || _holdAndFallDown.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_fallDownSpeed, _labelSpeed);
                EditorGUI.indentLevel--;
            }

            if (RequiresConstantRepaint())
            {
                EditorGUILayout.Space();
                DrawMeter((LevelMonitor)target);
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Reset Auto Gain"))
                    foreach (LevelMonitor lm in targets) lm.ResetAutoGain();
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_outputEvent);

            serializedObject.ApplyModifiedProperties();
        }

        // Draw a VU meter with a given LevelMonitor instance.
        void DrawMeter(LevelMonitor monitor)
        {
            var rect = GUILayoutUtility.GetRect(128, 9);

            const float kMeterRange = 60;
            var amp  = 1 + monitor.inputAmplitude / kMeterRange;
            var peak = 1 - monitor.calculatedGain / kMeterRange;
            var dr = monitor.dynamicRange / kMeterRange;

            // Background
            DrawRect(0, 0, 1, 1, rect, new Color(0.1f, 0.1f, 0.1f, 1));

            // Dynamic range indicator
            DrawRect(peak - dr, 0, peak, 1, rect, new Color(0.3f, 0.3f, 0.3f, 1));

            // Amplitude bar
            var x1 = Mathf.Min(amp, peak - dr);
            var x2 = Mathf.Min(peak, amp);
            DrawRect(0, 0, x1, 1, rect, new Color(0, 0.3f, 0, 1)); // under the range
            DrawRect(x1, 0, x2, 1, rect, new Color(0, 0.7f, 0, 1)); // inside the range
            DrawRect(x2, 0, amp, 1, rect, Color.red);  // over the range

            // Output level bar
            var x3 = peak + dr * (monitor.outputAmplitude - 1);
            DrawRect(x3 - 3 / rect.width, 0, x3, 1, rect, Color.green);

            // Label: -60dB
            Handles.Label(
                new Vector2(rect.xMin + 1, rect.yMax - 10),
                "-60dB", EditorStyles.miniLabel
            );

            // Label: 0dB
            Handles.Label(
                new Vector2(rect.xMin + rect.width - 22, rect.yMax - 10),
                "0dB", EditorStyles.miniLabel
            );
        }

        // Vertex array for drawing rectangles: Reused to avoid GC allocation.
        Vector3 [] _rectVertices = new Vector3 [4];

        // Draw a rectangle with normalized coordinates.
        void DrawRect(float x1, float y1, float x2, float y2, Rect area, Color color)
        {
            x1 = area.xMin + area.width  * Mathf.Clamp01(x1);
            x2 = area.xMin + area.width  * Mathf.Clamp01(x2);
            y1 = area.yMin + area.height * Mathf.Clamp01(y1);
            y2 = area.yMin + area.height * Mathf.Clamp01(y2);

            _rectVertices[0] = new Vector2(x1, y1);
            _rectVertices[1] = new Vector2(x1, y2);
            _rectVertices[2] = new Vector2(x2, y2);
            _rectVertices[3] = new Vector2(x2, y1);

            Handles.DrawSolidRectangleWithOutline(_rectVertices, color, Color.clear);
        }
    }
}
