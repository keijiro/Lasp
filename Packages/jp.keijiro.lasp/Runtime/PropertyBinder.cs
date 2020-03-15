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
        [SerializeField] bool _enabled = true;

        // Audio level property (setter only)
        public float Level { set { if (_enabled) OnSetLevel(value); } }

        // Binder implementation
        protected abstract void OnSetLevel(float level);
    }

    // Generic intermediate implementation
    public abstract class GenericPropertyBinder<T> : PropertyBinder
    {
        // Serialized target property information
        [SerializeField] Component _target = null;
        [SerializeField] string _propertyName = null;

        // This field in only used in Editor to determine the target property
        // type. Don't modify it after instantiation.
        [SerializeField, HideInInspector] string _propertyType
          = typeof(T).AssemblyQualifiedName;

        // Target property setter
        protected T TargetProperty { set => SetTargetProperty(value); }

        UnityAction<T> _setterCache;

        void SetTargetProperty(T value)
        {
            if (_setterCache == null)
            {
                if (_target == null) return;
                if (string.IsNullOrEmpty(_propertyName)) return;

                _setterCache
                  = (UnityAction<T>)System.Delegate.CreateDelegate
                    (typeof(UnityAction<T>), _target, "set_" + _propertyName);
            }

            _setterCache(value);
        }
    }

    // Binder for float properties
    public sealed class FloatPropertyBinder : GenericPropertyBinder<float>
    {
        [SerializeField] float _value0 = 0;
        [SerializeField] float _value1 = 1;

        protected override void OnSetLevel(float level)
          => TargetProperty = Mathf.Lerp(_value0, _value1, level);
    }

    // Binder for vector (Vector3) properties
    public sealed class Vector3PropertyBinder : GenericPropertyBinder<Vector3>
    {
        [SerializeField] Vector3 _value0 = Vector3.zero;
        [SerializeField] Vector3 _value1 = Vector3.one;

        protected override void OnSetLevel(float level)
          => TargetProperty = Vector3.Lerp(_value0, _value1, level);
    }

    // Binder for quaternion properties, controlled with Euler angles
    public sealed class EulerRotationPropertyBinder
      : GenericPropertyBinder<Quaternion>
    {
        [SerializeField] Vector3 _value0 = Vector3.zero;
        [SerializeField] Vector3 _value1 = new Vector3(0, 90, 0);

        protected override void OnSetLevel(float level)
          => TargetProperty
             = Quaternion.Euler(Vector3.Lerp(_value0, _value1, level));
    }

    // Binder for color properties
    public sealed class ColorPropertyBinder : GenericPropertyBinder<Color>
    {
        [SerializeField] Color _value0 = Color.black;
        [SerializeField] Color _value1 = Color.white;

        protected override void OnSetLevel(float level)
          => TargetProperty = Color.Lerp(_value0, _value1, level);
    }
}
