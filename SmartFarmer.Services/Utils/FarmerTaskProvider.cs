using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SmartFarmer.Exceptions;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public static class FarmerTaskProvider
{
    private static ConcurrentDictionary<string, IFarmerTask> _resolvedMappings;
    private static ConcurrentDictionary<string, Func<IFarmerTask>> _customMappings;

    private static Assembly[] _loadedAssemblies;

    static FarmerTaskProvider()
    {
        _resolvedMappings = new ConcurrentDictionary<string, IFarmerTask>();
        _customMappings = new ConcurrentDictionary<string, Func<IFarmerTask>>();

        LoadAssembliesFromFolder();
    }

    public static Assembly[] AvailableAssemblies => 
        _loadedAssemblies ?? 
        AppDomain.CurrentDomain.GetAssemblies();

    public static void ConfigureMapping<T>(Func<T> initializer) 
        where T : IFarmerTask
    {
        var key = typeof(T).FullName;

        _customMappings.TryAdd(key, initializer as Func<IFarmerTask>);
    }

    public static IFarmerTask GetTaskDelegateByClassFullName(
        string taskTypeFullName, 
        string[] excludedNamespaces = null,
        string[] assemblyNames = null)
    {
        return GetTaskDelegateByType(taskTypeFullName, false, excludedNamespaces, assemblyNames);
    }

    public static IFarmerTask GetTaskDelegateByInterfaceFullName(
        string taskTypeFullName, 
        string[] excludedNamespaces = null,
        string[] assemblyNames = null)
    {
        return GetTaskDelegateByType(taskTypeFullName, true, excludedNamespaces, assemblyNames);
    }

    /// <summary>
    /// Locates all the executors of a given task. Returns the first found implementor.
    /// </summary>
    /// <param name="taskType">The interface of the specific task.</param>
    /// <param name="excludedNamespaces">Optional namespaces to be excluded.</param>
    /// <param name="assemblyNames">Optional assembly names.</param>
    public static IFarmerTask GetTaskDelegateByType(
        Type taskType, 
        string[] excludedNamespaces = null,
        string[] assemblyNames = null)
    {
        if (taskType == null) throw new ArgumentNullException(nameof(taskType));

        // limiting to known task
        if (!taskType.GetInterfaces().Contains(typeof(IFarmerTask)))
        {
            throw new InvalidTaskException();
        }

        return GetTaskDelegateByType(taskType.FullName, taskType.IsInterface, excludedNamespaces, assemblyNames);
    }

    private static IFarmerTask GetTaskDelegateByType(
        string taskTypeFullName, 
        bool isInterface,
        string[] excludedNamespaces = null,
        string[] assemblyNames = null)
    {
        if (_resolvedMappings.TryGetValue(taskTypeFullName, out var resolved))
        {
            return resolved;
        }

        if (_customMappings.TryGetValue(taskTypeFullName, out var activator))
        {
            var customTask = activator.Invoke();
            if (customTask != null)
            {
                _resolvedMappings.TryAdd(taskTypeFullName, customTask);
            }
            
            return customTask;
        }

        var assemblies = 
            AvailableAssemblies
                .Where(x => assemblyNames == null || assemblyNames.Contains(x.FullName))
                .ToArray();

        var taskInstance = GetTaskDelegateByTypeCore(
            assemblies, 
            GetTypeByFullName(taskTypeFullName), 
            isInterface,
            excludedNamespaces);

        if (taskInstance != null)
        {
            _resolvedMappings.TryAdd(taskTypeFullName, taskInstance);
        }

        return taskInstance;
    }

    /// <summary>
    /// Returns the first found implementor of the given task type.
    /// </summary>
    /// <param name="assemblies">The set of assemblies.</param>
    /// <param name="taskType">The interface of the specific task.</param>
    /// <param name="excludedNamespaces">Optional namespaces to be excluded.</param>
    private static IFarmerTask GetTaskDelegateByTypeCore(
        Assembly[] assemblies, 
        Type taskType, 
        bool isInterface,
        string[] excludedNamespaces = null)
    {
        var task = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p =>
                p.IsClass && 
                !p.IsAbstract &&
                (
                    !isInterface || 
                    (
                        p.GetInterfaces().Any(x => x.GUID == taskType.GUID)// &&
                        //taskType.IsAssignableFrom(p)
                    )
                ) && 
                (
                    excludedNamespaces == null || 
                    !excludedNamespaces.Any(n => n == p.Namespace)
                ))
            .FirstOrDefault();

        if (task == null)
        {
            // not found task
            throw new TaskNotFoundException(null, new Exception("implementation of " + taskType.FullName + " has not been found"));
        }

        var taskInstance = Activator.CreateInstance(task) as IFarmerTask;

        if (taskInstance == null)
        {
            // task initialization failure
            throw new TaskInitializationException();
        }

        return taskInstance;
    }

    ///
    private static Type GetTypeByFullName(string taskTypeFullName)
    {
        foreach (var assembly in AvailableAssemblies)
        {
            Type t = assembly.GetType(taskTypeFullName, false);
            if (t != null)
                return t;
        }

        throw new ArgumentException(
            "Type " + taskTypeFullName + " doesn't exist in the current app domain");
    }

    /// <summary>
    /// Load assemblies from folder to include all assemblies in current domain. 
    /// By default, not used assemblies are not loaded in current domain
    /// </summary>
    private static void LoadAssembliesFromFolder()
    {
        string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var extensionName = "dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            extensionName = "so";
        }

        try
        {
            var allAssemblies = new List<Assembly>();
            foreach (string assembly in Directory.GetFiles(path, "*." + extensionName))
            {
                allAssemblies.Add(Assembly.LoadFile(assembly));
            }

            _loadedAssemblies = allAssemblies.ToArray();
        }
        catch (Exception)
        {
            return;
        }
    }

}
