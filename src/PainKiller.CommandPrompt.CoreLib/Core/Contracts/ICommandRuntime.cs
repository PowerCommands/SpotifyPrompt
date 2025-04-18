namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;
public interface ICommandRuntime
{
    RunResult Execute(string input);
}