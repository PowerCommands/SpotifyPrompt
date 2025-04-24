using PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.DomainObjects;

namespace PainKiller.CommandPrompt.CoreLib.Modules.OllamaModule.Contracts;

public interface IOllamaService
{
    bool IsOllamaServerRunning();
    void StartOllamaServer();
    Task<string> SendChatToOllama();
    void AddMessage(ChatMessage message);
    void ClearChatMessages();
    void Reset();
    void ShowInstalledModels();
    bool RemoveModel(string modelName);
    bool DownloadModel(string modelName);
    void ShowModelInfo(string model);
}