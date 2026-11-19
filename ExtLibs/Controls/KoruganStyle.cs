using System;
using System.Collections.Generic;
using System.Drawing;

namespace MissionPlanner.Controls
{
    /// <summary>
    /// Korugan look (black, white and FED-STD-595 sand), matching Korugan CAD's "korugan" theme.
    /// Hard edges, hairline rules, uppercase labels. The custom-drawn controls read this when
    /// the Korugan theme is active; ThemeManager switches it on and loads the fonts.
    /// </summary>
    public static class KoruganStyle
    {
        /// <summary>True while the Korugan theme is the active theme.</summary>
        public static bool Enabled;

        // fonts (null until loaded; every user falls back to the control's own font)
        public static FontFamily UiFamily;      // Barlow Medium   (body text)
        public static FontFamily UiSemiFamily;  // Barlow SemiBold (buttons, tabs, labels)
        public static FontFamily UiBoldFamily;  // Barlow          (bold only)
        public static FontFamily NumFamily;     // IBM Plex Mono Medium (numbers)

        // palette (FS 33531 Sand #C7B49E, FS 30277 Sand #9E8A75)
        public static readonly Color Sand = Color.FromArgb(0xC7, 0xB4, 0x9E);
        public static readonly Color SandDark = Color.FromArgb(0x9E, 0x8A, 0x75);
        public static readonly Color SandLight = Color.FromArgb(0xE6, 0xDC, 0xCD);
        public static readonly Color Chrome = Color.FromArgb(0x0B, 0x0B, 0x0B);
        public static readonly Color Panel = Color.FromArgb(0x0E, 0x0E, 0x0E);
        public static readonly Color Hover = Color.FromArgb(0x16, 0x16, 0x16);
        public static readonly Color Sel = Color.FromArgb(0x26, 0x21, 0x19);
        public static readonly Color Line = Color.FromArgb(0x26, 0x26, 0x26);
        public static readonly Color Ink = Color.FromArgb(0xF2, 0xF2, 0xF2);
        public static readonly Color Muted = Color.FromArgb(0x8F, 0x8F, 0x8F);

        private static readonly Dictionary<string, Font> cache = new Dictionary<string, Font>();

        /// <summary>Body text font (Barlow Medium; bold maps to Barlow Bold).</summary>
        public static Font Ui(Font basis)
        {
            if (basis == null) return null;
            if ((basis.Style & FontStyle.Bold) != 0)
                return Get(UiBoldFamily, basis, basis.Style) ?? basis;
            return Get(UiFamily, basis, basis.Style) ?? basis;
        }

        /// <summary>Label / button font (Barlow SemiBold).</summary>
        public static Font Semi(Font basis)
        {
            if (basis == null) return null;
            return Get(UiSemiFamily, basis, basis.Style & ~FontStyle.Bold) ?? Ui(basis);
        }

        public static Font Semi(float size)
        {
            return Get(UiSemiFamily, size, GraphicsUnit.Point, FontStyle.Regular);
        }

        /// <summary>Number font (IBM Plex Mono Medium).</summary>
        public static Font Num(float size, GraphicsUnit unit)
        {
            return Get(NumFamily, size, unit, FontStyle.Regular);
        }

        private static Font Get(FontFamily fam, Font basis, FontStyle style)
        {
            return Get(fam, basis.Size, basis.Unit, style);
        }

        private static Font Get(FontFamily fam, float size, GraphicsUnit unit, FontStyle style)
        {
            if (fam == null || size <= 0) return null;
            try
            {
                if (!fam.IsStyleAvailable(style))
                {
                    style &= ~(FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout);
                    if (!fam.IsStyleAvailable(style))
                        style = fam.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold;
                }

                var key = fam.Name + "|" + size + "|" + unit + "|" + (int)style;
                lock (cache)
                {
                    Font f;
                    if (!cache.TryGetValue(key, out f))
                    {
                        f = new Font(fam, size, style, unit);
                        cache[key] = f;
                    }
                    return f;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
