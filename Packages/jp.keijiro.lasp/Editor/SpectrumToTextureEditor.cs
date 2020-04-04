using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Lasp.Editor
{
    //
    // Custom editor (inspector) for SpectrumToTexture
    //
    [CustomEditor(typeof(SpectrumToTexture))]
    sealed class SpectrumToTextureEditor : UnityEditor.Editor
    {
        #region Inspector implementation

        SerializedProperty _renderTexture;
        ReorderableList _overrideList;

        void OnEnable()
        {
            var finder = new PropertyFinder(serializedObject);

            _renderTexture = finder["_renderTexture"];

            _overrideList = new ReorderableList
              ( serializedObject, finder["_overrideList"],
                true, true, true, true )
              { drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_renderTexture);
            EditorGUILayout.Space();
            _overrideList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region ReorderableList implementation

        void DrawHeader(Rect rect)
          => EditorGUI.LabelField(rect, "Material Override");

        void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var prop =
              _overrideList.serializedProperty.GetArrayElementAtIndex(index);

            var finder = new RelativePropertyFinder(prop);
            var renderer = finder["_renderer"];
            var validRenderer = renderer.objectReferenceValue != null;
            var label = new GUIContent(finder["_propertyName"].stringValue);

            rect.yMin++;
            rect.yMax--;

            // Renderer column
            rect.width = rect.width / 2;
            EditorGUI.PropertyField(rect, renderer, GUIContent.none);

            // PropertyName column (only enabled when the renderer is valid)
            using (new EditorGUI.DisabledScope(!validRenderer))
            {
                rect.x += rect.width + 2;
                if (EditorGUI.DropdownButton(rect, label, FocusType.Keyboard))
                    CreatePropertySelector(prop).DropDown(rect);
            }
        }

        #endregion

        #region Property selector dropdown as generic menu

        GenericMenu CreatePropertySelector(SerializedProperty property)
        {
            var finder = new RelativePropertyFinder(property);
            var renderer = (Renderer)finder["_renderer"].objectReferenceValue;
            var currentName = finder["_propertyName"].stringValue;

            // Retrieve the texture property names from the material object.
            var names = renderer.sharedMaterial.GetTexturePropertyNames();

            // Generic menu construction
            var menu = new GenericMenu();
            foreach (var name in names)
                menu.AddItem(new GUIContent(name), name == currentName,
                             OnSelectPropertyName, (property, name));
            return menu;
        }

        void OnSelectPropertyName(object tuple)
        {
            var data = (System.ValueTuple<SerializedProperty, string>)tuple;
            var finder = new RelativePropertyFinder(data.Item1);
            var serializedObject = data.Item1.serializedObject;

            // Update the property values.
            serializedObject.Update();
            finder["_propertyName"].stringValue = data.Item2;
            finder["_propertyID"].intValue = Shader.PropertyToID(data.Item2);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
