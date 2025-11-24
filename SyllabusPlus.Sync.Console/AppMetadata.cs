using System;
using System.IO;
using System.Reflection;

namespace SyllabusPlus.Sync.Console;

static class AppMetadata
{
    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public static string BuildDate =>
        File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location)
            .ToString("yyyy-MM-dd HH:mm:ss");

    public static string EnvironmentName =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? "Production";
}
