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

            public Renderer Renderer {
                get => _renderer;
                set => _renderer = value;
            }

            public string PropertyName {
                get => _propertyName;
                set => SetPropertyName(value);
            }

            public int PropertyID => _propertyID;

            void SetPropertyName(string name)
            {
                _propertyName = name;
                _propertyID = Shader.PropertyToID(name);
            }
        }

        #endregion

        #region Editable attributes

        // X-axis log scale switch
        [SerializeField] bool _logScale = true;
        public bool logScale {
            get => _logScale;
            set => _logScale = value;
        }

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

        #region Runtime public property

        // Baked spectrum texture
        public Texture texture => _texture;

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
                                         TextureFormat.RFloat, false)
                           { wrapMode = TextureWrapMode.Clamp };

            // Texture update
            if (_logScale)
                _texture.LoadRawTextureData(_analyzer.logSpectrumArray);
            else
                _texture.LoadRawTextureData(_analyzer.spectrumArray);

            _texture.Apply();

            // Update the external render texture.
            if (_renderTexture != null)
                Graphics.CopyTexture(_texture, _renderTexture);

            // Lazy initialization of the material property block.
            if (_block == null) _block = new MaterialPropertyBlock();

            // Apply the material overrides.
            foreach (var o in _overrideList)
            {
                o.Renderer.GetPropertyBlock(_block);
                _block.SetTexture(o.PropertyID, _texture);
                o.Renderer.SetPropertyBlock(_block);
            }
        }

        #endregion
    }
}
