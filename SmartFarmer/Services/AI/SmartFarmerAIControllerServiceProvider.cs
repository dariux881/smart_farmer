using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SmartFarmer.Misc;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerServiceProvider : ISmartFarmerAIControllerServiceProvider
{
    private readonly ConcurrentDictionary<string, ISmartFarmerAIPlantModule> _aiModules;
    private static Assembly[] _loadedAssemblies;

    public SmartFarmerAIControllerServiceProvider()
    {
        _aiModules = new ConcurrentDictionary<string, ISmartFarmerAIPlantModule>();

        LoadAssembliesFromFolder();
        GatherAIModules();
    }

    public ISmartFarmerAIPlantModule GetAIPlantModuleByPlantId(string plantId)
    {
        return _aiModules[plantId];
    }

    private void GatherAIModules()
    {
        var assemblies = _loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();
        var taskType = typeof(ISmartFarmerAIPlantModule);

        var modules = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p =>
                p.IsClass && 
                !p.IsAbstract &&
                (
                    p.GetInterfaces().Any(x => x.GUID == taskType.GUID)// &&
                    //taskType.IsAssignableFrom(p)
                ));
        
        foreach (var module in modules)
        {
            var moduleInstance = Activator.CreateInstance(module) as ISmartFarmerAIPlantModule;

            if (moduleInstance == null)
            {
                // AI Module initialization failure
                SmartFarmerLog.Error($"failing initialization of {module.FullName}");
            }

            _aiModules.TryAdd(moduleInstance.PlantId, moduleInstance);
        }

        SmartFarmerLog.Information($"Found {_aiModules.Count} AI modules");
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