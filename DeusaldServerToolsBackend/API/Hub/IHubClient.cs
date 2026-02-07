namespace DeusaldServerToolsBackend;

public interface IHubClient
{
    Task OnMessageFromServerAsync(string msgId, byte[] data);
}