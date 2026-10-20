namespace Affdata.Core.Chart
{
    /// <summary>Shared ordering of Arc and Trace faces; render passes stay with each client.</summary>
    public static class ArcRenderOrder
    {
        public const int RightHalfOffset = 500;

        // project-arc's colour/endpoint-height order in Affdata's 1/100 world units.
        // Use chart-space world height, before timing-group or camera transforms.
        public static int SegmentOrder(int colorId, float worldY)
            => (3 * colorId) + (4 * (int)(worldY * 100f));

        // Preserve Play's tie-breaking keys for segments with the same face order.
        public static int NoteKey(int timingGroup, int timing, int endTiming, int colorId)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + timingGroup;
                hash = hash * 31 + timing;
                hash = hash * 31 + endTiming;
                return hash * 31 + colorId;
            }
        }

        public static int SegmentKey(int noteKey, int segmentIndex, bool isTrace)
        {
            unchecked
            {
                return (noteKey * 31 + segmentIndex) * 31 + (isTrace ? 1 : 0);
            }
        }

        public static int Compare(int orderA, int keyA, int orderB, int keyB)
        {
            int order = orderA.CompareTo(orderB);
            return order != 0 ? order : keyA.CompareTo(keyB);
        }
    }
}
