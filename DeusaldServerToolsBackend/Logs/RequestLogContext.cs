using System.Collections.Concurrent;
using NLog;

namespace DeusaldServerToolsBackend;

public sealed class RequestLogContext : IRequestLogContext, IDisposable
{
    private readonly ConcurrentDictionary<string, IDisposable> _Scopes = new(StringComparer.OrdinalIgnoreCase);
    
    public void SetUserId(string userId)
    {
        Set(BaseNLogConsts.USER_ID_KEY, userId);
    }

    public void SetGameId(string gameId)
    {
        Set(BaseNLogConsts.GAME_ID_KEY, gameId);
    }

    public void SetErrorId(string errorId)
    {
        Set(BaseNLogConsts.ERROR_ID, errorId);
    }
    
    public void SetErrorType(string errorType)
    {
        Set(BaseNLogConsts.ERROR_TYPE, errorType);
    }
    
    public void Dispose()
    {
        foreach (KeyValuePair<string, IDisposable> kv in _Scopes)
        {
            try { kv.Value.Dispose(); } catch { /* ignore */ }
        }
        _Scopes.Clear();
    }
    
    private void Set(string key, object? value)
    {
        IDisposable newScope = ScopeContext.PushProperty(key, value);

        _Scopes.AddOrUpdate(key, newScope, (_, existing) =>
        {
            existing.Dispose();
            return newScope;
        });
    }
}