using System.Reflection;
using ComponentInspector.Models;

namespace ComponentInspector.Core.Interfaces;

internal interface IComponentInspector
{
    #region Interface methods
    
    void InspectAssembly(Assembly assembly);
    void ListTypes();
    void ListFields();
    void ListProperties();
    void ListMethods();
    void DumpAll();
    void InvokeMethod(MethodInvocation invocation);
    
    #endregion
}