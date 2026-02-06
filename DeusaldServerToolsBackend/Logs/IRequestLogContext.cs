namespace DeusaldServerToolsBackend;

public interface IRequestLogContext
{
    public void SetUserId(string userId);
    public void SetGameId(string gameId);
    public void SetErrorId(string errorId);
    public void SetErrorType(string errorType);
}