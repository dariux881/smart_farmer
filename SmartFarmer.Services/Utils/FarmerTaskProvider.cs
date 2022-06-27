using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        public static IFarmerTask GetTaskDelegateByType(Type taskType)
        {
            if (_resolvedMappings.TryGetValue(taskType, out var resolved))
            {
                return resolved;
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    p.GetInterfaces().Contains(taskType) &&
                    taskType.IsAssignableFrom(p) && 
                    p.IsClass && 
                    !p.IsAbstract);
        }
    }
}
