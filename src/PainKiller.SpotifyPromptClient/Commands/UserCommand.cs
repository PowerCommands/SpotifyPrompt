using PainKiller.SpotifyPromptClient.Managers;
using Spectre.Console;

namespace PainKiller.SpotifyPromptClient.Commands;

[CommandDesign(     description: "Spotify - User command", 
                       examples: ["//View user details","user"])]
public class UserCommand(string identifier) : ConsoleCommandBase<CommandPromptConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        var user = UserManager.Default.GetCurrentUser();
        DisplayUser(user);
        return Ok();
    }
    private void DisplayUser(UserProfile user)
    {
        AnsiConsole.MarkupLine("[bold green] 🎵 Spotify User Profile[/]");
        AnsiConsole.WriteLine();
        
        var table = new Table { Border = TableBorder.Rounded, Expand = true };
        table.AddColumn(new TableColumn("[green]Property[/]").LeftAligned());
        table.AddColumn(new TableColumn("[green]Value[/]").LeftAligned());
        
        table.AddRow("[green]ID[/]", user.Id);
        table.AddRow("[green]Display Name[/]", user.DisplayName);
        table.AddRow("[green]Email[/]", user.Email);
        table.AddRow("[green]Country[/]", user.Country);
        table.AddRow("[green]Product[/]", user.Product);
        
        var followersCell = $"[green]{user.Followers.Total}[/]";
        if (!string.IsNullOrEmpty(user.Followers.Href))
        {
            followersCell += $" ([underline blue]{user.Followers.Href}[/])";
        }
        table.AddRow("[green]Followers[/]", followersCell);
        
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}