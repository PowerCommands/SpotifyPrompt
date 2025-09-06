using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Start play on the active Spotify player.",
                       examples: ["//Play","play","//Play the third track of the current selected tracks.","play 3","//Play all selected tracks","play --all"])]
public class PlayCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override void OnInitialized()
    {
        if(Configuration.Spotify.StartSpotifyClient) ShellService.Default.Execute("spotify");
    }
    public override RunResult Run(ICommandLineInput input)
    {
        IPlayerService playerManager = new PlayerService();
        try
        {
            playerManager.Play();
        }
        catch (Exception ex)
        {
            Writer.WriteLine("Trying to start Spotify...");
            ShellService.Default.Execute("spotify");
            Thread.Sleep(3000);
            playerManager.Play();
            return Ok(ex.Message);
        }

        InfoPanelService.Instance.Update();
        return Ok();
    }
}