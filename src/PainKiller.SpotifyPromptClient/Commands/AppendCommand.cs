namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Handle append select mode, build playlist by selecting artist, tracks or albums and append that to the current selected items.\nThis way you can build up your content and use build command to create your playlist with the selected mode.", 
                    suggestions: ["on", "off"],  
                       examples: ["//Change append mode, turn off if on and vice versa","append"])]
public class AppendCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public static bool AppendMode = true;
    public override RunResult Run(ICommandLineInput input)
    {
        var toggle = this.GetSuggestion(input.Arguments.FirstOrDefault(), "");
        if (!string.IsNullOrEmpty(toggle)) AppendMode = toggle == "on";
        Writer.WriteDescription($"Append mode:", AppendMode ? "Enabled" : "Disabled","Change mode with enable or disable argument");
        return Ok();
    }
}