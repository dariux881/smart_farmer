using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace SmartFarmer.Misc;

public class FarmerServiceLocator
{
    private static bool initialized;
    private static ConcurrentDictionary<string, object> _registry;
    private static ConcurrentDictionary<string, Func<object>> _registryFunctions;
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
        if (_registryFunctions == null) _registryFunctions = new ConcurrentDictionary<string, Func<object>>();

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
            _registryFunctions.TryAdd(typeof(T).FullName, activator as Func<object>);
            //_registry.TryAdd(typeof(T).FullName, activator());
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
        var service = GetServiceCore(typeof(T));
        if (service != null)
        {
            return (T)service;
        }

        if (required) throw new InvalidOperationException("service " + typeof(T).FullName + " not found but is required");

        return default(T);
    }

    public static object GetServiceByType(Type t, bool required = false)
    {
        var service = GetServiceCore(t);
        if (service != null)
        {
            return service;
        }

        if (required) throw new InvalidOperationException("service " + t.FullName + " not found but is required");

        return null;
    }

    private static object GetServiceCore(Type t)
    {
        var key = t.FullName;

        if (_registry.TryGetValue(key, out var serviceImplementor))
        {
            return serviceImplementor;
        }

        if (_registryFunctions.TryRemove(key, out var serviceActivator))
        {
            var instance = serviceActivator();
            _registry.TryAdd(key, instance);

            return instance;
        }

        return null;
    }

    private static void AutoDiscovery()
    {
        initializationSem.Wait();
        //throw new NotImplementedException();
        initializationSem.Release();
    }
}