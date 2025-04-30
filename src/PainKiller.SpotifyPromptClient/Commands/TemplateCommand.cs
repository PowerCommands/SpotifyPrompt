using PainKiller.SpotifyPromptClient.DomainObjects.Data;
using PainKiller.SpotifyPromptClient.Enums;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - Handle templates",
                       examples: ["//View templates.","template"])]
public class TemplateCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var templateStorage = new ObjectStorage<PlaylistTemplates, PlaylistTemplate>();
        var templates = templateStorage.GetItems();
        var selectTemplate = ListService.ListDialog("Select template", templates.Select(t => t.Name).ToList());
        if (selectTemplate.Count == 0) return Ok();
        var selectedTemplate = templates[selectTemplate.First().Key];
        var action = ToolbarService.NavigateToolbar<TemplateAction>();
        if (action == TemplateAction.Delete)
        {
            var confirmDelete = DialogService.YesNoDialog($"Do you want to delete template {selectedTemplate.Name}?");
            if (confirmDelete)
            {
                templateStorage.Remove(template => template.Id == selectedTemplate.Id);
                Writer.WriteSuccessLine($"Template {selectedTemplate.Name} deleted");
            }
        }
        //else if (action == TemplateAction.Edit)
        //{
            
        //}
        return Ok();
    }
}