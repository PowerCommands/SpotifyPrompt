using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Artist actions", 
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

        SelectedService.Default.UpdateSelected(selectedArtists);

        var tracksStorage = new ObjectStorage<Tracks, TrackObject>();
        var tracks = new List<TrackObject>();
        foreach (var artist in selectedArtists.Take(10))
        {
            var artistTracks = tracksStorage.GetItems().Where(t => t.Artists.Any(a => a.Name.Contains(artist.Name))).ToList() ?? [];
            if (artistTracks.Count == 0) continue;
            tracks.AddRange(artistTracks);
        }
        SelectedService.Default.UpdateSelected(tracks);
        ShowSelectedTracks();

        //var firstArtist = selectedArtists.First();
        //var topTracks = ArtistManager.Default.GetTopTracks(firstArtist.Id);
        //Writer.WriteHeadLine("Top tracks");
        //Writer.WriteTable(topTracks.Select(t => new { t.Name, AlbumName = t.Album.Name, t.Album.ReleaseDate }));

        //var aiConfig = Configuration.Core.Modules.Ollama;
        //var aiManager = new AIManager(aiConfig.BaseAddress, aiConfig.Port, aiConfig.Model);
        //var relatedArtistsNames = aiManager.GetSimilarArtists(firstArtist.Name);
        
        //var relatedArtists = new List<ArtistSimplified>();
        //foreach (var artistName in relatedArtistsNames)
        //{
        //    var artist = ArtistManager.Default.GetArtistByName(artistName);
        //    if (artist != null) relatedArtists.Add(artist);
        //}
        //Writer.WriteHeadLine("Related artists");
        //Writer.WriteTable(relatedArtists.Select(a => new {a.Id, a.Name}));
        return Ok();
    }
    private void Presentation(List<ArtistSimplified> items) => Writer.WriteTable(items);
}