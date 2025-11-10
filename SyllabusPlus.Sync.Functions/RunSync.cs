
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SyllabusPlus.Sync.Functions;

public class RunSync
{
    private readonly ILogger<RunSync> _log;
    public RunSync(ILogger<RunSync> log) => _log = log;

    [Function("RunSync")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route =  "sync")] HttpRequestData req)
    {
        var dryRun = req.Url.Query.Contains("dryRun=true");
        _log.LogInformation("Sync called. dryRun={dryRun}", dryRun);
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteStringAsync($"Sync started. dryRun={dryRun}");
        return resp;
    }
}
