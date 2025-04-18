using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
namespace PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.DomainObjects;
public class DefaultInfoPanel(IInfoPanelContent content) : IInfoPanel
{
    public void Draw(int margin)
    {
        var top = Console.CursorTop;
        var left = Console.CursorLeft;
        Clear(margin);
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(content.GetText());
        Console.CursorTop = top;
        Console.CursorLeft = left;
    }

    private void Clear(int margin)
    {
        Console.SetCursorPosition(0, 0);
        for (int i = 0; i < margin; i++)
        {
            Console.WriteLine(new string(' ', Console.WindowWidth));
        }
    }
}