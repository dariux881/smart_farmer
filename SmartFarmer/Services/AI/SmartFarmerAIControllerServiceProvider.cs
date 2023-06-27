using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SmartFarmer.AI;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerServiceProvider : ISmartFarmerAIControllerServiceProvider
{
    private readonly ConcurrentDictionary<string, ISmartFarmerAIModule> _aiModules;
    private static Assembly[] _loadedAssemblies;

    public SmartFarmerAIControllerServiceProvider()
    {
        _aiModules = new ConcurrentDictionary<string, ISmartFarmerAIModule>();

        LoadAssembliesFromFolder();
        GatherAIModules();
    }

    public ISmartFarmerAIPlantModule GetAIModuleByPlant(FarmerPlantInstance plant)
    {
        if (_aiModules.TryGetValue(plant.ID, out var specificModule) && specificModule is ISmartFarmerAIPlantModule)
        {
            return specificModule as ISmartFarmerAIPlantModule;
        }

        if (_aiModules.TryGetValue(plant.PlantKindID, out var genericModule) && genericModule is ISmartFarmerAIPlantModule)
        {
            return genericModule as ISmartFarmerAIPlantModule;
        }

        if (_aiModules.TryGetValue(plant.Plant.BotanicalName, out genericModule) && genericModule is ISmartFarmerAIPlantModule)
        {
            return genericModule as ISmartFarmerAIPlantModule;
        }
        
        return null;
    }

    public ISmartFarmerAITaskModule GetAITaskModuleByTask(string taskInterfaceFullName)
    {
        
        if (_aiModules.TryGetValue(taskInterfaceFullName, out var genericModule) && genericModule is ISmartFarmerAITaskModule)
        {
            return genericModule as ISmartFarmerAITaskModule;
        }
        
        return null;
    }

    private void GatherAIModules()
    {
        var assemblies = _loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();
        var plantType = typeof(ISmartFarmerAIPlantModule);
        var taskType = typeof(ISmartFarmerAITaskModule);

        var types = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p =>
                p.IsClass && 
                !p.IsAbstract &&
                (
                    p.GetInterfaces().Any(x => x.GUID == plantType.GUID) ||
                    p.GetInterfaces().Any(x => x.GUID == taskType.GUID)
                ));
        
        foreach (var type in types)
        {
            var moduleInstance = Activator.CreateInstance(type) as ISmartFarmerAIPlantModule;

            if (moduleInstance is ISmartFarmerAIPlantModule plantModule)
            {
                AddPlantAIModule(plantModule);
            }
            else if (moduleInstance is ISmartFarmerAITaskModule taskModule)
            {
                AddTaskAIModule(taskModule);
            }
            else
            {
                // AI Module initialization failure
                SmartFarmerLog.Error($"failing initialization of {type.FullName}");
            }
        }

        SmartFarmerLog.Information($"Found {_aiModules.Count} AI modules");
    }

    private void AddTaskAIModule(ISmartFarmerAITaskModule taskModule)
    {
        if (string.IsNullOrEmpty(taskModule.TaskInterfaceFullName))
        {
            SmartFarmerLog.Error($"task module does not specify Task Interface FullName");
            return;
        }

        _aiModules.TryAdd(taskModule.TaskInterfaceFullName, taskModule);
    }

    private void AddPlantAIModule(ISmartFarmerAIPlantModule plantModule)
    {
    
        if (!string.IsNullOrEmpty(plantModule.PlantId))
        {
            _aiModules.TryAdd(plantModule.PlantId, plantModule);
        }

        if (!string.IsNullOrEmpty(plantModule.PlantBotanicalName))
        {
            _aiModules.TryAdd(plantModule.PlantBotanicalName, plantModule);
        }
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