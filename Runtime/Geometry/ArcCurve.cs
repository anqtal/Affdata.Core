// Adapted from AffdataEdit's Arcade.Gameplay.ArcAlgorithm.
// See Third Party Notices.md for the original MIT copyright and permission notice.
using System;

namespace Affdata.Core.Geometry
{
    /// <summary>
    /// AFF curve interpolation and cubic camera easing. Progress is not clamped.
    /// </summary>
    public static class ArcCurve
    {
        public static float S(float start, float end, float t)
        {
            return ((1 - t) * start) + (end * t);
        }

        public static float O(float start, float end, float t)
        {
            return start + ((end - start) * (1 - (float)Math.Cos(1.5707963f * t)));
        }

        public static float I(float start, float end, float t)
        {
            return start + ((end - start) * (float)Math.Sin(1.5707963f * t));
        }

        public static float B(float start, float end, float t)
        {
            // Preserve the original float intermediates and evaluation order.
            float o = 1 - t;
            return ((float)Math.Pow(o, 3) * start)
                 + (3 * (float)Math.Pow(o, 2) * t * start)
                 + (3 * o * (float)Math.Pow(t, 2) * end)
                 + ((float)Math.Pow(t, 3) * end);
        }

        public static float X(float start, float end, float t, ArcCurveType type)
        {
            switch (type)
            {
                case ArcCurveType.B:
                    return B(start, end, t);
                case ArcCurveType.Si:
                case ArcCurveType.SiSi:
                case ArcCurveType.SiSo:
                    return I(start, end, t);
                case ArcCurveType.So:
                case ArcCurveType.SoSi:
                case ArcCurveType.SoSo:
                    return O(start, end, t);
                default:
                    return S(start, end, t);
            }
        }

        public static float Y(float start, float end, float t, ArcCurveType type)
        {
            switch (type)
            {
                case ArcCurveType.B:
                    return B(start, end, t);
                case ArcCurveType.SiSi:
                case ArcCurveType.SoSi:
                    return I(start, end, t);
                case ArcCurveType.SiSo:
                case ArcCurveType.SoSo:
                    return O(start, end, t);
                default:
                    return S(start, end, t);
            }
        }

        public static float Qi(float value)
        {
            return value * value * value;
        }

        public static float Qo(float value)
        {
            value--;
            return (value * value * value) + 1;
        }
    }
}
