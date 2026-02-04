using DeusaldServerToolsClient;
using DeusaldSharp;

namespace DeusaldServerToolsBackend;

public class ServerException : Exception
{
    public ErrorCode ErrorCode       { get; }
    public LogLevel  ServerLogLevel  { get; }
    public string    MessageToClient { get; }
    public Guid      ErrorId         { get; }

    public ServerException(ErrorCode errorCode, LogLevel serverLogLevel, string messageToClient, string message) : base(message)
    {
        ErrorCode       = errorCode;
        ServerLogLevel  = serverLogLevel;
        MessageToClient = messageToClient;
        ErrorId         = ServerLogLevel == LogLevel.Off ? Guid.Empty : Guid.NewGuid();
    }

    public ServerException(ErrorCode errorCode, LogLevel serverLogLevel, string messageToClientAndServer) : base(messageToClientAndServer)
    {
        ErrorCode       = errorCode;
        ServerLogLevel  = serverLogLevel;
        MessageToClient = messageToClientAndServer;
        ErrorId         = ServerLogLevel == LogLevel.Off ? Guid.Empty : Guid.NewGuid();
    }

    public ServerException(ErrorCode errorCode, string messageToClient) : base("")
    {
        ErrorCode       = errorCode;
        ServerLogLevel  = LogLevel.Off;
        MessageToClient = messageToClient;
        ErrorId         = Guid.Empty;
    }
}