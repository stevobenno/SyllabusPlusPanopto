using System;
using System.ServiceModel;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public static class PanoptoEndpointBuilder
{
    public static EndpointAddress BuildEndpoint(string baseUrl, string relativePath)
    {
        var baseUri = new Uri(baseUrl);
        var fullUri = new Uri(baseUri, relativePath);
        return new EndpointAddress(fullUri);
    }
}
