using MuMuSDK.Desktop.Models;
using System.Text.Json;

public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                var defaultSettings = new AppSettings();
                Save(defaultSettings);
                return defaultSettings;
            }

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

            if (!string.IsNullOrWhiteSpace(settings.MumuInstallDirectory))
            {
                // Pass the raw text directly into a DirectoryInfo object tracker
                // This forces the OS to evaluate the absolute path hierarchy and strip literal duplicate separators
                var dirInfo = new System.IO.DirectoryInfo(settings.MumuInstallDirectory);

                // FullName returns the absolute single-backslash operating system path representation
                settings.MumuInstallDirectory = dirInfo.FullName;
            }

            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Suppressed for stability
        }
    }
}