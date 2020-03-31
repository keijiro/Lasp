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
            const float refLevel = 0.7071f;
            const float epsilon = 1.5849e-13f;
            const float range = 100;

            var Nv = _vertices.Length;
            var Ns = spectrum.Length;

            Handles.color = Color.white;

            for (var i = 0; i < Nv; i++)
            {
                // Log scale (x-axis)
                var x = Mathf.Log(xScale * i        + 1) /
                        Mathf.Log(xScale * (Nv - 1) + 1);

                // Log scale (y-axis)
                var y = spectrum[i * Ns / Nv];
                y = 20 * Mathf.Log10(y / refLevel + epsilon);

                // Transform the point into the editor rect.
                x = x * rect.width + rect.xMin;
                y = rect.yMin - Mathf.Clamp(y / range, -1, 0) * rect.height;

                _vertices[i] = new Vector3(x, y, 0);
            }

            Handles.DrawAAPolyLine(_vertices);
        }
    }
}
