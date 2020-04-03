using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    //
    // A utility class for drawing a spectrum graph
    //
    static class SpectrumDrawer
    {
        static Vector3 [] _vertices = new Vector3 [64];

        public static void DrawGraph(System.ReadOnlySpan<float> spectrum)
        {
            var rect = GUILayoutUtility.GetRect(128, 64);

            // Background
            Handles.DrawSolidRectangleWithOutline
              (rect, new Color(0.1f, 0.1f, 0.1f, 1), Color.clear);

            // Spectrum curve
            const float xScale = 0.3f;
            var Nv = _vertices.Length;
            var Ns = spectrum.Length;

            for (var i = 0; i < Nv; i++)
            {
                // X-axis (log scale)
                var x = Mathf.Log(xScale * i        + 1) /
                        Mathf.Log(xScale * (Nv - 1) + 1);

                // Y-axis (normalized linear scale)
                var y = Mathf.Clamp01(spectrum[i * Ns / Nv]);

                // Transform the point into the editor rect.
                x = x * rect.width + rect.xMin;
                y = rect.yMax - y * rect.height;

                _vertices[i] = new Vector3(x, y, 0);
            }

            Handles.color = Color.white;
            Handles.DrawAAPolyLine(_vertices);
        }
    }
}
