using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Contracts;
using PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Authorize to your registered spotify application", 
                        options: [""],
                       examples: ["//Authorize with your spotify account","login"])]
public class LoginCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        try
        {
            var clientId = Configuration.Core.Modules.Security.DecryptSecret("spotify_prompt");
            var cfg    = Configuration.Spotify;
            var mgr    = new AuthorizationCodeFlowManager(clientId, cfg.RedirectUri, cfg.Scopes);
            Writer.WriteLine("Authenticating with Spotify...");
            var code = mgr.AuthenticateAsync().GetAwaiter().GetResult();

            Writer.WriteLine("Exchanging authorization code for access token...");
            var tokenResponse = mgr.ExchangeCodeForTokenAsync(code).GetAwaiter().GetResult();

            StorageService<TokenResponse>.Service.StoreObject(tokenResponse);

            Writer.WriteSuccessLine("Authentication successful. Token has been saved.");
            return Ok();
        }
        catch (Exception ex)
        {
            Writer.WriteError($"Authentication failed: {ex.Message}", "");
            return Nok("Authorization process failed.");
        }
    }

}