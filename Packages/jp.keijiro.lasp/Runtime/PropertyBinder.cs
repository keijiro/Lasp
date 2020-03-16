using UnityEngine;
using UnityEngine.Events;

namespace Lasp
{
    //
    // Property binder classes used for driving properties of external objects
    // by audio level
    //

    // Property binder base class
    [System.Serializable]
    public abstract class PropertyBinder
    {
        // Enable switch
        public bool Enabled = true;

        // Audio level property (setter only)
        public float Level { set { if (Enabled) OnSetLevel(value); } }

        // Binder implementation
        protected abstract void OnSetLevel(float level);
    }

    // Generic intermediate implementation
    public abstract class GenericPropertyBinder<T> : PropertyBinder
    {
        // Serialized target property information
        public Component Target;
        public string PropertyName;

        // This field in only used in Editor to determine the target property
        // type. Don't modify it after instantiation.
        [SerializeField, HideInInspector]
        string _propertyType = typeof(T).AssemblyQualifiedName;

        // Target property setter
        protected T TargetProperty { set => SetTargetProperty(value); }

        UnityAction<T> _setterCache;

        void SetTargetProperty(T value)
        {
            if (_setterCache == null)
            {
                if (Target == null) return;
                if (string.IsNullOrEmpty(PropertyName)) return;

                _setterCache
                  = (UnityAction<T>)System.Delegate.CreateDelegate
                    (typeof(UnityAction<T>), Target, "set_" + PropertyName);
            }

            _setterCache(value);
        }
    }

    // Binder for float properties
    public sealed class FloatPropertyBinder : GenericPropertyBinder<float>
    {
        public float Value0 = 0;
        public float Value1 = 1;

        protected override void OnSetLevel(float level)
          => TargetProperty = Mathf.Lerp(Value0, Value1, level);
    }

    // Binder for vector (Vector3) properties
    public sealed class Vector3PropertyBinder : GenericPropertyBinder<Vector3>
    {
        public Vector3 Value0 = Vector3.zero;
        public Vector3 Value1 = Vector3.one;

        protected override void OnSetLevel(float level)
          => TargetProperty = Vector3.Lerp(Value0, Value1, level);
    }

    // Binder for quaternion properties, controlled with Euler angles
    public sealed class EulerRotationPropertyBinder
      : GenericPropertyBinder<Quaternion>
    {
        public Vector3 Value0 = Vector3.zero;
        public Vector3 Value1 = new Vector3(0, 90, 0);

        protected override void OnSetLevel(float level)
          => TargetProperty
             = Quaternion.Euler(Vector3.Lerp(Value0, Value1, level));
    }

    // Binder for color properties
    public sealed class ColorPropertyBinder : GenericPropertyBinder<Color>
    {
        public Color Value0 = Color.black;
        public Color Value1 = Color.white;

        protected override void OnSetLevel(float level)
          => TargetProperty = Color.Lerp(Value0, Value1, level);
    }
}
