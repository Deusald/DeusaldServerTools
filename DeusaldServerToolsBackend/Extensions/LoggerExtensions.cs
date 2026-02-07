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

using Microsoft.Extensions.Logging;

namespace DeusaldServerToolsBackend;

public static class LoggerExtensions
{
    public static NLog.LogLevel ToNLog(this DeusaldSharp.LogLevel level)
    {
        return level switch
        {
            DeusaldSharp.LogLevel.Off   => NLog.LogLevel.Off,
            DeusaldSharp.LogLevel.Trace => NLog.LogLevel.Trace,
            DeusaldSharp.LogLevel.Debug => NLog.LogLevel.Debug,
            DeusaldSharp.LogLevel.Info  => NLog.LogLevel.Info,
            DeusaldSharp.LogLevel.Warn  => NLog.LogLevel.Warn,
            DeusaldSharp.LogLevel.Error => NLog.LogLevel.Error,
            DeusaldSharp.LogLevel.Fatal => NLog.LogLevel.Fatal,
            _                           => NLog.LogLevel.Off
        };
    }

    public static LogLevel ToMicrosoftLog(this DeusaldSharp.LogLevel level)
    {
        return level switch
        {
            DeusaldSharp.LogLevel.Off   => LogLevel.None,
            DeusaldSharp.LogLevel.Trace => LogLevel.Trace,
            DeusaldSharp.LogLevel.Debug => LogLevel.Debug,
            DeusaldSharp.LogLevel.Info  => LogLevel.Information,
            DeusaldSharp.LogLevel.Warn  => LogLevel.Warning,
            DeusaldSharp.LogLevel.Error => LogLevel.Error,
            DeusaldSharp.LogLevel.Fatal => LogLevel.Critical,
            _                           => LogLevel.None
        };
    }
}