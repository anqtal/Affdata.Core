using UnityEngine;

namespace Affdata.Core.Chart
{
    /// <summary>project-arc's high-color tint and first-segment opacity for height indicators.</summary>
    public static class HeightIndicatorStyle
    {
        // endZ is the first segment's end in untransformed playfield units (1/100 of project-arc).
        // The caller supplies Arc opacity before distance fade, and applies timing-group color once.
        public static Color GetColor(Color highColor, float arcOpacity, float endZ,
            bool isZeroDuration, float nearLineFade = 1f)
        {
            // Normal arcs fade from 225 to 75 over distances 85..95; zero-duration arcs use 80..90.
            float farDistance = isZeroDuration ? 90f : 95f;
            float distanceFade = Mathf.Clamp01((endZ + farDistance) / 10f);
            int segmentOpacity = (int)((75f + 150f * distanceFade) * Mathf.Clamp01(nearLineFade));
            int parentOpacity = (int)(Mathf.Clamp01(arcOpacity) * 255f);

            // Cocos truncates each node's opacity to a byte, including the cascaded result.
            highColor.a *= (parentOpacity * segmentOpacity / 255) / 255f;
            return highColor;
        }
    }
}
