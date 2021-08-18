using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    //
    // A utility class for drawing a spectrum graph
    //
    static class SpectrumDrawer
    {
        static Vector3[] _vertices = new Vector3[160];

        #if UNITY_2021_2_OR_NEWER
        // To avoid issue 1358691 (compilation error with ReadOnlySpan),
        // we copy the ReadOnlySpan to a temporary span before accessing.
        static float[] _temp = new float[4096];
        public static void DrawGraph(System.ReadOnlySpan<float> spectrum_ro)
        {
            var spectrum = new System.Span<float>(_temp, 0, spectrum_ro.Length);
            spectrum_ro.CopyTo(spectrum);
        #else
        public static void DrawGraph(System.ReadOnlySpan<float> spectrum)
        {
        #endif
            EditorGUILayout.Space();

            // Graph area
            var rect = GUILayoutUtility.GetRect(128, 64);

            // Background
            Handles.DrawSolidRectangleWithOutline
              (rect, new Color(0.1f, 0.1f, 0.1f, 1), Color.clear);

            // Don't draw the actual graph if it isn't a repaint event.
            if (Event.current.type != EventType.Repaint) return;

            // Spectrum curve construction
            for (var i = 0; i < _vertices.Length; i++)
            {
                var x = (float)i / _vertices.Length;
                var y = spectrum[i * spectrum.Length / _vertices.Length];

                x = x * rect.width + rect.xMin;
                y = rect.yMax - y * rect.height;

                _vertices[i] = new Vector3(x, y, 0);
            }

            // Curve
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(_vertices);
        }
    }
}
