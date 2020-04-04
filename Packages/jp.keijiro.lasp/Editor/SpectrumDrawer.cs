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

        public static void DrawGraph(System.ReadOnlySpan<float> spectrum)
        {
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
