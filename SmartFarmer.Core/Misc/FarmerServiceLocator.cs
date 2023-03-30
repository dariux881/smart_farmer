using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace SmartFarmer.Misc;

public class FarmerServiceLocator
{
    private static bool initialized;
    private static ConcurrentDictionary<string, object> _registry;
    // private static ConcurrentDictionary<string, Func<object>> _registryFunctions;
    private static SemaphoreSlim initializationSem;

    static FarmerServiceLocator() {
        initializationSem = new SemaphoreSlim(1);
    }

    public static void InitializeServiceLocator(bool autoDiscovery = false)
    {
        initializationSem.Wait();

        if (initialized) {
            initializationSem.Release();
            return;
        }

        if (_registry == null) _registry = new ConcurrentDictionary<string, object>();
        // if (_registryFunctions == null) _registryFunctions = new ConcurrentDictionary<string, Func<object>>();

        // if (autoDiscovery)
        // {
        //     AutoDiscovery();
        // }

        initialized = true;
        initializationSem.Release();
    }

    public static void ReleaseServices()
    {
        initializationSem.Wait();

        var disposableInstances = 
            _registry
                .Values
                    .Where(x => x is IDisposable)
                    .Select(x => x as IDisposable)
                    .ToList();

        disposableInstances.ForEach(x => x.Dispose());

        initializationSem.Release();
    }

    public static void AddInstance<T, U>(U serviceInstance) 
        where U : class, T
    {
        initializationSem.Wait();

        var requestedType = typeof(T);
        if (!requestedType.IsInterface) {
            initializationSem.Release();
            throw new InvalidOperationException(requestedType.FullName + " must be an interface");
        }

        _registry.TryAdd(requestedType.FullName, serviceInstance);
        initializationSem.Release();
    }

    public static void MapService<T>(Func<T> activator)
    {
        initializationSem.Wait();

        try{
            _registry.TryAdd(typeof(T).FullName, activator());
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        finally
        {
            initializationSem.Release();
        }
    }

    public static T GetService<T>(bool required = false)
    {
        var key = typeof(T).FullName;
        if (_registry.TryGetValue(key, out var serviceImplementor))
        {
            return (T)serviceImplementor;
        }

        if (required) throw new InvalidOperationException("service " + key + " not found but is required");

        return default(T);
    }

    private static void AutoDiscovery()
    {
        initializationSem.Wait();
        //throw new NotImplementedException();
        initializationSem.Release();
    }
}