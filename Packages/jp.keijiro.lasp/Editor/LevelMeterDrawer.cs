using UnityEngine;
using UnityEditor;

namespace Lasp.Editor
{
    //
    // A utility class for drawing a level meter in the inspector
    //
    static class LevelMeterDrawer
    {
        static class Styles
        {
            public static Color Background = new Color(0.1f, 0.1f, 0.1f, 1);
            public static Color Gray = new Color(0.3f, 0.3f, 0.3f, 1);
            public static Color Green1 = new Color(0, 0.3f, 0, 1);
            public static Color Green2 = new Color(0, 0.7f, 0, 1);
            public static Color Red = new Color(1, 0, 0, 1);
        }

        // Draw a level meter with a given AudioLevelTracker instance.
        public static void DrawMeter(AudioLevelTracker tracker)
        {
            var rect = GUILayoutUtility.GetRect(128, 10);

            const float kMeterRange = 60;
            var amp  = 1 + tracker.inputLevel  / kMeterRange;
            var peak = 1 - tracker.currentGain / kMeterRange;
            var dr = tracker.dynamicRange / kMeterRange;

            // Background
            DrawRect(0, 0, 1, 1, rect, Styles.Background);

            // Dynamic range indicator
            DrawRect(peak - dr, 0, peak, 1, rect, Styles.Gray);

            // Amplitude bar
            var x1 = Mathf.Min(amp, peak - dr);
            var x2 = Mathf.Min(peak, amp);
            DrawRect( 0, 0,  x1, 1, rect, Styles.Green1); // Lower than floor
            DrawRect(x1, 0,  x2, 1, rect, Styles.Green2); // Inside the range
            DrawRect(x2, 0, amp, 1, rect, Styles.Red); // Higher than nominal

            // Output level bar
            var x3 = peak + dr * (tracker.normalizedLevel - 1);
            DrawRect(x3 - 3 / rect.width, 0, x3, 1, rect, Color.green);

            // Label: -60dB
            var pm60 = new Vector2(rect.xMin + 1, rect.yMax - 8);
            Handles.Label(pm60, "-60dB", EditorStyles.miniLabel);

            // Label: 0dB
            var p0 = new Vector2(rect.xMin + rect.width - 22, rect.yMax - 8);
            Handles.Label(p0, "0dB", EditorStyles.miniLabel);
        }

        // Vertex array for drawing rectangles: Reused to avoid GC allocation.
        static Vector3 [] _rectVertices = new Vector3 [4];

        // Draw a rectangle with normalized coordinates.
        static void DrawRect
          (float x1, float y1, float x2, float y2, Rect area, Color color)
        {
            x1 = area.xMin + area.width  * Mathf.Clamp01(x1);
            x2 = area.xMin + area.width  * Mathf.Clamp01(x2);
            y1 = area.yMin + area.height * Mathf.Clamp01(y1);
            y2 = area.yMin + area.height * Mathf.Clamp01(y2);

            _rectVertices[0] = new Vector2(x1, y1);
            _rectVertices[1] = new Vector2(x1, y2);
            _rectVertices[2] = new Vector2(x2, y2);
            _rectVertices[3] = new Vector2(x2, y1);

            Handles.DrawSolidRectangleWithOutline(_rectVertices, color, Color.clear);
        }
    }
}
