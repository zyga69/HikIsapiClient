namespace NetDaemonApps.Lib.HikIsapiClient.HikIsapiClient.ExtensionMethods
{
    public static class ILoggerExtensions
    {
        public static Serilog.ILogger ForContext(this Serilog.ILogger logger, string sourceContext)
        {
            return logger.ForContext("SourceContext", sourceContext);
        }
    }
}