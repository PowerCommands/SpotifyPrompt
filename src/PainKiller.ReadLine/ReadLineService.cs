﻿using PainKiller.ReadLine.Contracts;
using PainKiller.ReadLine.DomainObjects;
using PainKiller.ReadLine.Events;
using PainKiller.ReadLine.Handlers;
using PainKiller.ReadLine.Managers;
using static System.String;

namespace PainKiller.ReadLine
{
    public class ReadLineService
    {
        private readonly List<string> _history = new();
        private readonly List<string> _suggestions = new();
        private static readonly Lazy<ReadLineService> Lazy = new(() => new ReadLineService());
        public static ReadLineService Service => Lazy.Value;
        public static event EventHandler<CommandHighlightedArgs>? CommandHighlighted;
        public static event EventHandler<CmdLineTextChangedArgs>? CmdLineTextChanged;
        public static Action? OpenShortCutPressed;
        public static Action? SaveShortCutPressed;
        public static void InitializeAutoComplete(string[] history, string[] suggestions)
        {
            if (history.Length > 0) Service.AddHistory(history);
            if (suggestions.Length > 0) Service.AddSuggestions(suggestions);
            Service.AutoCompletionHandler = new AutoCompleteHandler(suggestions, new SuggestionProviderManager().SuggestionProviderFunc);
        }
        public void AddHistory(params string[] history) => _history.AddRange(history);
        public void AddSuggestions(params string[] suggestions) => _suggestions.AddRange(suggestions);
        public IAutoCompleteHandler AutoCompletionHandler { private get; set; } = null!;
        public string Read(string prompt = "")
        {
            Console.Write(prompt);
            var keyHandler = new KeyHandler(new Console2(), _history, AutoCompletionHandler);
            var text = GetText(keyHandler);
            if(!string.IsNullOrEmpty(text)) _history.Add(text.Trim());
            return text;
        }
        private string GetText(KeyHandler keyHandler)
        {
            ConsoleKeyInfo keyInfo = default;
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyInfo = Console.ReadKey(true);
                keyHandler.Handle(keyInfo);
                HighlightText(keyHandler);
                OnCmdLineTextChanged(new CmdLineTextChangedArgs(keyHandler.Text));
            }
            Console.WriteLine();
            OnCmdLineTextChanged(new CmdLineTextChangedArgs("\n"));
            return keyHandler.Text;
        }
        private void HighlightText(KeyHandler keyHandler)
        {
            if (IsNullOrEmpty(keyHandler.Text) || keyHandler.Text.Split(' ').Length > 1) return;

            if (_suggestions.Any(s => s == keyHandler.Text))
            {
                var currentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.SetCursorPosition(Console.GetCursorPosition().Left - keyHandler.Text.Length, Console.GetCursorPosition().Top);
                Console.Write(keyHandler.Text);
                OnCommandHighlighted(new CommandHighlightedArgs(keyHandler.Text));
                Console.ForegroundColor = currentColor;
            }
        }
        private static void OnCommandHighlighted(CommandHighlightedArgs e) => CommandHighlighted?.Invoke("ReadLineService", e);
        private static void OnCmdLineTextChanged(CmdLineTextChangedArgs e) => CmdLineTextChanged?.Invoke("ReadLineService", e);
        internal static void OnOpenShortCutPressed() => OpenShortCutPressed?.Invoke();
        internal static void OnSaveShortCutPressed() => SaveShortCutPressed?.Invoke();

    }
}