﻿using System.Text;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.LayoutRenderers;
using NLog.Targets;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NAPS2;

public static class NLogConfig
{
    public static ILogger CreateLogger(Func<bool> enableDebugLogging)
    {
        LayoutRenderer.Register<CustomExceptionLayoutRenderer>("exception");
        var config = new LoggingConfiguration();
        var target = new FileTarget
        {
            FileName = Path.Combine(Paths.AppData, "errorlog.txt"),
            Layout = "${longdate} ${processid} ${message} ${exception:format=tostring}",
            ArchiveAboveSize = 100000,
            MaxArchiveFiles = 5
        };
        var debugTarget = new FileTarget
        {
            FileName = Path.Combine(Paths.AppData, "debuglog.txt"),
            Layout = "${longdate} ${processid} ${message} ${exception:format=tostring}",
            ArchiveAboveSize = 100000,
            MaxArchiveFiles = 1
        };
        config.AddTarget("errorlogfile", target);
        config.AddTarget("debuglogfile", debugTarget);
        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));
        var debugRule = new LoggingRule("*", LogLevel.Debug, debugTarget);
        debugRule.Filters.Add(new WhenMethodFilter(_ => enableDebugLogging() ? FilterResult.Log : FilterResult.Ignore));
        config.LoggingRules.Add(debugRule);
        LogManager.Configuration = config;
        return new NLogLoggerFactory().CreateLogger("NAPS2");
    }

    private class CustomExceptionLayoutRenderer : ExceptionLayoutRenderer
    {
        protected override void AppendToString(StringBuilder sb, Exception ex)
        {
            // Note we don't want to use the AppendDemystified() helper
            // https://github.com/benaadams/Ben.Demystifier/issues/85
            sb.Append(ex.Demystify());
        }
    }
}