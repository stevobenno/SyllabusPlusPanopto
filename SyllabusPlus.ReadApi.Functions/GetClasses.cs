
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SyllabusPlusPanopto.Domain;

namespace SyllabusPlus.ReadApi.Functions;

public class GetClasses
{
    private readonly ILogger<GetClasses> _log;
    public GetClasses(ILogger<GetClasses> log) => _log = log;

    [Function("GetClasses")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "classes")] HttpRequestData req)

    {
        _log.LogInformation("GetClasses called");
        // Placeholder response
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new [] {
            new SpRawRow("DEMO1001", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), "Room 101", "RecorderA", "owner@leeds.ac.uk")
        });
        return resp;
    }
}
