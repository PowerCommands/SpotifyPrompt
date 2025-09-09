using PainKiller.CommandPrompt.CoreLib.Core.Enums;
using PainKiller.CommandPrompt.CoreLib.Modules.InfoPanelModule.Services;
using PainKiller.SpotifyPromptClient.Enums;
using PainKiller.SpotifyPromptClient.Managers;

namespace PainKiller.SpotifyPromptClient.Services;
public static class CustomListService
{
    public static List<T> ShowSelectFromList<T>(string headline, List<T> items, IConsoleWriter writer, bool multiSelect = true, Func<T, string, bool>? match = null, string initialSearch = "") where T : class, new()
    {
        var inputBuffer = initialSearch;
        var selectedIndices = new HashSet<int>();
        var currentIndex = 0;
        var filteringEnabled = match != null;

        while (true)
        {
            Console.Clear();

            Console.WriteLine(filteringEnabled
                ? $"{Emo.Right.Icon()} Type to filter, ↑↓ to navigate, SPACE to select, ENTER to confirm, ESC to exit."
                : $"{Emo.Right.Icon()} ↑↓ to navigate, SPACE to select, ENTER to confirm, ESC to exit.");

            Console.Title = filteringEnabled && !string.IsNullOrEmpty(inputBuffer) ? inputBuffer : "*";

            var listToShow = filteringEnabled
                ? items.Where(item => match!(item, inputBuffer)).ToList()
                : items;

            if (listToShow.Count == 0)
            {
                Console.WriteLine($"No matching results... (Press ESC {Emo.Escape.Icon()} to exit)");
            }
            else
            {
                writer.WriteHeadLine(headline);
                for (int i = 0; i < listToShow.Count; i++)
                {
                    var prefix = i == currentIndex ? "→" : " ";
                    var selected = selectedIndices.Contains(i) ? "[x]" : "[ ]";
                    Console.WriteLine($"{prefix} {(multiSelect ? selected : "   ")} {listToShow[i]}");
                }
            }

            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Escape) return new List<T>();
            if (key.Key == ConsoleKey.Enter)
            {
                if (multiSelect)
                    return selectedIndices.Select(index => listToShow[index]).ToList();
                return listToShow.Count > currentIndex ? new List<T> { listToShow[currentIndex] } : new List<T>();
            }
            if (key.Key == ConsoleKey.UpArrow)
            {
                currentIndex = (currentIndex - 1 + listToShow.Count) % listToShow.Count;
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                currentIndex = (currentIndex + 1) % listToShow.Count;
            }
            else if (key.Key == ConsoleKey.Spacebar && multiSelect)
            {
                if (selectedIndices.Contains(currentIndex))
                    selectedIndices.Remove(currentIndex);
                else
                    selectedIndices.Add(currentIndex);
            }
            else if (filteringEnabled)
            {
                if (key.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    inputBuffer = inputBuffer[..^1];
                    currentIndex = 0;
                    selectedIndices.Clear();
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    inputBuffer += key.KeyChar;
                    currentIndex = 0;
                    selectedIndices.Clear();
                }
            }
        }
    }
    public static void ShowSelectedTracks(List<TrackObject> tracks, IConsoleWriter writer, string title = "")
    {
        if (tracks.Count == 0)
        {
            writer.WriteLine("No tracks selected.");
            return;
        }
        if (!string.IsNullOrEmpty(title)) writer.WriteHeadLine(title);
        var selectedTracks = ShowSelectFromList("Select your tracks", tracks, writer);
        if (selectedTracks.Count == 0) return;

        var action = ToolbarService.NavigateToolbar<SelectedTracksAction>();
        if (action == SelectedTracksAction.Play)
        {
            IPlayerService playerManager = new PlayerService();
            playerManager.Play(selectedTracks.Select(t => t.Uri));
            InfoPanelService.Instance.Update();
        }
        if (action == SelectedTracksAction.Queue)
        {
            foreach (var track in selectedTracks) QueueService.Default.AddToQueue(track.Uri);
            writer.WriteSuccessLine("Tracks added to queue."); ;
        }
        else if (action == SelectedTracksAction.Playlist)
        {
            var user = UserService.Default.GetCurrentUser();
            var playListName = DialogService.QuestionAnswerDialog("Name your new playlist");
            var description = DialogService.QuestionAnswerDialog("Describe your new playlist");

            PlaylistManager.Default.CreatePlaylist(playListName, $"{description} Playlist created with SpotifyPrompt", selectedTracks, [user.DisplayName], isPublic: true);
        }
    }
}