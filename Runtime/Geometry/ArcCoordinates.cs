// Adapted from AffdataEdit's Arcade.Gameplay.ArcAlgorithm.
// See Third Party Notices.md for the original MIT copyright and permission notice.
namespace Affdata.Core.Geometry
{
    /// <summary>Conversions between AFF coordinates and the shared gameplay world scale.</summary>
    public static class ArcCoordinates
    {
        public const float LaneWidth = 4.25f;
        public const float ArcY0 = 1f;
        public const float ArcY1 = 5.5f;

        public static float ArcXToWorld(float x)
        {
            return (-LaneWidth * 2 * x) + LaneWidth;
        }

        public static float ArcYToWorld(float y)
        {
            return ArcY0 + ((ArcY1 - ArcY0) * y);
        }

        public static float WorldXToArc(float x)
        {
            return (x - LaneWidth) / (-LaneWidth * 2);
        }

        public static float WorldYToArc(float y)
        {
            return (y - ArcY0) / (ArcY1 - ArcY0);
        }
    }
}
