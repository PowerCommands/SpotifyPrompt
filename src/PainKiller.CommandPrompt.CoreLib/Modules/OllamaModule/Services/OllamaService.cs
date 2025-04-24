using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.DomainObjects;
using Microsoft.Extensions.Logging;
using PainKiller.CommandPrompt.CoreLib.Logging.Services;
using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Contracts;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Services;
public class OllamaService : IOllamaService
{
    private readonly ILogger<OllamaService> _logger = LoggerProvider.CreateLogger<OllamaService>();
    private readonly List<ChatMessage> _messages = [];

    private static IOllamaService? _instance;
    private static readonly Lock LockObject = new();
    private readonly string _baseAddress;
    private readonly int _port;
    private readonly string _model;

    private OllamaService(string baseAddress, int port, string model)
    {
        _baseAddress = baseAddress;
        _port = port;
        _model = model;
    }

    public static IOllamaService GetInstance(string baseAddress, int port, string model)
    {
        if (_instance != null) return _instance;
        lock (LockObject)
        {
            _instance ??= new OllamaService(baseAddress, port, model);
        }
        return _instance;
    }

    public bool IsOllamaServerRunning()
    {
        try
        {
            using var client = new TcpClient(_baseAddress, _port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
    public void StartOllamaServer()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ollama",
            Arguments = "serve",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(startInfo);
    }
    public async Task<string> SendChatToOllama()
    {
        var ollamaBaseAddress = $"http://{_baseAddress}:{_port}";
        using var httpClient = new HttpClient { BaseAddress = new Uri(ollamaBaseAddress) };

        var payload = new
        {
            model = _model,
            messages = _messages,
            stream = false
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var response = await httpClient.PostAsync("/api/chat",
            new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Error from ollama server: {response.StatusCode}");
            return $"Error from ollama server: {response.StatusCode}";
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(jsonResponse);
        if (document.RootElement.TryGetProperty("message", out var messageObj) &&
            messageObj.TryGetProperty("content", out var content))
        {
            return content.GetString() ?? "<Empty answer>";
        }

        return "Unexpected respond from Ollama-server.";
    }
    public void AddMessage(ChatMessage message) => _messages.Add(message);
    public void ClearChatMessages()
    {
        _messages.Clear();
        _logger.LogInformation("Chat history cleared.");
    }
    public void Reset()
    {
        ClearChatMessages();
        AddMessage(new ChatMessage("system", "Forget all previous conversations. Start a new session."));
        _logger.LogInformation("Chat history cleared and new session initiated.");
    }
    public void ShowInstalledModels()
    {
        try
        {
            var result = ShellService.Default.StartInteractiveProcess("ollama", "list");
            Console.WriteLine("Installed Models:");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing ollama list: {ex.Message}");
        }
    }
    public void ShowModelInfo(string model)
    {
        try
        {
            var result = ShellService.Default.StartInteractiveProcess("ollama", $"show {model}");
            ConsoleService.Writer.WriteDescription(model, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing ollama list: {ex.Message}");
        }
    }
    public bool RemoveModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            _logger.LogWarning("Model name cannot be empty.");
            return false;
        }
        try
        {
            var result = ShellService.Default.StartInteractiveProcess("ollama", "remove gemma3");
            Console.WriteLine("Remove Result:");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while removing model '{modelName}': {ex.Message}");
            return false;
        }
        return true;
    }
    public bool DownloadModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            _logger.LogWarning("Model name cannot be empty.");
            return false;
        }
        try
        {
            var result = ShellService.Default.StartInteractiveProcess("ollama", "download DeepSeekCoder");
            Console.WriteLine("Download Result:");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while downloading model '{modelName}': {ex.Message}");
            return false;
        }
        return true;
    }
}