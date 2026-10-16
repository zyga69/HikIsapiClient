namespace Zyga.HikIsapiClient.ExtensionMethods;

/// <summary>
/// Serilog <see cref="Serilog.ILogger"/> helpers used internally for per-device logging context.
/// </summary>
public static class ILoggerExtensions
{
    /// <summary>
    /// Enriches the logger with a <c>SourceContext</c> property set to the given value.
    /// </summary>
    /// <param name="logger">The logger to enrich.</param>
    /// <param name="sourceContext">The source context value, typically identifying the device.</param>
    /// <returns>A logger scoped to the given source context.</returns>
    public static Serilog.ILogger ForContext(this Serilog.ILogger logger, string sourceContext)
    {
        return logger.ForContext("SourceContext", sourceContext);
    }
}
