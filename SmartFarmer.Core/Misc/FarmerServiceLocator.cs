using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Utils;

namespace SmartFarmer.Misc;

public class FarmerServiceLocator
{
    private static bool initialized;
    private static ConcurrentDictionary<string, object> _registry;
    private static ConcurrentDictionary<string, Func<object>> _registryFunctions;
    private const string KEY_SEPARATOR = "_";
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

    public static void MapService<T>(Func<T> activator, IFarmerService fService = null)
    {
        string key = BuildKey(typeof(T), fService);

        MapServiceCore<T>(key, activator);
    }

    public static void RemoveService<T>(IFarmerService fService = null)
    {
        string key = BuildKey(typeof(T), fService);

        if (_registryFunctions.TryRemove(key, out var func))
        {
            return;
        }

        if (_registry.TryRemove(key, out var service))
        {
            if (service is IDisposable disp)
            {
                disp.Dispose();
            }
            return;
        }
    }

    public static object GetServiceByFullName(
        string typeFullName, 
        string fServiceId = null, 
        bool required = true)
    {
        var service = GetServiceCore(typeFullName, fServiceId);
        if (service != null)
        {
            return service;
        }

        if (required) throw new InvalidOperationException("service " + typeFullName + " not found but is required");

        return null;        
    }

    public static T GetService<T>(bool required, string fServiceId)
    {
        var service = GetServiceCore(typeof(T).FullName, fServiceId);
        if (service != null)
        {
            return (T)service;
        }

        if (required) throw new InvalidOperationException("service " + typeof(T).FullName + " not found but is required");

        return default(T);
    }

    public static T GetService<T>(bool required = false, IFarmerService fService = null)
    {
        return GetService<T>(required, fService?.ID);
    }

    public static object GetServiceByType(Type t, bool required, string fServiceId)
    {
        var service = GetServiceCore(t.FullName, fServiceId);
        if (service != null)
        {
            return service;
        }

        if (required) throw new InvalidOperationException("service " + t.FullName + " not found but is required");

        return null;
    }

    public static object GetServiceByType(Type t, bool required = false, IFarmerService fService = null)
    {
        return GetServiceByType(t, required, fService?.ID);
    }

    private static void MapServiceCore<T>(string key, Func<T> activator)
    {
        initializationSem.Wait();

        try{
            _registryFunctions.TryAdd(key, activator as Func<object>);
            //_registry.TryAdd(key, activator());
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

    private static string BuildKey(Type t, IFarmerService fService)
    {
        return BuildKey(t, fService?.ID);
    }

    private static string BuildKey(Type t, string fServiceId)
    {
        return BuildKey(t.FullName, fServiceId);
    }

    private static string BuildKey(string typeFullName, string fServiceId)
    {
        if (fServiceId != null)
        {
            typeFullName = fServiceId + KEY_SEPARATOR + typeFullName;
        }

        return typeFullName;
    }

    private static object GetServiceCore(string typeFullName, string fServiceId = null)
    {
        var key = BuildKey(typeFullName, fServiceId);

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