using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace PalWorldPressF
{
    // Persisted user settings, stored as settings.json next to the executable
    // so they survive restarts (issue #6)
    class Settings
    {
        public int PressedKeyCode { get; set; } = (int)Keys.F;
        public int ToggleKeyCode { get; set; } = (int)Keys.J;
        public int SpamIntervalMs { get; set; } = 100;
        public bool CancelOnKeyPress { get; set; } = false;
        public int CancelKeyCode { get; set; } = (int)Keys.Escape;

        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Settings? loaded = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath));
                    if (loaded != null && loaded.IsValid())
                    {
                        return loaded;
                    }
                }
            }
            catch
            {
                // Corrupt or unreadable file: fall back to defaults
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Folder not writable: keep running with in-memory settings only
            }
        }

        private bool IsValid()
        {
            return PressedKeyCode >= (int)Keys.A && PressedKeyCode <= (int)Keys.Z
                && ToggleKeyCode >= (int)Keys.A && ToggleKeyCode <= (int)Keys.Z
                && SpamIntervalMs >= 10 && SpamIntervalMs <= 5000
                && IsValidCancelKey(CancelKeyCode);
        }

        // Keys offered in the cancel-key dropdown; keep both in sync
        public static bool IsValidCancelKey(int keyCode)
        {
            return keyCode == (int)Keys.Escape
                || keyCode == (int)Keys.Tab
                || keyCode == (int)Keys.Space
                || keyCode == (int)Keys.Enter
                || (keyCode >= (int)Keys.A && keyCode <= (int)Keys.Z);
        }
    }
}
