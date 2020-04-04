using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    //
    // Utility class for presenting the device selector dropdown
    //
    sealed class DeviceSelector
    {
        #region Public members

        public DeviceSelector(SerializedObject serializedObject)
        {
            var finder = new PropertyFinder(serializedObject);
            _useDefaultDevice = finder["_useDefaultDevice"];
            _deviceID = finder["_deviceID"];
        }

        public void ShowGUI()
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

        #endregion

        #region Labels

        static class Styles
        {
            public static Label NoDevice      = "No device available";
            public static Label DefaultDevice = "Default Device";
            public static Label Select        = "Select";
        }

        #endregion

        #region Private members

        SerializedProperty _useDefaultDevice;
        SerializedProperty _deviceID;

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
            _deviceID.serializedObject.Update();
            // Trash the stringValue before setting the ID
            // to avoid issue #1228004.
            _deviceID.stringValue = "xx.invalid.id.xx";
            _deviceID.stringValue = (string)id;
            _deviceID.serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
