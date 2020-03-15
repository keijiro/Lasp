using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    // Simple string label with GUIContent
    struct Label
    {
        GUIContent _guiContent;

        public static implicit operator GUIContent(Label label)
          => label._guiContent;

        public static implicit operator Label(string text)
          => new Label { _guiContent = new GUIContent(text) };
    }

    // Utilities for finding serialized properties
    struct PropertyFinder
    {
        SerializedObject _so;

        public PropertyFinder(SerializedObject so)
          => _so = so;

        public SerializedProperty this[string name]
          => _so.FindProperty(name);
    }

    struct RelativePropertyFinder
    {
        SerializedProperty _sp;

        public RelativePropertyFinder(SerializedProperty sp)
          => _sp = sp;

        public SerializedProperty this[string name]
          => _sp.FindPropertyRelative(name);
    }

    static class PropertyBinderNameUtil
    {
        public static string Shorten(SerializedProperty prop)
          => ObjectNames.NicifyVariableName(
               prop.managedReferenceFullTypename
               .Replace("Lasp.Runtime Lasp.", ""));
    }

    static class PropertyBinderTypeLabel<T>
    {
        static public GUIContent Content => _gui;
        static GUIContent _gui = new GUIContent
          (ObjectNames.NicifyVariableName(typeof(T).Name)
             .Replace("Property Binder", ""));
    }
}
