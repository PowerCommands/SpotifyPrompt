# Spotify PromptClient

A CLI extension for the PainKiller CommandPrompt framework that lets you control Spotify playback directly from your terminal.

<img src="images/logo2.png" width="640">

## Prerequisites

- **.NET 9.0 SDK** installed on your machine.
- A **Spotify Developer** account with a registered application. You’ll need:
  - **Client ID**
  - **Redirect URI** (e.g., `http://localhost:5000/callback/`)
- Your Spotify **Client ID** stored as an encrypted secret under the key `spotify_prompt` in the built‑in Security module.

   ```secret --create "spotify_prompt"```


## Installation

1. **Clone** this repository:
   ```bash
   git clone https://github.com/your-org/SpotifyPromptClient.git
   cd SpotifyPromptClient
   ```

2. **Configure** your application settings in `CommandPromptConfiguration.yaml` (or equivalent). Example:
   ```yaml
   spotify:
     redirectUri: "http://localhost:5000/callback/"
     refreshMarginInMinutes: 10
     scopes:
       - user-read-playback-state
       - user-modify-playback-state
       - playlist-read-private   
   ```

3. **Build** the project:
   ```bash
   dotnet build
   ```

4. **Run** the CommandPrompt host (it will auto‑discover SpotifyPromptClient commands):
   ```bash
   dotnet run --project src/PainKiller.CommandPrompt.Host
   ```

## Basic Spotify Commands

| Command                   | Description                                                                                           |
|---------------------------|-------------------------------------------------------------------------------------------------------|
| `login`                   | Authorize the CLI to access your Spotify account.                                                     |
| `device [DeviceName]`     | List saved devices. If you provide `DeviceName`, switches playback to that device.                  |
| `list [--update]`         | Show your playlists. Use `--update` to fetch & cache all playlists from Spotify before selecting one. |
| `play`                    | Start or resume playback on the current active device.                                                |
| `pause`                   | Pause playback.                                                                                       |
| `next`                    | Skip to the next track.                                                                               |
| `previous`                | Go back to the previous track.                                                                        |

### Workflow Example

1. **Login**:
   ```bash
   login
   ```
2. **(Optional) Select Device**:
   ```bash
   device            # lists devices & marks the active one
   device "My Phone"   # switch to "My Phone" for playback
   ```
3. **Update & List Playlists**:
   ```bash
   list --update    # fetch all playlists from Spotify
   list             # choose a playlist to play
   ```
   - After running `list`, you’ll get an interactive search to pick a playlist by name.
   - The selected playlist will start playing automatically.
4. **Control Playback**:
   ```bash
   next     # skip forward
   previous # skip back
   pause    # pause
   play     # resume
   ```

## Tips

- You can re‑run `list` without `--update` to quickly pick a different playlist from cache.
- If your access token expires, the built‑in info‑panel will auto‑refresh it (you’ll see status at the top).
- Ensure your Redirect URI in the Spotify Developer Dashboard matches exactly what you configured.

---

**Enjoy controlling Spotify right from your terminal!**

