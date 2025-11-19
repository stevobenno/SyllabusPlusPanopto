using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Transform.Domain.Settings;
using System;
using System.ServiceModel;
using System.Xml;

public class PanoptoBindingFactory : IPanoptoBindingFactory
{
    private readonly PanoptoBindingOptions _opts;

    public PanoptoBindingFactory(IOptions<PanoptoSettings> settings)
    {
        _opts = settings.Value.Binding;
    }

    public BasicHttpBinding CreateBinding()
    {
        var b = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
        {
            SendTimeout = TimeSpan.FromSeconds(_opts.SendTimeoutSeconds),
            OpenTimeout = TimeSpan.FromSeconds(_opts.OpenTimeoutSeconds),
            ReceiveTimeout = TimeSpan.FromMinutes(_opts.ReceiveTimeoutMinutes),
            CloseTimeout = TimeSpan.FromSeconds(_opts.CloseTimeoutSeconds),

            MaxReceivedMessageSize = _opts.MaxReceivedMessageSize,
            MaxBufferSize = (int)_opts.MaxReceivedMessageSize,
            MaxBufferPoolSize = _opts.MaxReceivedMessageSize,

            MessageEncoding = WSMessageEncoding.Text,
            TransferMode = TransferMode.Buffered,
            TextEncoding = System.Text.Encoding.UTF8,
            UseDefaultWebProxy = true
        };

        b.ReaderQuotas = new XmlDictionaryReaderQuotas
        {
            MaxStringContentLength = _opts.MaxStringContentLength,
            MaxArrayLength = _opts.MaxArrayLength
        };

        // These mirror the XML you had
        b.Security.Mode = BasicHttpSecurityMode.Transport;
        b.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
        b.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

        return b;
    }
}
