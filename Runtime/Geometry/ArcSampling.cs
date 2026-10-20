using System.Collections.Generic;
using UnityEngine;

namespace Affdata.Core.Geometry
{
    /// <summary>Sample positions from project-arc LogicArcNote::buildSegments.</summary>
    public static class ArcSampling
    {
        // The original note duration selects 14 or 7 samples/second. The render duration
        // includes a gap to connected successors. Accumulate in double, then let the
        // caller cast each progress to float when evaluating its curve/coordinates.
        // Reuse the caller's list; sampling only runs when geometry is rebuilt.
        public static void FillProgress(List<double> progress, int duration, int renderDuration,
            float smoothness, bool straightTrace)
        {
            progress.Clear();
            progress.Add(0);
            if (renderDuration > 0 && !straightTrace
                && !float.IsNaN(smoothness) && !float.IsInfinity(smoothness))
            {
                float rate = (duration < 1000 ? 14f : 7f) * Mathf.Max(1f, smoothness);
                double step = 1.0 / (renderDuration / 1000.0 * rate);
                for (double t = step; t > 0 && t < 1.0; t += step)
                    progress.Add(t);
            }
            // Explicit endpoints also represent zero-duration slams.
            progress.Add(1);
        }
    }
}
