using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NotificationWorker.Logging;

public static class SecoreLoggingBootstrap
{
    public const string ServiceName = "notificator";

    public static string ResolveRootPath(IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(SecoreLoggingOptions.SectionName).Get<SecoreLoggingOptions>()
                      ?? new SecoreLoggingOptions();

        var root = (options.RootPath ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            root = environment.IsDevelopment()
                ? Path.Combine(environment.ContentRootPath, "logs", ServiceName)
                : $"/secore/{ServiceName}";
        }
        else if (!Path.IsPathRooted(root))
        {
            root = Path.Combine(environment.ContentRootPath, root);
        }

        Directory.CreateDirectory(root);
        return root;
    }

    public static LoggerConfiguration Configure(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(SecoreLoggingOptions.SectionName).Get<SecoreLoggingOptions>()
                      ?? new SecoreLoggingOptions();

        var root = ResolveRootPath(configuration, environment);
        var prefix = string.IsNullOrWhiteSpace(options.FilePrefix) ? ServiceName : options.FilePrefix.Trim();
        var filePath = Path.Combine(root, $"{prefix}-.log");

        if (!Enum.TryParse<LogEventLevel>(options.MinimumLevel, true, out var minLevel))
            minLevel = LogEventLevel.Information;

        const string outputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        return loggerConfiguration
            .MinimumLevel.Is(minLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", ServiceName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File(
                path: filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: options.RetainedFileCountLimit,
                shared: true,
                encoding: System.Text.Encoding.UTF8,
                outputTemplate: outputTemplate);
    }
}
