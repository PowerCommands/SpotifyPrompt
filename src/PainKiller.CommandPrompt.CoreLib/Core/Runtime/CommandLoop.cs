using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.ReadLine.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Core.Runtime;

public class CommandLoop(ICommandRuntime runtime, IUserInputReader inputReader, CoreConfiguration config) : ICommandLoop
{
    private readonly ILogger<CommandLoop> _logger = LoggerProvider.CreateLogger<CommandLoop>();
    public void Start()
    {
        while (true)
        {
            var input = inputReader.ReadLine(config.Prompt).Trim();
            _logger.LogDebug(input);

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"User entered exit, application shutdown.");
                break;
            }
            var result = runtime.Execute(input);
            if(result.Success) _logger.LogDebug($"Result: {result.Identifier} {result.Message} {result.Success}");
            else
            {
                ConsoleService.Writer.WriteError($"Error occured running {result.Identifier} command. {result.Message}");
                _logger.LogCritical($"Error occured running {result.Identifier} command. {result.Message}");
            }
        }
    }
}