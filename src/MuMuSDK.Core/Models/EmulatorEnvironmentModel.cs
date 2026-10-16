namespace MuMuSDK.Core.Models
{
    /// <summary>
    /// Holds immutable metadata primitives audited from the target player installation space.
    /// </summary>
    public sealed class EmulatorEnvironmentModel
    {
        // --- BASELINE TRACKING STATES ---
        public bool IsValid { get; set; }
        public string NemuConfigPath { get; set; } = string.Empty;
        public string CliToolPath { get; set; } = string.Empty;
        public string ExtractedCliSettings { get; set; } = string.Empty;

        // --- CONFIGURATION-DRIVEN PROCESS SELECTION MARKS ---
        public string DynamicProcessTargetKey { get; set; } = string.Empty; // No default text assumptions

        // --- SYSTEM LAYER CONFIGURATIONS ---
        public int ActivePlayerIndex { get; set; } = -1; // -1 marks an uninitialized index block
        public int AdbHostPort { get; set; } = 0;
        public string MachineUuid { get; set; } = string.Empty;
        public string GlobalInstanceId { get; set; } = string.Empty;

        // --- PHASE MODULE MEMORY ARENA BOUNDS ---
        // Defaults to zero to force immediate failure messages if extraction fails
        public long LowerTargetMemorySize { get; set; } = 0;
        public long UpperTargetMemorySize { get; set; } = 0;

        public EmulatorEnvironmentModel(bool isValid, string nemuConfigPath, string cliToolPath, string extractedCliSettings)
        {
            IsValid = isValid;
            NemuConfigPath = nemuConfigPath;
            CliToolPath = cliToolPath;
            ExtractedCliSettings = extractedCliSettings;
        }

        /// <summary>
        /// Instantiates an invalid fallback representation when an initialization pass fails.
        /// </summary>
        public static EmulatorEnvironmentModel CreateEmpty() =>
            new(false, "Error: Unresolved", "Error: Unresolved", "Error: Process Aborted");
    }
}
