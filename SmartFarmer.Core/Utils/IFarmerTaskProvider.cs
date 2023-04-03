using System;
using System.Reflection;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public interface IFarmerTaskProvider
{
    Assembly[] AvailableAssemblies { get; }

    void ConfigureMapping<T>(Func<T> initializer) where T : IFarmerTask;
    IFarmerTask GetTaskDelegateByClassFullName(string taskTypeFullName, string[] excludedNamespaces = null, string[] assemblyNames = null);
    IFarmerTask GetTaskDelegateByInterfaceFullName(string taskTypeFullName, string[] excludedNamespaces = null, string[] assemblyNames = null);
    IFarmerTask GetTaskDelegateByType(Type taskType, string[] excludedNamespaces = null, string[] assemblyNames = null);
}
