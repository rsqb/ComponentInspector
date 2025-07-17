using System.Reflection;

namespace ComponentInspector.Core.Interfaces;

public interface IComponentInspector
{
    #region Interface methods
    
    void InspectAssembly(Assembly assembly);
    void ListTypes();
    void ListFields();
    void ListProperties();
    void ListMethods();
    void DumpAll();
    
    #endregion
}