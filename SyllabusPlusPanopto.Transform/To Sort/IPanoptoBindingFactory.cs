using System.ServiceModel;

public interface IPanoptoBindingFactory
{
    BasicHttpBinding CreateBinding();
}