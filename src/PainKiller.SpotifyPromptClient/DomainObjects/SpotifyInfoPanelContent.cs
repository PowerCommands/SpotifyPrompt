using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class SpotifyInfoPanelContent : IInfoPanelContent
{
    private readonly RefreshTokenManager _refreshManager;
    public SpotifyInfoPanelContent(int refreshMarginInMinutes)
    {
        var configContainer = ConfigurationService.Service.Get<CommandPromptConfiguration>();
        var config = configContainer.Configuration;
        var clientId = config.Core.Modules.Security.DecryptSecret("spotify_prompt");
        var spotifyConfig = config.Spotify;

        var flowManager = new AuthorizationCodeFlowManager(clientId, spotifyConfig.RedirectUri, spotifyConfig.Scopes);
        _refreshManager = RefreshTokenManager.InitializeManager(flowManager, TimeSpan.FromMinutes(refreshMarginInMinutes));
    }
    public string GetText()
    {
        var (token, status) = _refreshManager.EnsureTokenValid();
        return status;
    }
}