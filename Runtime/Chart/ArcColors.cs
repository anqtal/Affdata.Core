using Affdata.Core.Colors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Affdata.Core.Chart
{
    /// <summary>Arc color IDs, default palette and fallback shared by Edit and Play.</summary>
    public static class ArcColors
    {
        public const int BlueId = 0;
        public const int RedId = 1;
        public const int GreenId = 2;
        public const int GrayId = 3;
        public const int BlackId = 4;

        // Black is the fallback for IDs outside the four-entry palette, including BlackId.
        public static readonly Color Fallback = Color.black;
        public static readonly IReadOnlyList<Color> NormalHigh = Array.AsReadOnly(new Color[]
        {
            new Color32(0x0C, 0xD4, 0xD4, 0xFF),
            new Color32(0xFF, 0x96, 0xDC, 0xFF),
            new Color32(0x23, 0xFF, 0x6C, 0xFF),
            new Color32(0xDC, 0xDC, 0xDC, 0xFF),
        });
        public static readonly IReadOnlyList<Color> NormalLow = Array.AsReadOnly(new Color[]
        {
            new Color32(0x19, 0xA0, 0xEB, 0xFF),
            new Color32(0xF0, 0x69, 0x9B, 0xFF),
            new Color32(0x28, 0xC8, 0x1E, 0xFF),
            new Color32(0xAA, 0xAA, 0xAA, 0xFF),
        });

        public static (Color high, Color low) GetColor(int colorId,
            IReadOnlyList<Color> highColors, IReadOnlyList<Color> lowColors)
        {
            if ((uint)colorId >= (uint)highColors.Count || (uint)colorId >= (uint)lowColors.Count)
                return (Fallback, Fallback);
            return (highColors[colorId], lowColors[colorId]);
        }

        public static Rgba32 GoldTrace => new Rgba32(0xF4, 0xB9, 0x42);
    }
}
