using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.ReadLine.Managers;
using PainKiller.SpotifyPromptClient.Commands;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class SpotifyInfoPanelContent(int refreshMarginInMinutes, int latestTracksCount) : IInfoPanelContent
{
    private readonly ILogger<SpotifyInfoPanelContent> _logger = LoggerProvider.CreateLogger<SpotifyInfoPanelContent>();
    public string GetText()
    {
        try
        {
            var configContainer = ConfigurationService.Service.Get<CommandPromptConfiguration>();
            var config = configContainer.Configuration;
            var clientId = config.Core.Modules.Security.DecryptSecret("spotify_prompt");
            var spotifyConfig = config.Spotify;

            var flowManager = new AuthorizationCodeFlowService(clientId, spotifyConfig.RedirectUri, spotifyConfig.Scopes);
            var refreshManager = RefreshTokenService.InitializeManager(flowManager, TimeSpan.FromMinutes(refreshMarginInMinutes));

            var (token, status) = refreshManager.EnsureTokenValid();

            var devices = DeviceService.Default.GetDevices();
            SuggestionProviderManager.AppendContextBoundSuggestions(nameof(DeviceCommand).Replace("Command", "").ToLower(), devices.OrderBy(d => d.IsActive).Select(d => d.Name).ToArray());

            var currentTrack = TrackService.Default.GetCurrentlyPlayingTrack();
            var currentlyPlaying = currentTrack == null ? "-" : $"{currentTrack.Artists.First().Name} - {currentTrack.Name}";
            LatestManager.Default.UpdateLatest(currentTrack, latestTracksCount);

            var volume = DeviceService.Default.GetCurrentVolume();
            var device = devices.FirstOrDefault(d => d.IsActive);
            var deviceName = $"{device?.Name} volume:{volume}%" ?? "No active device";

            var playerManager = new PlayerService();
            var shuffleState = playerManager.GetShuffleState();
            var shuffleStateText = shuffleState ? "Enabled" : "Disabled";
            
            var leftText = $"Device: {deviceName} {status}";
            var rightText = $"Shuffle status: {shuffleStateText}";

            var totalWidth = Console.WindowWidth;
            var padding = (totalWidth-1) - leftText.Length - rightText.Length;
            if (padding < 1) padding = 1;

            var secondLine = leftText + new string(' ', padding) + rightText;

            return $"Currently playing: {currentlyPlaying}\n{secondLine}";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while getting Spotify info panel content: {ex.Message}");
        }
        return "Error refreshing information, a refresh token may need to be fetched, you could try to start the player with play command.\nIf that does´nt work try to login again with login command.";
    }
}