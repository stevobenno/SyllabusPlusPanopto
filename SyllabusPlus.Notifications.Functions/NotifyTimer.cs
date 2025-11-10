
using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SyllabusPlus.Notifications.Functions;

public class NotifyTimer
{
    private readonly ILogger<NotifyTimer> _log;
    public NotifyTimer(ILogger<NotifyTimer> log) => _log = log;

    [Function("NotifyTimer")]
    public void Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        _log.LogInformation("Notifications tick at: {time}", DateTime.UtcNow);
    }
}
