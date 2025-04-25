using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;
using PainKiller.SpotifyPromptClient.Services;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Playlist builder", 
                       examples: ["//Build a new playlist.","build"])]
public class BuildCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var templateStorage = new ObjectStorage<PlaylistTemplates, PlaylistTemplate>();
        var playlistStorage = new ObjectStorage<Playlists, PlaylistInfo>();
        var playlistTrackStorage = new ObjectStorage<PlaylistTracks, PlaylistWithTracks>();
        var templates = templateStorage.GetItems();
        var selectedTemplateItems = ListService.ListDialog("Select template", templates.Select(t => t.Name).ToList());
        if (selectedTemplateItems.Count == 0) return Ok();
        var selectedTemplate = templates[selectedTemplateItems.First().Key];
        if(string.IsNullOrEmpty(selectedTemplate.Id)) selectedTemplate = GetNewTemplate();
        var tracks = BuildService.Default.GetPlaylist(selectedTemplate);
        var summary = BuildService.Default.GetPlayListSummary(selectedTemplate);
        Writer.WriteLine();
        ConsoleService.Writer.WriteHeadLine($"Playlist summary: {summary}");
        Writer.WriteTable(tracks.Select(t => new {Artist = t.Artists.First().Name, t.Name}));

        var confirmNewPlayList = DialogService.YesNoDialog("Do you want to create a new playlist with these tracks?");
        if (!confirmNewPlayList) return Ok();

        var name = DialogService.QuestionAnswerDialog("Name of the playlist:");
        var id = PlaylistModifyManager.Default.CreatePlaylist(UserManager.Default.GetCurrentUser().Id, name, $"{selectedTemplate.Description} created by {Configuration.Core.Name}");
        PlaylistModifyManager.Default.AddTracksToPlaylist(id, tracks.Select(t => t.Uri).ToList());
        playlistStorage.Insert(new PlaylistInfo { Id = id, Name = name, Owner = UserManager.Default.GetCurrentUser().Id, Tags = string.Join(',', selectedTemplate.Tags), TrackCount = tracks.Count}, playlist => playlist.Id == id);
        playlistTrackStorage.Insert(new PlaylistWithTracks { Id = id, Items = tracks }, playlist => playlist.Id == id);

        var confirmSave = DialogService.YesNoDialog("Do you want to save this template for future use?");
        if (confirmSave)
        {
            var newTemplate = new PlaylistTemplate { Id = Guid.NewGuid().ToString(), Name = name, Tags = selectedTemplate.Tags, SourceType = selectedTemplate.SourceType, RandomMode = selectedTemplate.RandomMode, YearSpan = selectedTemplate.YearSpan, Count = selectedTemplate.Count };
            templateStorage.Insert(newTemplate, template => template.Id == newTemplate.Id);
        }
        return Ok();
    }
    private PlaylistTemplate GetNewTemplate()
    {
        var retVal = new PlaylistTemplate { Name = DialogService.QuestionAnswerDialog("Name:") };
        var tags = BuildService.Default.GetTags();
        var tagsSelect = ListService.ListDialog("Choose tags:", tags, multiSelect: true);
        retVal.Tags = tagsSelect.Select(t => t.Value).ToList() ?? [];
        retVal.SourceType = ToolbarService.NavigateToolbar<PlaylistSourceType>();
        retVal.RandomMode = ToolbarService.NavigateToolbar<RandomMode>();
        var yearSpan = new YearSpan(1900, 2100);
        var confirmSetYear = DialogService.YesNoDialog("Do you want to specify a year range?");
        if(confirmSetYear)
        {
            var startYear = DialogService.QuestionAnswerDialog("Start year:");
            var endYear = DialogService.QuestionAnswerDialog("End year:");
            if (int.TryParse(startYear, out var start) && int.TryParse(endYear, out var end))
            {
                yearSpan.Start = start;
                yearSpan.End = end;
            }
        }
        retVal.YearSpan = yearSpan;
        var count = DialogService.QuestionAnswerDialog("How many tracks should the playlist contain (max value):");
        retVal.Count = int.TryParse(count, out var countValue) ? countValue : 100;
        return retVal;
    }
}