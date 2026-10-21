namespace Affdata.Core.Chart
{
    /// <summary>The timing fields used to generate gameplay beat lines.</summary>
    public readonly struct BeatLineTiming
    {
        public readonly int Timing;
        public readonly float Bpm;
        public readonly float Divisor;

        public BeatLineTiming(int timing, float bpm, float divisor)
        {
            Timing = timing;
            Bpm = bpm;
            Divisor = divisor;
        }
    }
}
