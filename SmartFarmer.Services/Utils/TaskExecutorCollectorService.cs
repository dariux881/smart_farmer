using System;
using System.Collections.Generic;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils
{
    [Obsolete]
    public class TaskExecutorCollectorService
    {
        public Dictionary<FarmerTool, IFarmerTask> TaskMapper;


        private static readonly Lazy<TaskExecutorCollectorService> _instance =
           new Lazy<TaskExecutorCollectorService>(() => new TaskExecutorCollectorService());

        public static TaskExecutorCollectorService Instance => _instance.Value;

        public TaskExecutorCollectorService()
        {
            TaskMapper = new Dictionary<FarmerTool, IFarmerTask>();
        }

        public IFarmerTask? GetExecutorByTool(FarmerTool tool)
        {
            if (TaskMapper.TryGetValue(tool, out var task))
            {
                return task;
            }

            return null;
        }
    }
}
