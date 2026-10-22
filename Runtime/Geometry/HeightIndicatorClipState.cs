using UnityEngine;

namespace Affdata.Core.Geometry
{
    /// <summary>Visibility and position of the indicator owned by an Arc's first segment.</summary>
    public struct HeightIndicatorClipState
    {
        private bool retired;
        private bool hasClipped;
        private int lastTiming;

        // Positions are in untransformed playfield units (1/100 of project-arc).
        // The caller supplies the Arc's current judge-line point and resets on rebuild/seek.
        public bool TryGetPosition(int timing, bool pastStart, bool clipping, float velocity,
            Vector3 start, float endZ, Vector3 judgementPoint, out Vector3 position)
        {
            if (timing < lastTiming)
            {
                retired = false;
                hasClipped = false;
            }
            lastTiming = timing;
            hasClipped |= clipping;
            clipping = hasClipped;

            // RenderArcNote::update also retires an unheld first segment past the camera.
            retired |= pastStart && velocity > 0 && endZ > 7f;

            if (clipping)
            {
                // RenderArcNote::clipPassedSegments: limit = 10 * sign in reference units.
                float sign = velocity >= 0 ? 1 : -1;
                retired |= endZ * sign >= 0.1f * sign;
            }

            position = clipping ? new Vector3(judgementPoint.x, judgementPoint.y, 0) : start;
            return !retired;
        }
    }
}
