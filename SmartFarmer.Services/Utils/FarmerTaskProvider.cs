using System;
using System.Collections.Concurrent;
using System.Linq;
using SmartFarmer.Exceptions;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils
{
    public static class FarmerTaskProvider
    {
        private static ConcurrentDictionary<Type, IFarmerTask> _resolvedMappings;

        static FarmerTaskProvider()
        {
            _resolvedMappings = new ConcurrentDictionary<Type, IFarmerTask>();
        }

        /// <summary>
        /// Locates all the executors of a given task. Returns the first found implementor.
        /// </summary>
        /// <param name="taskType">The interface of the specific task.</param>
        public static IFarmerTask GetTaskDelegateByType(Type taskType)
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

            // getting classes that implement the given taskType (interface)
            var task = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    p.GetInterfaces().Contains(taskType) &&
                    taskType.IsAssignableFrom(p) && 
                    p.IsClass && 
                    !p.IsAbstract)
                .FirstOrDefault();

            if (task == null)
            {
                // not found task
                throw new TaskNotFoundException();
            }

            var taskInstance = Activator.CreateInstance(task) as IFarmerTask;

            if (taskInstance == null)
            {
                // task initialization failure
                throw new TaskInitializationException();
            }

            _resolvedMappings.TryAdd(taskType, taskInstance);

            return taskInstance;
        }
    }
}
