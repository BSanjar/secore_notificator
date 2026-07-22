namespace NotificationWorker.Logging;

/// <summary>
/// Persistent file logging outside the container.
/// Server layout:
///   /secore/mainSystem   — WebApplication1
///   /secore/notificator  — this service (notificator-YYYYMMDD.log)
///   /secore/sheduler     — payment scheduler
/// </summary>
public sealed class SecoreLoggingOptions
{
    public const string SectionName = "SecoreLogging";

    /// <summary>Host folder mounted into the container. Default: /secore/notificator</summary>
    public string RootPath { get; set; } = "/secore/notificator";

    /// <summary>File name prefix. Daily files: {FilePrefix}-20260722.log</summary>
    public string FilePrefix { get; set; } = "notificator";

    /// <summary>How many daily files to keep (null = unlimited).</summary>
    public int? RetainedFileCountLimit { get; set; } = 120;

    /// <summary>Minimum level written to file.</summary>
    public string MinimumLevel { get; set; } = "Information";
}
