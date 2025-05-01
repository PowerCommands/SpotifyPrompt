using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Artist actions, selected artists will be stored in the current selection that could be used to build playlists.\nEnable append mode with append command to append artists to the current selection.", 
                      arguments: ["filter"],
                        options: ["tags"],
                       examples: ["//Show your artists and their tracks","artist"])]
public class ArtistCommand(string identifier) : SelectedBaseCommand(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var filter = string.Join(' ', input.Arguments);
        var artistStorage = new SpotifyObjectStorage<Artists, ArtistSimplified>();
        var artists = artistStorage.GetItems();
        
        var tags = input.GetOptionValue("tags");
        var selectedArtists = ListService.ShowSelectFromFilteredList("Select a artist!", artists,(info, s) => (info.Name.Contains(s,StringComparison.OrdinalIgnoreCase) && info.Tags.Contains(tags, StringComparison.OrdinalIgnoreCase)), Presentation, Writer, filter);
        if (selectedArtists.Count == 0) return Ok();
        if (AppendCommand.AppendMode && selectedArtists.Count > 1)
        {
            var selectOneArtist = ListService.ListDialog("Select one artist", selectedArtists.Select(a => a.Name).ToList());
            if (selectOneArtist.Count > 0)
            {
                var artist = selectedArtists[selectOneArtist.First().Key];
                selectedArtists.Clear();
                selectedArtists.Add(artist);
            }
        }
        if(AppendCommand.AppendMode) SelectedManager.Default.AppendToSelected(selectedArtists);
        else SelectedManager.Default.UpdateSelected(selectedArtists);
        
        ShowSelectedArtists();
        if(AppendCommand.AppendMode) return Ok();

        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var artist in selectedArtists.Take(10))
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Artists.Any(a => a.Name.Contains(artist.Name))).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        SelectedManager.Default.UpdateSelected(tracks);
        ShowSelectedTracks();
        return Ok();
    }
    private void Presentation(List<ArtistSimplified> items) => Writer.WriteTable(items);
}