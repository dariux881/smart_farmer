using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SmartFarmer.Exceptions;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils
{
    public static class FarmerTaskProvider
    {
        private static ConcurrentDictionary<Type, IFarmerTask> _resolvedMappings;
        private static Assembly[] loadedAssemblies;

        static FarmerTaskProvider()
        {
            _resolvedMappings = new ConcurrentDictionary<Type, IFarmerTask>();
            LoadAssembliesFromFolder();
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
    
                loadedAssemblies = allAssemblies.ToArray();
            }
            catch (Exception)
            {
                return;
            }
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
            if (_resolvedMappings.TryGetValue(taskType, out var resolved))
            {
                return resolved;
            }

            // limiting to known task
            if (!taskType.GetInterfaces().Contains(typeof(IFarmerTask)))
            {
                throw new InvalidTaskException();
            }

            var assemblies = 
                (loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies())
                    .Where(x => assemblyNames == null || assemblyNames.Contains(x.FullName))
                    .ToArray();

            var taskInstance = GetTaskDelegateByTypeCore(
                assemblies, 
                taskType, 
                excludedNamespaces);

            if (taskInstance != null)
            {
                _resolvedMappings.TryAdd(taskType, taskInstance);
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
            string[] excludedNamespaces = null)
        {
            var task = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    p.GetInterfaces().Contains(taskType) &&
                    taskType.IsAssignableFrom(p) && 
                    (excludedNamespaces == null || !excludedNamespaces.Any(n => n == p.Namespace)) &&
                    p.IsClass && 
                    !p.IsAbstract)
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
    }
}
