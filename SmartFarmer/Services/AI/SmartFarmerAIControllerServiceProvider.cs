using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SmartFarmer.AI;
using SmartFarmer.AI.Base;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerServiceProvider : ISmartFarmerAIControllerServiceProvider
{
    private readonly ConcurrentDictionary<string, ISmartFarmerAIPlantPlanGenerator> _aiPlantPlanGenerators;
    private readonly ConcurrentDictionary<string, ISmartFarmerAITaskPlanGenerator> _aiTaskPlanGenerators;
    private readonly ConcurrentDictionary<string, ISmartFarmerAIPlantDetector> _aiPlantDetectors;
    private readonly ConcurrentDictionary<string, ISmartFarmerAITaskDetector> _aiTaskDetectors;
    private static Assembly[] _loadedAssemblies;

    public SmartFarmerAIControllerServiceProvider()
    {
        _aiPlantPlanGenerators = new ConcurrentDictionary<string, ISmartFarmerAIPlantPlanGenerator>();
        _aiTaskPlanGenerators = new ConcurrentDictionary<string, ISmartFarmerAITaskPlanGenerator>();
        _aiPlantDetectors = new ConcurrentDictionary<string, ISmartFarmerAIPlantDetector>();
        _aiTaskDetectors = new ConcurrentDictionary<string, ISmartFarmerAITaskDetector>();

        LoadAssembliesFromFolder();
        GatherAIModules();
    }

    public ISmartFarmerAIPlantPlanGenerator GetAIPlantPlanGenerator(FarmerPlantInstance plant)
    {
        ISmartFarmerAIPlantPlanGenerator aiModule;

        if (_aiPlantPlanGenerators.TryGetValue(plant.ID, out aiModule))
        {
            return aiModule;
        }

        if (_aiPlantPlanGenerators.TryGetValue(plant.PlantKindID, out aiModule))
        {
            return aiModule;
        }

        if (_aiPlantPlanGenerators.TryGetValue(plant.Plant.BotanicalName, out aiModule))
        {
            return aiModule;
        }
        
        if (_aiPlantPlanGenerators.TryGetValue("", out aiModule))
        {
            return aiModule;
        }

        return null;
    }

    public ISmartFarmerAIPlantDetector GetAIPlantDetector(FarmerPlantInstance plant)
    {
        ISmartFarmerAIPlantDetector aiModule;

        if (_aiPlantDetectors.TryGetValue(plant.ID, out aiModule))
        {
            return aiModule;
        }

        if (_aiPlantDetectors.TryGetValue(plant.PlantKindID, out aiModule))
        {
            return aiModule;
        }

        if (_aiPlantDetectors.TryGetValue(plant.Plant.BotanicalName, out aiModule))
        {
            return aiModule;
        }
        
        return null;
    }

    public ISmartFarmerAITaskPlanGenerator GetAITaskPlanGenerator(string taskInterfaceFullName)
    {
        
        if (_aiTaskPlanGenerators.TryGetValue(taskInterfaceFullName, out var genericModule))
        {
            return genericModule;
        }
        
        return null;
    }

    public ISmartFarmerAITaskDetector GetAITaskDetector(string taskInterfaceFullName)
    {
        
        if (_aiTaskDetectors.TryGetValue(taskInterfaceFullName, out var genericModule))
        {
            return genericModule;
        }
        
        return null;
    }

    private void GatherAIModules()
    {
        var assemblies = _loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();

        var targetTypeGUIDs = new [] {
            typeof(ISmartFarmerAIPlantPlanGenerator).GUID,
            typeof(ISmartFarmerAITaskPlanGenerator).GUID,
            typeof(ISmartFarmerAIPlantDetector).GUID,
            typeof(ISmartFarmerAITaskDetector).GUID
        };

        var types = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p =>
                p.IsClass && 
                !p.IsAbstract &&
                (
                    p.GetInterfaces().Any(x => targetTypeGUIDs.Contains(x.GUID))
                ));
        
        foreach (var type in types)
        {
            var moduleInstance = Activator.CreateInstance(type) as ISmartFarmerAIModule;

            switch (moduleInstance)
            {
                case ISmartFarmerAIPlantPlanGenerator plantModule:
                    AddPlantAIPlanGeneratorModule(plantModule);
                    break;
                
                case ISmartFarmerAIPlantDetector plantDetector:
                    AddPlantAIDetectorModule(plantDetector);
                    break;

                case ISmartFarmerAITaskPlanGenerator taskModule:
                    AddTaskAIPlanGeneratorModule(taskModule);
                    break;

                case ISmartFarmerAITaskDetector taskDetector:
                    AddTaskAIDetectorModule(taskDetector);
                    break;
                
                default:
                    // AI Module initialization failure
                    SmartFarmerLog.Error($"failing initialization of {type.FullName}");
                    break;
            }
        }

        SmartFarmerLog.Information($"Found {_aiPlantPlanGenerators.Count} AI modules");
    }

    private void AddTaskAIPlanGeneratorModule(ISmartFarmerAITaskPlanGenerator taskModule)
    {
        if (string.IsNullOrEmpty(taskModule.TaskInterfaceFullName))
        {
            SmartFarmerLog.Error($"task module does not specify Task Interface FullName");
            return;
        }

        _aiTaskPlanGenerators.TryAdd(taskModule.TaskInterfaceFullName, taskModule);
    }

    private void AddTaskAIDetectorModule(ISmartFarmerAITaskDetector taskModule)
    {
        if (string.IsNullOrEmpty(taskModule.TaskInterfaceFullName))
        {
            SmartFarmerLog.Error($"task module does not specify Task Interface FullName");
            return;
        }

        _aiTaskDetectors.TryAdd(taskModule.TaskInterfaceFullName, taskModule);
    }

    private void AddPlantAIPlanGeneratorModule(ISmartFarmerAIPlantPlanGenerator plantModule)
    {
        bool addedById = false;
        bool addedByName = false;

        if (!string.IsNullOrEmpty(plantModule.PlantId))
        {
            addedById = _aiPlantPlanGenerators.TryAdd(plantModule.PlantId, plantModule);
        }

        if (!string.IsNullOrEmpty(plantModule.PlantBotanicalName))
        {
            addedByName = _aiPlantPlanGenerators.TryAdd(plantModule.PlantBotanicalName, plantModule);
        }

        if (!addedById && !addedByName && !_aiPlantPlanGenerators.ContainsKey(string.Empty))
        {
            _aiPlantPlanGenerators.TryAdd(string.Empty, plantModule);
        }
    }

    private void AddPlantAIDetectorModule(ISmartFarmerAIPlantDetector plantModule)
    {
    
        if (!string.IsNullOrEmpty(plantModule.PlantId))
        {
            _aiPlantDetectors.TryAdd(plantModule.PlantId, plantModule);
        }

        if (!string.IsNullOrEmpty(plantModule.PlantBotanicalName))
        {
            _aiPlantDetectors.TryAdd(plantModule.PlantBotanicalName, plantModule);
        }
    }
    
    /// <summary>
    /// Load assemblies from folder to include all assemblies in current domain. 
    /// By default, not used assemblies are not loaded in current domain
    /// </summary>
    private void LoadAssembliesFromFolder()
    {
        _loadedAssemblies = GetAssemblies().ToArray();
    }

    private IEnumerable<Assembly> GetAssemblies()
    {
        var list = new List<string>();
        var stack = new Stack<Assembly>();

        stack.Push(Assembly.GetEntryAssembly());

        do
        {
            var asm = stack.Pop();

            yield return asm;

            foreach (var reference in asm.GetReferencedAssemblies())
                if (!list.Contains(reference.FullName))
                {
                    stack.Push(Assembly.Load(reference));
                    list.Add(reference.FullName);
                }

        }
        while (stack.Count > 0);
    }
}