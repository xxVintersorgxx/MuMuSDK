using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MuMuSDK.Desktop.ViewModels
{
    public partial class PluginManagerViewModel : ViewModelBase
    {
        public ObservableCollection<PluginDisplayItem> DiscoveredPlugins { get; } = new()
        {
            new PluginDisplayItem("MuMuSDK.Plugins.Game.Genshin", "com.miHoYo.Genshin", "v31.4.2-Stable", "🟩 Loaded & Ready"),
            new PluginDisplayItem("MuMuSDK.Plugins.Game.Honkai", "com.miHoYo.hkrpg", "v31.1.0-Mismatched", "🟥 Blocked (Update Required)")
        };

        [RelayCommand]
        private void RefreshPluginFolder()
        {
            // Will re-invoke reflection scanning routines on-demand downstream
        }
    }

    public class PluginDisplayItem
    {
        public string AssemblyIdentity { get; set; }
        public string TargetProcessComm { get; set; }
        public string VersionSignature { get; set; }
        public string DeploymentStatus { get; set; }

        public PluginDisplayItem(string asm, string comm, string ver, string status)
        {
            AssemblyIdentity = asm;
            TargetProcessComm = comm;
            VersionSignature = ver;
            DeploymentStatus = status;
        }
    }
}
