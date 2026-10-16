namespace MuMuSDK.Desktop.Models
{
    public class AppSettings
    {
        public string MumuInstallDirectory { get; set; } = @"C:\Program Files\Netease\MuMuPlayer";
        public string TargetPackageName { get; set; } = "com.Target.Game";
        public int SelectedTickIntervalMs { get; set; } = 100;
    }
}