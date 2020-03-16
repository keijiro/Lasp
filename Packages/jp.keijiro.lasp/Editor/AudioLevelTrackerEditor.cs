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
        #region Private members

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

        #endregion

        #region Labels

        static class Styles
        {
            public static Label NoDevice      = "No device available";
            public static Label DefaultDevice = "Default Device";
            public static Label Select        = "Select";
            public static Label DynamicRange  = "Dynamic Range (dB)";
            public static Label Gain          = "Gain (dB)";
            public static Label Speed         = "Speed";
        }

        #endregion

        #region Device selector

        void ShowDeviceSelector()
        {
            // Use Default Device switch
            EditorGUILayout.PropertyField
              (_useDefaultDevice, Styles.DefaultDevice);

            if (_useDefaultDevice.hasMultipleDifferentValues ||
                !_useDefaultDevice.boolValue)
            {
                // ID field and selector dropdown
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_deviceID);
                var rect = EditorGUILayout.GetControlRect
                             (false, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.DropdownButton
                      (rect, Styles.Select, FocusType.Keyboard))
                    CreateDeviceSelectMenu().DropDown(rect);
            }
        }

        GenericMenu CreateDeviceSelectMenu()
        {
            var menu = new GenericMenu();
            var devices = Lasp.AudioSystem.InputDevices;

            if (devices.Any())
                foreach (var dev in devices)
                    menu.AddItem(new GUIContent(dev.Name),
                                 false, OnSelectDevice, dev.ID);
            else
                menu.AddItem(Styles.NoDevice, false, null);

            return menu;
        }

        void OnSelectDevice(object id)
        {
            serializedObject.Update();
            // Trash the stringValue before setting the ID
            // to avoid issue #1228004.
            _deviceID.stringValue = "xx.invalid.id.xx";
            _deviceID.stringValue = (string)id;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Editor implementation

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

            // Device selection (disabled during play mode)
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                ShowDeviceSelector();

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

        #endregion
    }
}
