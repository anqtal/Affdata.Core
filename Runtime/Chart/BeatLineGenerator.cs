using System;
using System.Collections.Generic;

namespace Affdata.Core.Chart
{
    /// <summary>Gameplay beat-line times, preserving AffdataPlay's generation rules.</summary>
    public static class BeatLineGenerator
    {
        // Input events must be sorted by timing. Non-final intervals exclude the next
        // event; the final interval includes audioLength. No extra pre-roll is added.
        // Returned times use Math.Round (midpoints to even), as Play did before sharing.
        public static IEnumerable<int> Generate(IReadOnlyList<BeatLineTiming> timingEvents, int audioLength)
        {
            for (int i = 0; i < timingEvents.Count; i++)
            {
                BeatLineTiming timingEvent = timingEvents[i];
                bool last = i == timingEvents.Count - 1;
                int endTiming = last ? audioLength : timingEvents[i + 1].Timing;
                if (!TryGetBeatInterval(timingEvent, out double beatIntervalMs)) continue;

                int beatIndex = 0;
                double beatLineTiming = timingEvent.Timing;
                while (last ? beatLineTiming <= endTiming : beatLineTiming < endTiming)
                {
                    yield return (int)Math.Round(beatLineTiming);
                    beatIndex++;
                    beatLineTiming = timingEvent.Timing + beatIntervalMs * beatIndex;
                }
            }
        }

        private static bool TryGetBeatInterval(BeatLineTiming timingEvent, out double beatIntervalMs)
        {
            // Zero divisor skips the whole interval, including its starting beat line.
            // Keep Play's positive-BPM cutoff; negative BPM uses its absolute value below.
            if (timingEvent.Bpm > 10000 || timingEvent.Divisor <= 0)
            {
                beatIntervalMs = 0;
                return false;
            }

            // Preserve the float interval calculation before widening to double.
            // Zero BPM with positive divisor produces only the interval's starting line.
            double calculatedIntervalMs = timingEvent.Bpm == 0
                ? double.MaxValue
                : 60000f / Math.Abs(timingEvent.Bpm) * timingEvent.Divisor;
            beatIntervalMs = Math.Max(calculatedIntervalMs, 1);
            return true;
        }
    }
}
