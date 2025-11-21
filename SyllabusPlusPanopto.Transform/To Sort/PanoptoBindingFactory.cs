using System;
using System.ServiceModel;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public sealed class PanoptoBindingFactory : IPanoptoBindingFactory
    {
        public BasicHttpBinding CreateBinding()
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            // Panopto SOAP limits + behaviour
            binding.MaxReceivedMessageSize = 1024 * 1024 * 50; // 50MB
            binding.MaxBufferSize = 1024 * 1024 * 50;
            binding.MaxBufferPoolSize = 1024 * 1024 * 10;

            // Timeouts
            binding.SendTimeout = TimeSpan.FromMinutes(2);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            binding.OpenTimeout = TimeSpan.FromSeconds(30);
            binding.CloseTimeout = TimeSpan.FromSeconds(30);

            // Allow large/complex SOAP payloads
            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            return binding;
        }
    }
}
