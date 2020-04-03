using UnityEditor;

namespace Lasp.Editor
{
    //
    // Utility class for presenting the dynamic range and auto gain properties
    //
    sealed class DynamicRangeEditor
    {
        SerializedProperty _dynamicRange;
        SerializedProperty _autoGain;
        SerializedProperty _gain;

        static class Styles
        {
            public static Label DynamicRange = "Dynamic Range (dB)";
            public static Label Gain         = "Gain (dB)";
        }

        public DynamicRangeEditor(SerializedObject so)
        {
            var finder = new PropertyFinder(so);
            _dynamicRange = finder["_dynamicRange"];
            _autoGain     = finder["_autoGain"];
            _gain         = finder["_gain"];
        }

        public void ShowGUI()
        {
            EditorGUILayout.PropertyField(_dynamicRange, Styles.DynamicRange);
            EditorGUILayout.PropertyField(_autoGain);

            if (_autoGain.hasMultipleDifferentValues || !_autoGain.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_gain, Styles.Gain);
                EditorGUI.indentLevel--;
            }
        }

        public void ShowResetPeakButton(UnityEngine.Object[] targets)
        {
            if (!EditorApplication.isPlaying) return;

            EditorGUILayout.Space();

            if (UnityEngine.GUILayout.Button("Reset Auto Gain"))
                foreach (UnityEngine.Component t in targets)
                    t.SendMessage("ResetAutoGain");
        }
    }
}
