using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using JellyfinShellExecutor.Configuration;

namespace JellyfinShellExecutor
{
    /// <summary>
    /// The main plugin class for Shell Executor.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger<Plugin> _logger;
        private readonly string _scriptPath;
        private readonly string _logPath = "/tmp/executor.log";

        /// <inheritdoc />
        public override string Name => "Shell Executor";
        
        /// <inheritdoc />
        public override Guid Id => Guid.Parse("eb5d7894-8eef-4b36-aa7f-5d124e828ce1");
        
        /// <inheritdoc />
        public override string Description => "Execute shell scripts on plugin initialization";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="logger">The logger.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            _logger = logger;
            _scriptPath = Path.Combine(applicationPaths.PluginConfigurationsPath, "JellyfinShellExecutor", "script.sh");
            Instance = this;
            
            // Log the script path for debugging
            _logger.LogInformation("Script path: {Path}", _scriptPath);
            LogToFile($"Script path configured as: {_scriptPath}");
            
            // Ensure plugin directory exists
            var pluginDir = Path.GetDirectoryName(_scriptPath);
            if (!string.IsNullOrEmpty(pluginDir))
            {
                Directory.CreateDirectory(pluginDir);
            }
            
            // Initialize script file if it doesn't exist
            if (!File.Exists(_scriptPath))
            {
                _logger.LogInformation("Script file doesn't exist, creating with default content");
                SaveScript(Configuration.ScriptContent);
            }
            else
            {
                // Load existing script content into configuration
                _logger.LogInformation("Loading existing script from {Path}", _scriptPath);
                try
                {
                    var content = File.ReadAllText(_scriptPath);
                    Configuration.ScriptContent = content;
                    SaveConfiguration();
                    _logger.LogInformation("Loaded script content: {Length} characters", content.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load script content");
                }
            }
            
            // Execute the script on initialization
            ExecuteScript();
        }

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = this.Name,
                    EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
                }
            };
        }

        /// <summary>
        /// Called when configuration is updated.
        /// </summary>
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            base.UpdateConfiguration(configuration);
            
            if (configuration is PluginConfiguration config)
            {
                _logger.LogInformation("Configuration updated, saving script");
                SaveScript(config.ScriptContent);
            }
        }

        /// <summary>
        /// Saves the script content to disk.
        /// </summary>
        /// <param name="content">The script content.</param>
        public void SaveScript(string content)
        {
            try
            {
                _logger.LogInformation("Saving script to {Path}", _scriptPath);
                LogToFile($"Saving script to: {_scriptPath}");
                
                // Ensure directory exists
                var dir = Path.GetDirectoryName(_scriptPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                // Save to file
                File.WriteAllText(_scriptPath, content);
                
                // Make the script executable on Unix-like systems
                if (Environment.OSVersion.Platform == PlatformID.Unix || 
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    var chmod = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x \"{_scriptPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(chmod);
                    process?.WaitForExit();
                }
                
                _logger.LogInformation("Script saved successfully");
                LogToFile("Script saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save script");
                LogToFile($"ERROR saving script: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Logs a message to the executor log file.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void LogToFile(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
                File.AppendAllText(_logPath, logEntry);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }

        /// <summary>
        /// Executes the shell script.
        /// </summary>
        public void ExecuteScript()
        {
            try
            {
                // First ensure the script file exists and is up to date
                if (!File.Exists(_scriptPath))
                {
                    _logger.LogInformation("Script file doesn't exist, creating it");
                    SaveScript(Configuration.ScriptContent);
                }

                if (!File.Exists(_scriptPath))
                {
                    var message = $"Script file not found at {_scriptPath}";
                    _logger.LogWarning(message);
                    LogToFile($"WARNING: {message}");
                    return;
                }

                _logger.LogInformation("Executing script at {Path}", _scriptPath);
                LogToFile($"=== EXECUTING SCRIPT: {_scriptPath} ===");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = _scriptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_scriptPath)
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        _logger.LogInformation("Script output: {Output}", output.Trim());
                        LogToFile($"STDOUT: {output.Trim()}");
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogError("Script error: {Error}", error.Trim());
                        LogToFile($"STDERR: {error.Trim()}");
                    }

                    _logger.LogInformation("Script executed with exit code: {ExitCode}", process.ExitCode);
                    LogToFile($"EXIT CODE: {process.ExitCode}");
                    LogToFile("=== SCRIPT EXECUTION COMPLETED ===");
                }
                else
                {
                    var message = "Failed to start process";
                    _logger.LogError(message);
                    LogToFile($"ERROR: {message}");
                }
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error while executing script");
                LogToFile($"ERROR (IOException): {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access while executing script");
                LogToFile($"ERROR (UnauthorizedAccessException): {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while executing script");
                LogToFile($"ERROR (Exception): {ex.Message}");
            }
            finally
            {
                LogToFile("");  // Add empty line for readability
            }
        }
    }
}