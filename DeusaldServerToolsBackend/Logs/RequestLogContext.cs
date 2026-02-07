// MIT License

// DeusaldServerTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Concurrent;
using JetBrains.Annotations;
using NLog;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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