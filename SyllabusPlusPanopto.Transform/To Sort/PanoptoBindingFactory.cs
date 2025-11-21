using System;
using System.ServiceModel;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Transform.Domain.Settings;

namespace SyllabusPlusPanopto.Transform.To_Sort;

public static class PanoptoBindingFactory
{
    public static BasicHttpBinding Create(IOptions<PanoptoBindingOptions> optionsAccessor)
    {
        var opts = optionsAccessor.Value;

        var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
        {
            SendTimeout = TimeSpan.FromSeconds(opts.SendTimeoutSeconds),
            OpenTimeout = TimeSpan.FromSeconds(opts.OpenTimeoutSeconds),
            ReceiveTimeout = TimeSpan.FromMinutes(opts.ReceiveTimeoutMinutes),
            CloseTimeout = TimeSpan.FromSeconds(opts.CloseTimeoutSeconds),

            // Hard-coded, sensible defaults based on Panopto’s sample config
            MaxReceivedMessageSize = 20_000_000
        };

        binding.ReaderQuotas.MaxStringContentLength = 20_000_000;
        binding.ReaderQuotas.MaxArrayLength = 20_000_000;

        return binding;
    }
}
