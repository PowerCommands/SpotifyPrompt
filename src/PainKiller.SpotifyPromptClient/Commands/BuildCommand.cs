using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Playlist builder, use current selected items (artist, albums, tracks) to build new playlist, a playlist could have different source types, tag filtering, year range and use related artists to randomize new playlists.",
                    suggestions: ["new"],
                       examples: ["//Build a new playlist.","build"])]
public class BuildCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var templateStorage = new ObjectStorage<PlaylistTemplates, PlaylistTemplate>();
        var templates = templateStorage.GetItems();
        var suggestion = this.GetSuggestion(input.Arguments.FirstOrDefault(), "");
        var selectedTemplate = new PlaylistTemplate {Id = ""};

        if (string.IsNullOrEmpty(suggestion))
        {
            var selectedTemplateItems = ListService.ListDialog("Select template", templates.Select(t => t.Name).ToList());
            if (selectedTemplateItems.Count == 0) return Ok();
            selectedTemplate = templates[selectedTemplateItems.First().Key];
        }
        
        if(string.IsNullOrEmpty(selectedTemplate.Id)) selectedTemplate = GetNewTemplate();

        var config = Configuration.Core.Modules.Ollama;
        var tracks = BuildManager.Default.GetPlaylist(selectedTemplate, new AIManager(config.BaseAddress, config.Port,config.Model));
        var summary = BuildManager.Default.GetPlayListSummary(selectedTemplate);
        Writer.WriteLine();
        ConsoleService.Writer.WriteHeadLine($"Playlist summary: {summary}");
        Writer.WriteTable(tracks.Select(t => new {Artist = t.Artists.First().Name, t.Name}));
        ConsoleService.Writer.WriteHeadLine($"Playlist summary: {summary}");
        SelectedManager.Default.UpdateSelected(tracks);
        
        var confirmNewPlayList = DialogService.YesNoDialog("Do you want to create a new playlist with these tracks?");
        if (!confirmNewPlayList) return Ok();

        var name = DialogService.QuestionAnswerDialog("Name of the playlist:");
        PlaylistManager.Default.CreatePlaylist(name, $"{selectedTemplate.Description} created by {Configuration.Core.Name}", tracks, selectedTemplate.Tags);

        var confirmSave = DialogService.YesNoDialog("Do you want to save this template for future use?");
        if (confirmSave)
        {
            var ids = new List<string>();
            if(selectedTemplate.SourceType == PlaylistSourceType.Albums) ids = SelectedManager.Default.GetSelectedAlbums().Select(a => a.Id).ToList();
            if (selectedTemplate.SourceType == PlaylistSourceType.Artists) ids = SelectedManager.Default.GetSelectedArtists().Select(a => a.Id).ToList();
            var newTemplate = new PlaylistTemplate { Id = Guid.NewGuid().ToString(), Name = name, Tags = selectedTemplate.Tags, SourceType = selectedTemplate.SourceType, RandomMode = selectedTemplate.RandomMode, YearRange = selectedTemplate.YearRange, Count = selectedTemplate.Count, Ids = ids};
            templateStorage.Insert(newTemplate, template => template.Id == newTemplate.Id);
        }
        return Ok();
    }
    private PlaylistTemplate GetNewTemplate()
    {
        Writer.WriteLine();
        var retVal = new PlaylistTemplate { Name = DialogService.QuestionAnswerDialog("Name:") };
        var tags = BuildManager.Default.GetTags();
        tags.Insert(0, "*");
        var tagsSelect = ListService.ListDialog("Choose tags:", tags);
        retVal.Tags = tagsSelect.Select(t => t.Value).ToList() ?? [];
        retVal.SourceType = ToolbarService.NavigateToolbar<PlaylistSourceType>();
        retVal.RandomMode = ToolbarService.NavigateToolbar<RandomMode>();
        var yearSpan = new YearRange(1900, 2100);
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
        int.TryParse(DialogService.QuestionAnswerDialog("Max count per artist:"), out var maxCount);
        retVal.MaxCountPerArtist = maxCount > 0 ? maxCount : 3;
        retVal.YearRange = yearSpan;
        var count = DialogService.QuestionAnswerDialog("How many tracks should the playlist contain (max value):");
        retVal.Count = int.TryParse(count, out var countValue) ? countValue : 100;
        return retVal;
    }
}