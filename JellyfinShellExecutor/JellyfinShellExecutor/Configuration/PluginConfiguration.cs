using MediaBrowser.Model.Plugins;

namespace JellyfinShellExecutor.Configuration
{
    /// <summary>
    /// Plugin configuration class.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            ScriptContent = @"#!/bin/bash
# Default script content
echo ""Shell Executor Plugin initialized at $(date)""
";
        }

        /// <summary>
        /// Gets or sets the script content.
        /// </summary>
        public string ScriptContent { get; set; }
    }
}