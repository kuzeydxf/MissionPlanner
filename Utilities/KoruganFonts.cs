#if !LIB
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using MissionPlanner.Controls;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Loads the Korugan theme's fonts (Barlow, IBM Plex Mono) from Korugan\fonts next to the exe.
    /// They are registered privately for this process only; nothing is installed on the PC.
    /// </summary>
    public static class KoruganFonts
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const uint FR_PRIVATE = 0x10;

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        // file -> family name as GDI/GDI+ groups it (name ID 1 of each font)
        private static readonly Dictionary<string, string> Files = new Dictionary<string, string>
        {
            { "Barlow-Medium.ttf", "Barlow Medium" },
            { "Barlow-SemiBold.ttf", "Barlow SemiBold" },
            { "Barlow-Bold.ttf", "Barlow" },
            { "IBMPlexMono-Medium.ttf", "IBM Plex Mono Medium" },
        };

        private static PrivateFontCollection collection;
        private static bool loaded;

        public static void Load()
        {
            if (loaded)
                return;
            loaded = true;

            try
            {
                var dir = Path.Combine(Settings.GetRunningDirectory(), "Korugan", "fonts");
                if (!Directory.Exists(dir))
                {
                    log.Warn("Korugan fonts folder missing: " + dir);
                    return;
                }

                collection = new PrivateFontCollection();

                foreach (var pair in Files)
                {
                    var file = Path.Combine(dir, pair.Key);
                    if (!File.Exists(file))
                        continue;

                    try
                    {
                        // GDI (TextRenderer, standard labels) needs the font registered with the process
                        AddFontResourceEx(file, FR_PRIVATE, IntPtr.Zero);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    collection.AddFontFile(file);
                    // Skia-drawn controls (QuickView) load the file directly
                    global::System.Extension.PrivateFontFiles[pair.Value] = file;
                }

                foreach (var fam in collection.Families)
                {
                    switch (fam.Name)
                    {
                        case "Barlow Medium":
                            KoruganStyle.UiFamily = fam;
                            break;
                        case "Barlow SemiBold":
                            KoruganStyle.UiSemiFamily = fam;
                            break;
                        case "Barlow":
                            KoruganStyle.UiBoldFamily = fam;
                            break;
                        case "IBM Plex Mono Medium":
                            KoruganStyle.NumFamily = fam;
                            break;
                    }
                }

                // never leave bold without a face
                if (KoruganStyle.UiBoldFamily == null)
                    KoruganStyle.UiBoldFamily = KoruganStyle.UiSemiFamily ?? KoruganStyle.UiFamily;
                if (KoruganStyle.UiSemiFamily == null)
                    KoruganStyle.UiSemiFamily = KoruganStyle.UiFamily;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
#endif
