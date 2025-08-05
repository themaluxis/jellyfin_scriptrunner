using MediaBrowser.Model.Plugins;

namespace JellyfinShellExecutor.Configuration
{
    /// <summary>
    /// Plugin configuration class.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        private string _scriptContent = @"#!/bin/bash
# Default script content
echo ""Shell Executor Plugin initialized at $(date)""
";

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the script content.
        /// </summary>
        public string ScriptContent
        {
            get => _scriptContent;
            set => _scriptContent = value ?? string.Empty;
        }
    }
}