using System;
using System.Net.Mime;
using MediaBrowser.Controller.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JellyfinShellExecutor.Api
{
    /// <summary>
    /// Shell Executor API Controller.
    /// </summary>
    [ApiController]
    [Authorize(Policy = "RequiresElevation")]
    [Route("plugins/shellexecutor")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ShellExecutorController : ControllerBase
    {
        private readonly Plugin _plugin;
        private readonly IServerConfigurationManager _config;
        private readonly ILogger<ShellExecutorController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellExecutorController"/> class.
        /// </summary>
        /// <param name="config">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{ShellExecutorController}"/> interface.</param>
        public ShellExecutorController(
            IServerConfigurationManager config,
            ILogger<ShellExecutorController> logger)
        {
            _config = config;
            _logger = logger;
            _plugin = Plugin.Instance!;
        }

        /// <summary>
        /// Execute the shell script immediately.
        /// </summary>
        /// <returns>Execution result.</returns>
        [HttpPost("Execute")]
        public IActionResult ExecuteScript()
        {
            try
            {
                _logger.LogInformation("Executing script via API");
                _plugin.ExecuteScript();
                return new JsonResult(new ExecutionResult
                {
                    Success = true,
                    Message = "Script executed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute script via API");
                return new JsonResult(new ExecutionResult
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Execution result model.
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the execution was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the execution message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}