namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Handle append select mode", 
                       examples: ["//Change append mode, turn off if on and vice versa","append"])]
public class AppendCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public static bool AppendMode = false;
    public override RunResult Run(ICommandLineInput input)
    {
        AppendMode = !AppendMode;
        Writer.WriteLine($"Append mode on: {AppendMode}");
        return Ok();
    }
}