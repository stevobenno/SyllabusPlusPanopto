using System.ServiceModel;

namespace SyllabusPlusPanopto.Transform.To_Sort;

public interface IPanoptoBindingFactory
{
    BasicHttpBinding CreateBinding();
}
