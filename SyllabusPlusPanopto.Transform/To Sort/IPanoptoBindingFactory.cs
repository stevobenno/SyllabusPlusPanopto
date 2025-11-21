using System.ServiceModel;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public interface IPanoptoBindingFactory
{
    BasicHttpBinding CreateBinding();
}



