using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Contracts;
using Spectre.Console;

namespace PainKiller.SpotifyPromptClient.DomainObjects
{
    public class SpotifyPanel(IInfoPanelContent content) : IInfoPanel
    {
        public void Draw(int margin)
        {
            var top = Console.CursorTop;
            var left = Console.CursorLeft;

            Clear(margin);
            
            var text = content.GetText();
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length && i < margin; i++)
            {
                Console.SetCursorPosition(0, i);
                var padded = lines[i].PadRight(Console.WindowWidth);
                AnsiConsole.MarkupLine($"[black on lightgreen]{padded}[/]");
            }
            Console.SetCursorPosition(left, top);
        }
        private void Clear(int margin)
        {
            Console.SetCursorPosition(0, 0);
            var blankLine = new string(' ', Console.WindowWidth);
            for (int i = 0; i < margin; i++)
            {
                AnsiConsole.MarkupLine($"[on lightgreen]{blankLine}[/]");
            }
        }
    }
}