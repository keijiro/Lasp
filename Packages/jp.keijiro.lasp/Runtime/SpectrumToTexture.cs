using UnityEngine;

namespace Lasp
{
    //
    // Spectrum texture baking utility
    //
    [AddComponentMenu("LASP/Utility/Spectrum To Texture")]
    [RequireComponent(typeof(SpectrumAnalyzer))]
    public sealed class SpectrumToTexture : MonoBehaviour
    {
        #region Material override struct

        [System.Serializable]
        public struct MaterialOverride
        {
            [SerializeField] Renderer _renderer;
            [SerializeField] string _propertyName;
            [SerializeField] int _propertyID;

            public Renderer renderer {
                get => _renderer;
                set => _renderer = value;
            }

            public string propertyName {
                get => _propertyName;
                set => SetPropertyName(value);
            }

            public int propertyID => _propertyID;

            void SetPropertyName(string name)
            {
                _propertyName = name;
                _propertyID = Shader.PropertyToID(name);
            }
        }

        #endregion

        #region Editable attributes

        // Bake target render texture
        [SerializeField] RenderTexture _renderTexture = null;
        public RenderTexture renderTexture {
            get => _renderTexture;
            set => _renderTexture = value;
        }

        // Material override list
        [SerializeField] MaterialOverride[] _overrideList = null;
        public MaterialOverride[] overrideList {
            get => _overrideList;
            set => _overrideList = value;
        }

        #endregion

        #region Private members

        SpectrumAnalyzer _analyzer;
        Texture2D _texture;
        MaterialPropertyBlock _block;

        #endregion

        #region MonoBehaviour implementation

        void OnDestroy()
        {
            if (_texture != null) Destroy(_texture);
        }

        void Update()
        {
            // Spectrum analyzer component cache
            if (_analyzer == null) _analyzer = GetComponent<SpectrumAnalyzer>();

            // Refresh the temporary texture when the resolution was changed.
            if (_texture != null && _texture.width != _analyzer.resolution)
            {
                Destroy(_texture);
                _texture = null;
            }

            // Lazy initialization of the temporary texture
            if (_texture == null)
                _texture = new Texture2D(_analyzer.resolution, 1,
                                         TextureFormat.RFloat, false);

            // Texture update
            _texture.LoadRawTextureData(_analyzer.SpectrumArray);
            _texture.Apply();

            // Update the external render texture.
            if (_renderTexture != null)
                Graphics.CopyTexture(_texture, _renderTexture);

            // Lazy initialization of the material property block.
            if (_block == null) _block = new MaterialPropertyBlock();

            // Apply the material overrides.
            foreach (var o in _overrideList)
            {
                o.renderer.GetPropertyBlock(_block);
                _block.SetTexture(o.propertyID, _texture);
                o.renderer.SetPropertyBlock(_block);
            }
        }

        #endregion
    }
}