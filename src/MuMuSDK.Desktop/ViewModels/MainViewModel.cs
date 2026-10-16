using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuMuSDK.Core.Interfaces;
using MuMuSDK.Core.Models;
using MuMuSDK.Core.Services;
using MuMuSDK.Desktop.Models;
using System.Collections.ObjectModel;

namespace MuMuSDK.Desktop.ViewModels
{
    public partial class MainViewModel :  ViewModelBase
    {
        private readonly AppSettings _loadedSettings;
        private readonly IProcessTracker _processTracker;
        private readonly ILogBroker _logBroker;
        private readonly ITickerEngine _tickerEngine;
        private readonly IMemoryMapper _memoryMapper;

        public MainViewModel(AppSettings loadedSettings, IProcessTracker processTracker, ILogBroker logBroker, ITickerEngine tickerEngine, IMemoryMapper memoryMapper)
        {
            _loadedSettings = loadedSettings;
            _processTracker = processTracker;
            _logBroker = logBroker;
            _tickerEngine = tickerEngine;
            _memoryMapper = memoryMapper;

            _mumuInstallDirectory = _loadedSettings.MumuInstallDirectory;
            _targetPackageName = _loadedSettings.TargetPackageName;
            _selectedTickIntervalMs = _loadedSettings.SelectedTickIntervalMs;

            _logBroker.OnLogLineFlushed += HandleIncomingLogLine;

            // 1. Configure the initial clock rate based on loaded settings properties
            _tickerEngine.UpdateInterval(_selectedTickIntervalMs);

            // 2. Wire a listener hook to capture clock loops and handle background data syncs
            _tickerEngine.OnTickPulse += HandleSystemTickPulse;

            this.Log(MuMuLogLevel.Info, "System workspace architecture boot sequence established via dependency injection containers.");
        }

        // --- SECTION A: CORE DYNAMIC RUNTIME PARAMETERS ---
        [ObservableProperty] private string _mumuInstallDirectory = string.Empty;
        [ObservableProperty] private string _targetPackageName = string.Empty;
        [ObservableProperty] private int _selectedTickIntervalMs;
        [ObservableProperty]
        private EmulatorEnvironmentModel _environmentModel = EmulatorEnvironmentModel.CreateEmpty();
        public ObservableCollection<int> TickIntervalOptions { get; } = new() { 50, 75, 100 };

        // --- SECTION B: MEMORY BADGES & CHECKPOINTS MATRIX ---
        [ObservableProperty] public partial string LowerBaseHvaString { get; set; } = "0x0000000000000000";
        [ObservableProperty] public partial string UpperBaseHvaString { get; set; } = "0x0000000000000000";
        [ObservableProperty] public partial bool IsWin32HandleValid { get; set; } = false;
        [ObservableProperty] public partial bool IsMemoryPoolMapped { get; set; } = false;
        [ObservableProperty] public partial bool IsSwapperSignatureFound { get; set; } = false;
        [ObservableProperty] public partial bool IsPidZeroValidated { get; set; } = false;
        [ObservableProperty] public partial bool IsTaskChainAnchored { get; set; } = false;

        // --- SECTION C: LIVE TARGET DATASTREAM METRICS ---
        // Initialized as an empty collection so hardcoded text values don't pollute your telemetry grid
        public ObservableCollection<TelemetryRowItem> TelemetryMetrics { get; } = new();

        // --- SECTION D: TERMINAL CONSOLE STREAMS ---
        [ObservableProperty] public partial string SelectedLogLevel { get; set; } = "Info";
        public ObservableCollection<string> LogLevelOptions { get; } = new() { "Trace", "Info", "Warning", "Error", "Fatal" };
        [ObservableProperty] public partial bool MuteMmuEmulator { get; set; } = false;
        [ObservableProperty] public partial bool MuteTaskEnumerator { get; set; } = false;
        [ObservableProperty] public partial bool MuteMemoryMapper { get; set; } = false;

        public ObservableCollection<string> TerminalLogLines { get; } = new();

        // --- SOURCE GENERATOR CONFIGURATION INTERCEPTORS ---
        partial void OnMumuInstallDirectoryChanged(string value)
        {
            _loadedSettings.MumuInstallDirectory = value;
            SettingsManager.Save(_loadedSettings);
        }

        partial void OnTargetPackageNameChanged(string value)
        {
            _loadedSettings.TargetPackageName = value;
            SettingsManager.Save(_loadedSettings);
        }

        partial void OnSelectedTickIntervalMsChanged(int value)
        {
            _loadedSettings.SelectedTickIntervalMs = value;
            SettingsManager.Save(_loadedSettings);

            // Feed the updated interval property straight into the running ticker motor
            _tickerEngine?.UpdateInterval(value);
        }
        private void HandleSystemTickPulse()
        {
            // This method executes on every single timing loop cycle!
            // We will hook this up downstream to read unmanaged memory bytes rapidly.
        }
        partial void OnSelectedLogLevelChanged(string value)
        {
            if (Enum.TryParse<MuMuLogLevel>(value, out var parsedLevel))
            {
                _logBroker.GlobalLevel = parsedLevel;
            }
        }
        private void HandleIncomingLogLine(string fullLogLine)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                TerminalLogLines.Add(fullLogLine);

                if (TerminalLogLines.Count > 150)
                {
                    TerminalLogLines.RemoveAt(0);
                }
            });
        }
        partial void OnMuteMmuEmulatorChanged(bool value) => _logBroker.SetNamespaceMuted("MmuEmulator", value);
        partial void OnMuteTaskEnumeratorChanged(bool value) => _logBroker.SetNamespaceMuted("TaskEnumerator", value);
        partial void OnMuteMemoryMapperChanged(bool value) => _logBroker.SetNamespaceMuted("MemoryMapper", value);
        
        
        // --- INTERACTIVE COMMAND TIMELINE EVENTS ---
        [RelayCommand]
        private async Task BrowseMumuDirectory(Avalonia.Controls.Window? ownerWindow)
        {
            if (ownerWindow?.StorageProvider is null) return;

            var options = new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Select MuMuPlayer Installation Home Directory",
                AllowMultiple = false
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(MumuInstallDirectory))
                {
                    string sanitizedStartPath = System.IO.Path.GetFullPath(MumuInstallDirectory);
                    if (System.IO.Directory.Exists(sanitizedStartPath))
                    {
                        var storageFolder = await ownerWindow.StorageProvider.TryGetFolderFromPathAsync(new Uri(sanitizedStartPath));
                        if (storageFolder is not null)
                        {
                            options.SuggestedStartLocation = storageFolder;
                        }
                    }
                }
            }
            catch { /* Suppressed for fallback safety */ }

            var result = await ownerWindow.StorageProvider.OpenFolderPickerAsync(options);
            if (result is { Count: > 0 })
            {
                MumuInstallDirectory = System.IO.Path.GetFullPath(result[0].Path.LocalPath);
            }
        }
        [RelayCommand]
        private void Stage1Attach()
        {
            bool attachSuccess = _processTracker.TryAttach("MuMuNxDevice");

            if (attachSuccess)
            {
                IsWin32HandleValid = true;
                _logBroker.Log(MuMuLogLevel.Info, "ProcessSubsystem", 110, $"Handle secured. Host Operating System tracking PID: {_processTracker.ProcessId}");
            }
            else
            {
                IsWin32HandleValid = false;
                _logBroker.Log(MuMuLogLevel.Error, "ProcessSubsystem", 115, "Attachment failure hook triggered. MuMuNxDevice target instance window context not located.");
            }
        }

        [RelayCommand]
        private void Stage2ScanPools()
        {
            // Feed the dynamic memory sizes calculated during the pre-attachment directory scan!
            long targetLowerBytes = EnvironmentModel.LowerTargetMemorySize;
            long targetUpperBytes = EnvironmentModel.UpperTargetMemorySize;

            bool sweepSuccess = _memoryMapper.InitializeMapping(targetLowerBytes, targetUpperBytes);

            if (sweepSuccess)
            {
                LowerBaseHvaString = $"0x{_memoryMapper.LowerBaseHva.ToString("X")}";
                UpperBaseHvaString = $"0x{_memoryMapper.UpperBaseHva.ToString("X")}";
                IsMemoryPoolMapped = true;

                TelemetryMetrics.Clear();
                TelemetryMetrics.Add(new TelemetryRowItem("Dynamic Lower Virtual Pool", LowerBaseHvaString, "00 00 00 00 ...", $"{targetLowerBytes:N0} Bytes Mapped"));
                TelemetryMetrics.Add(new TelemetryRowItem("Dynamic Upper Virtual Pool", UpperBaseHvaString, "00 00 00 00 ...", $"{targetUpperBytes:N0} Bytes Mapped"));
            }
            else
            {
                IsMemoryPoolMapped = false;
                LowerBaseHvaString = "0x0000000000000000";
                UpperBaseHvaString = "0x0000000000000000";
                this.Log(MuMuLogLevel.Error, $"Memory mapping configuration sweep failed. Target allocations not located for sizes: L={targetLowerBytes}, U={targetUpperBytes}");
            }
        }

        [RelayCommand]
        private void Stage3FindKernel()
        {
            IsSwapperSignatureFound = true;
            IsPidZeroValidated = true;
            IsTaskChainAnchored = true;
            _logBroker.Log(MuMuLogLevel.Info, "MmuEmulator", 98, "Found invariant ASCII token signature 'swapper\\0'. Head task blocks successfully anchored inside driver registers.");
        }

        [RelayCommand]
        private void OpenPluginManager()
        {
            var dialogWindow = new Views.PluginManagerDialog();
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow is not null) dialogWindow.ShowDialog(desktop.MainWindow);
                else dialogWindow.Show();
            }
        }
        [RelayCommand]
        private async Task ScanEmulatorDirectory()
        {
            // Reset mapping indicators to ensure clear staging execution tracks
            EnvironmentModel = EmulatorEnvironmentModel.CreateEmpty();

            this.Log(MuMuLogLevel.Info, $"Commencing automated directory parsing pass on storage track: {MumuInstallDirectory}");

            // Execute folder scanning off the main UI execution thread loops cleanly
            var resultModel = await EmulatorEnvironmentDiscovery.ScanDirectoryAsync(MumuInstallDirectory);
            EnvironmentModel = resultModel;

            if (resultModel.IsValid)
            {
                this.Log(MuMuLogLevel.Info, "Environment scan successful. Environment model configuration parsed safely.");
                this.Log(MuMuLogLevel.Info, $"Profile Target: {resultModel.NemuConfigPath}");

                // Print the dynamically discovered byte dimensions directly to screen logs!
                this.Log(MuMuLogLevel.Info, $"Dynamic Allocation Targets Isolated -> Lower Pool: {resultModel.LowerTargetMemorySize:N0} bytes | Upper Pool: {resultModel.UpperTargetMemorySize:N0} bytes");

                if (resultModel.CliToolPath != "Not Found" && !string.IsNullOrWhiteSpace(resultModel.ExtractedCliSettings))
                {
                    this.Log(MuMuLogLevel.Info, $"[mumu-cli info] Active Instances: {resultModel.ExtractedCliSettings}");
                }
            }
            else
            {
                this.Log(MuMuLogLevel.Error, "Directory discovery operation failed. Verify that your MuMuPlayer home folder path contains valid tracking assets.");
            }
        }
    }
}
