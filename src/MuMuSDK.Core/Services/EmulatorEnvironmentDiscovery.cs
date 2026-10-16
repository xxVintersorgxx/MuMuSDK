using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MuMuSDK.Core.Models;

namespace MuMuSDK.Core.Services
{
    public static class EmulatorEnvironmentDiscovery
    {
        public static async Task<EmulatorEnvironmentModel> ScanDirectoryAsync(string installDirectory)
        {
            if (string.IsNullOrWhiteSpace(installDirectory) || !Directory.Exists(installDirectory))
            {
                return EmulatorEnvironmentModel.CreateEmpty();
            }

            return await Task.Run(() =>
            {
                try
                {
                    // 1. Locate the native binary helper utility
                    string foundCliPath = string.Empty;
                    string defaultTarget = Path.Combine(installDirectory, "mumu-cli.exe");

                    if (File.Exists(defaultTarget))
                    {
                        foundCliPath = defaultTarget;
                    }
                    else
                    {
                        string[] fileMatches = Directory.GetFiles(installDirectory, "mumu-cli.exe", SearchOption.AllDirectories);
                        if (fileMatches.Length > 0) foundCliPath = fileMatches[0];
                    }

                    if (string.IsNullOrEmpty(foundCliPath)) return EmulatorEnvironmentModel.CreateEmpty();

                    // 2. Query Instance Information Map
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = foundCliPath,
                        Arguments = "info -v all",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    string infoJsonText = string.Empty;
                    using (var cliProcess = Process.Start(startInfo))
                    {
                        if (cliProcess is not null)
                        {
                            infoJsonText = cliProcess.StandardOutput.ReadToEnd();
                            cliProcess.WaitForExit();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(infoJsonText)) return EmulatorEnvironmentModel.CreateEmpty();

                    var model = new EmulatorEnvironmentModel(false, "Pending Analysis", foundCliPath, "Parsing data streams...");

                    // 3. Defensive Extraction Phase A: Instance Properties
                    using (var jsonDoc = JsonDocument.Parse(infoJsonText))
                    {
                        var root = jsonDoc.RootElement;

                        if (root.TryGetProperty("0", out var playerZeroNode))
                        {
                            // GUARD: Check if the player process has actually initialized
                            bool isProcessStarted = playerZeroNode.TryGetProperty("is_process_started", out var procStartedProp) && procStartedProp.GetBoolean();
                            bool isAndroidStarted = playerZeroNode.TryGetProperty("is_android_started", out var androidStartedProp) && androidStartedProp.GetBoolean();

                            // Fail-Fast: If the player isn't actively running, abort instantly to avoid false positives
                            if (!isProcessStarted || !isAndroidStarted)
                            {
                                return EmulatorEnvironmentModel.CreateEmpty();
                            }

                            // Strictly extract properties only if the instance is verified active
                            if (playerZeroNode.TryGetProperty("pid", out var pidProp))
                            {
                                model.DynamicProcessTargetKey = pidProp.GetInt32().ToString();
                            }
                            if (playerZeroNode.TryGetProperty("adb_port", out var portProp))
                            {
                                model.AdbHostPort = portProp.GetInt32();
                            }
                            if (playerZeroNode.TryGetProperty("index", out var indexProp) && int.TryParse(indexProp.GetString(), out int parsedIndex))
                            {
                                model.ActivePlayerIndex = parsedIndex;
                            }
                        }
                    }

                    // Strict Guard: Abort if target parameters remain unassigned or unverified
                    if (string.IsNullOrEmpty(model.DynamicProcessTargetKey) || model.DynamicProcessTargetKey == "0")
                    {
                        return EmulatorEnvironmentModel.CreateEmpty();
                    }

                    // 4. Query System Settings Map
                    startInfo.Arguments = "setting -a -v 0";
                    string settingsJsonText = string.Empty;
                    using (var cliProcess = Process.Start(startInfo))
                    {
                        if (cliProcess is not null)
                        {
                            settingsJsonText = cliProcess.StandardOutput.ReadToEnd();
                            cliProcess.WaitForExit();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(settingsJsonText)) return EmulatorEnvironmentModel.CreateEmpty();

                    // 5. Strict Extraction Phase B: Memory Properties
                    using (var settingsDoc = JsonDocument.Parse(settingsJsonText))
                    {
                        var settingsRoot = settingsDoc.RootElement;

                        if (settingsRoot.TryGetProperty("vm_mem", out var memProp) &&
                            long.TryParse(memProp.GetString(), out long ramGb))
                        {
                            model.LowerTargetMemorySize = 1677721600;
                            model.UpperTargetMemorySize = ramGb * 1024 * 1024 * 1024;
                        }

                        if (settingsRoot.TryGetProperty("remote_target_uuid", out var uuidProp))
                        {
                            model.MachineUuid = uuidProp.GetString() ?? string.Empty;
                        }
                    }

                    if (model.LowerTargetMemorySize > 0 && model.UpperTargetMemorySize > 0)
                    {
                        model.IsValid = true;
                        model.NemuConfigPath = "CLI Verified Stream";
                        model.ExtractedCliSettings = "Instance parameters securely locked.";
                        return model;
                    }

                    return EmulatorEnvironmentModel.CreateEmpty();
                }
                catch
                {
                    return EmulatorEnvironmentModel.CreateEmpty();
                }
            });
        }
    }
}
