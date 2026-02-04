using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace DeusaldServerToolsBackend;

[LayoutRenderer("loki-format")]
[ThreadAgnostic]
public class LokiFormatter : WrapperLayoutRendererBase
{
    protected override string Transform(string log)
    {
        log = log.Replace(Environment.NewLine, "/n");
        log = log.Replace("\"",                "'");
        log = log.Replace("\\",                "/");
        return log;
    }
}