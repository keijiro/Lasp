using System.Linq;
using UnityEngine;
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
        SerializedProperty _useDefaultDevice;
        SerializedProperty _deviceID;
        SerializedProperty _channel;
        SerializedProperty _filterType;
        SerializedProperty _dynamicRange;
        SerializedProperty _autoGain;
        SerializedProperty _gain;
        SerializedProperty _holdAndFallDown;
        SerializedProperty _fallDownSpeed;

        PropertyBinderEditor _propertyBinderEditor;

        static class Styles
        {
            public static Label NoDevice      = "No device available";
            public static Label DefaultDevice = "Default Device";
            public static Label Select        = "Select";
            public static Label DynamicRange  = "Dynamic Range (dB)";
            public static Label Gain          = "Gain (dB)";
            public static Label Speed         = "Speed";
        }

        // Device selection dropdown menu used for setting the device ID
        void ShowDeviceSelectionDropdown(Rect rect)
        {
            var menu = new GenericMenu();
            var devices = Lasp.AudioSystem.InputDevices;

            if (devices.Any())
                foreach (var dev in devices)
                    menu.AddItem(new GUIContent(dev.Name), false, OnSelectDevice, dev.ID);
            else
                menu.AddItem(Styles.NoDevice, false, null);

            menu.DropDown(rect);
        }

        // Device selection menu item callback
        void OnSelectDevice(object id)
        {
            serializedObject.Update();
            _deviceID.stringValue = (string)id;
            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            var finder = new PropertyFinder(serializedObject);
            _useDefaultDevice = finder["_useDefaultDevice"];
            _deviceID         = finder["_deviceID"];
            _channel          = finder["_channel"];
            _filterType       = finder["_filterType"];
            _dynamicRange     = finder["_dynamicRange"];
            _autoGain         = finder["_autoGain"];
            _gain             = finder["_gain"];
            _holdAndFallDown  = finder["_holdAndFallDown"];
            _fallDownSpeed    = finder["_fallDownSpeed"];

            _propertyBinderEditor
              = new PropertyBinderEditor(finder["_propertyBinders"]);
        }

        public override bool RequiresConstantRepaint()
        {
            // Keep updated while playing.
            return Application.isPlaying && targets.Length == 1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Device selection
            EditorGUILayout.PropertyField(_useDefaultDevice, Styles.DefaultDevice);

            if (_useDefaultDevice.hasMultipleDifferentValues ||
                !_useDefaultDevice.boolValue)
            {
                // ID field and Select dropdown menu
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_deviceID);
                var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));
                if (EditorGUI.DropdownButton(rect, Styles.Select, FocusType.Keyboard))
                    ShowDeviceSelectionDropdown(rect);
                EditorGUILayout.EndHorizontal();
            }

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

            EditorGUILayout.PropertyField(_holdAndFallDown);

            // Show Fall Down Speed when "Hold And Fall Down" is on.
            if (_holdAndFallDown.hasMultipleDifferentValues ||
                _holdAndFallDown.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_fallDownSpeed, Styles.Speed);
                EditorGUI.indentLevel--;
            }

            // Draw the level meter during play mode.
            if (RequiresConstantRepaint())
            {
                EditorGUILayout.Space();
                LevelMeterDrawer.DrawMeter((AudioLevelTracker)target);
            }

            // Show Reset Peak Level button during play mode.
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Reset Auto Gain"))
                    foreach (AudioLevelTracker t in targets) t.ResetAutoGain();
            }

            serializedObject.ApplyModifiedProperties();

            // Property binders
            if (targets.Length == 1) _propertyBinderEditor.ShowGUI();
        }
    }
}
