﻿using PainKiller.CommandPrompt.CoreLib.Modules.StorageModule.Services;
using PainKiller.SpotifyPromptClient.DomainObjects;

namespace PainKiller.SpotifyPromptClient.Managers;
public class RefreshTokenManager
{
    private readonly AuthorizationCodeFlowManager _flowManager;
    private readonly TimeSpan _refreshBuffer;
    private RefreshTokenManager(AuthorizationCodeFlowManager flowManager, TimeSpan refreshBuffer)
    {
        _flowManager = flowManager;
        _refreshBuffer = refreshBuffer;
    }
    private static RefreshTokenManager _refreshTokenManager = null!;
    public static RefreshTokenManager InitializeManager(AuthorizationCodeFlowManager flowManager, TimeSpan refreshBuffer) => _refreshTokenManager ??= new RefreshTokenManager(flowManager, refreshBuffer);
    public static RefreshTokenManager DefaultInstance() => _refreshTokenManager;
    
    /// <summary>
    /// Ensures the stored token is refreshed if expired or within the buffer period.
    /// Returns the current valid token and a status message.
    /// </summary>
    public (TokenResponse Token, string Status) EnsureTokenValid()
    {
        var token = StorageService<TokenResponse>.Service.GetObject();
        if (string.IsNullOrEmpty(token.AccessToken)) return (token, "Spotify: not authenticated");
        if (token.IsExpired || token.TimeUntilExpiration < _refreshBuffer)
        {
            try
            {
                var newToken = _flowManager.RefreshAccessTokenAsync(token.RefreshToken)
                                           .GetAwaiter().GetResult();
                newToken.RetrievedAt = DateTime.UtcNow;
                StorageService<TokenResponse>.Service.StoreObject(newToken);
                var mins = (int)newToken.TimeUntilExpiration.TotalMinutes;
                return (newToken, $"Spotify: token refreshed, expires in {mins} min");
            }
            catch (Exception ex)
            {
                return (token, $"Spotify: refresh failed ({ex.Message})");
            }
        }
        var remaining = (int)token.TimeUntilExpiration.TotalMinutes;
        return (token, $"Spotify: token valid for {remaining} min");
    }
}