using Spectre.Console;
namespace PainKiller.CommandPrompt.CoreLib.Core.Presentation;
public static class InteractiveFilterMultiSelect<T>
{
    public static List<T> Run(IEnumerable<T> items, Func<T, string, bool> filter, Action<IEnumerable<T>, int> display)
    {
        var list = items.ToList();
        var filtered = list;
        var filterString = "";
        var selectedIndex = 0;

        while (true)
        {
            Console.Clear();
            display(filtered, selectedIndex);

            if (!string.IsNullOrWhiteSpace(filterString)) AnsiConsole.MarkupLine($"\n[grey]Filter:[/] [italic]{Markup.Escape(filterString)}[/]");
            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    return filtered;

                case ConsoleKey.Escape:
                    return new List<T>();

                case ConsoleKey.UpArrow:
                    if (selectedIndex > 0) selectedIndex--;
                    break;

                case ConsoleKey.DownArrow:
                    if (selectedIndex < filtered.Count - 1) selectedIndex++;
                    break;

                case ConsoleKey.Backspace:
                    if (filterString.Length > 0)
                    {
                        filterString = filterString[..^1];
                        filtered = list.Where(e => filter(e, filterString)).ToList();
                        selectedIndex = 0;
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        filterString += key.KeyChar;
                        filtered = list.Where(e => filter(e, filterString)).ToList();
                        selectedIndex = 0;
                    }
                    break;
            }

            if (selectedIndex >= filtered.Count)
                selectedIndex = Math.Max(0, filtered.Count - 1);
        }
    }
}