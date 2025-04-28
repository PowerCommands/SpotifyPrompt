using PainKiller.SpotifyPromptClient.Managers;
namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - View selected tracks, albums and artist. Could be used to create playlists.",
                        options: ["clear"],
                       examples: ["//View selected tracks, albums and artists","selected"])]
public class SelectedCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("clear"))
        {
            SelectedManager.Default.Clear();
            Writer.WriteSuccessLine("Selected items cleared.");
            return Ok();
        }
        Writer.WriteHeadLine("Selected items, you can append more with search command.");
        var tracks = SelectedManager.Default.GetSelectedTracks();
        var albums = SelectedManager.Default.GetSelectedAlbums();
        var artists = SelectedManager.Default.GetSelectedArtists();
        Writer.WriteDescription("Artists:",artists.Count.ToString());
        ShowSelectedArtists();
        Writer.WriteDescription("Albums:", albums.Count.ToString());
        ShowSelectedAlbums();
        Writer.WriteDescription("Tracks:", tracks.Count.ToString());
        ShowSelectedTracks();
        return Ok();
    }

    private void ShowSelectedTracks()
    {
        var tracks = SelectedManager.Default.GetSelectedTracks();
        Writer.WriteTable(tracks.Select((t, idx) => new { Index = idx + 1, Artist = t.Artists.FirstOrDefault()?.Name, t.Name, Album = t.Album.Name, t.Album.ReleaseDate }));
    }
    protected void ShowSelectedAlbums()
    {
        var albums = SelectedManager.Default.GetSelectedAlbums();
        if (albums.Count == 0)
        {
            return;
        }
        Writer.WriteTable(albums.Select(a => new { a.Name, Artist = a.Artists.FirstOrDefault()?.Name, a.ReleaseDate, a.TotalTracks }));
    }
    protected void ShowSelectedArtists()
    {
        var artists = SelectedManager.Default.GetSelectedArtists();
        Writer.WriteTable(artists.Select(a => new { a.Name, a.Tags }));
    }
}