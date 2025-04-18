﻿using PainKiller.CommandPrompt.CoreLib.Configuration.Services;
using PainKiller.CommandPrompt.CoreLib.Core.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.SecurityModule.Extensions;
using PainKiller.ReadLine.Managers;
using PainKiller.SpotifyPromptClient.Commands;
using PainKiller.SpotifyPromptClient.Configuration;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.DomainObjects;
public class SpotifyInfoPanelContent(int refreshMarginInMinutes) : IInfoPanelContent
{
    public string GetText()
    {
        try
        {
            var configContainer = ConfigurationService.Service.Get<CommandPromptConfiguration>();
            var config = configContainer.Configuration;
            var clientId = config.Core.Modules.Security.DecryptSecret("spotify_prompt");
            var spotifyConfig = config.Spotify;

            var flowManager = new AuthorizationCodeFlowManager(clientId, spotifyConfig.RedirectUri, spotifyConfig.Scopes);
            var refreshManager = RefreshTokenManager.InitializeManager(flowManager, TimeSpan.FromMinutes(refreshMarginInMinutes));

            var (token, status) = refreshManager.EnsureTokenValid();

            var devices = DeviceManager.Default.GetDevices();
            SuggestionProviderManager.AppendContextBoundSuggestions(
                nameof(DeviceCommand).Replace("Command", "").ToLower(),
                devices.OrderBy(d => d.IsActive).Select(d => d.Name).ToArray()
            );

            var playerManager = new PlayerManager(refreshManager);
            var currentlyPlaying = playerManager.GetCurrentlyPlaying();

            var device = devices.FirstOrDefault(d => d.IsActive);
            var deviceName = device?.Name ?? "No active device";

            var shuffleState = playerManager.GetShuffleState();
            var shuffleStateText = shuffleState ? "Enabled" : "Disabled";

            // Build two columns: left = device/status, right = shuffle
            var leftText = $"Device: {deviceName} {status}";
            var rightText = $"Shuffle status: {shuffleStateText}";

            var totalWidth = Console.WindowWidth;
            var padding = (totalWidth-1) - leftText.Length - rightText.Length;
            if (padding < 1) padding = 1;

            var secondLine = leftText + new string(' ', padding) + rightText;

            return $"Currently playing: {currentlyPlaying.Artists} - {currentlyPlaying.TrackName}\n{secondLine}";
        }
        catch (Exception)
        {
            ConsoleService.Writer.WriteError(
                "Could not update info, you probably need to login with login command."
            );
        }
        return "You probably need to login with login command";
    }
}