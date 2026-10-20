// Adapted from AffdataEdit's Arcade.Aff.ArcaeaFileFormat.
// See Third Party Notices.md for the original MIT copyright and permission notice.
#nullable enable
using System.Globalization;
using System.Text;

namespace Affdata.Core.Colors
{
    /// <summary>The hexadecimal color syntax shared by Edit and Play's AFF readers.</summary>
    public static class HexColor
    {
        /// <summary>Parses #RGB, #RGBA, #RRGGBB or #RRGGBBAA. Missing alpha is opaque.</summary>
        public static bool TryParse(string? raw, out Rgba32 color)
        {
            color = default;
            if (raw == null || raw.Length < 4 || raw[0] != '#') return false;
            string hex = raw.Substring(1);
            if (hex.Length == 3 || hex.Length == 4)
            {
                var expanded = new StringBuilder(8);
                foreach (char c in hex) expanded.Append(c).Append(c);
                hex = expanded.ToString();
            }
            if (hex.Length == 6) hex += "FF";
            if (hex.Length != 8 ||
                !uint.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint rgba))
                return false;
            color = Rgba32.FromPacked(rgba);
            return true;
        }

        /// <summary>Writes uppercase #RRGGBB, appending AA only when the color is not opaque.</summary>
        public static string Format(Rgba32 color)
        {
            string rgb = "#" + (color.PackedRgba >> 8).ToString("X6", CultureInfo.InvariantCulture);
            return color.A == 255 ? rgb : rgb + color.A.ToString("X2", CultureInfo.InvariantCulture);
        }
    }
}
