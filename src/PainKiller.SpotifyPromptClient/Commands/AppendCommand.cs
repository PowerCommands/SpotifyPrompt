namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Handle append select mode, build playlist by selecting artist, tracks or albums and append that to the current selected items.\nThis way you can build up your content and use build command to create your playlist with the selected mode.", 
                       examples: ["//Change append mode, turn off if on and vice versa","append"])]
public class AppendCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public static bool AppendMode = true;
    public override RunResult Run(ICommandLineInput input)
    {
        AppendMode = !AppendMode;
        Writer.WriteLine($"Append mode on: {AppendMode}");
        return Ok();
    }
}