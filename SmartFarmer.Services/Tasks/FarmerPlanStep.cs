using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{
    public class FarmerPlanStep : IFarmerPlanStep
    {
        private IFarmerTask _task;

        [JsonConstructor]
        public FarmerPlanStep(string id, string taskClassFullName, object[] buildParameters)
            : this(id, FarmerTaskProvider.GetTaskDelegateByClassFullName(taskClassFullName), buildParameters)
        {
            
        }

        public FarmerPlanStep(string id, IFarmerTask task, object[] parameters = null)
        {
            ID = id;
            _task = task;

            BuildParameters = parameters;
        }

        public string ID { get; private set; }
        public string TaskClassFullName => _task?.GetType().FullName;
        public object[] BuildParameters { get; private set; }
        public TimeSpan Delay { get; set; }
        public bool IsInProgress { get; set; }
        public Exception LastException { get; set; }

        public async Task Execute(object[] parameters, CancellationToken token)
        {
            if (TaskClassFullName == null) throw new ArgumentNullException(nameof(TaskClassFullName));

            await Task.Delay(Delay, token);

            var toolManager = FarmerToolsManager.Instance;
            var currentlyMountedTool = toolManager.GetCurrentlyMountedTool();

            SmartFarmerLog.Information("preparing task " + TaskClassFullName.GetType().FullName);

            if (_task.RequiredTool != Utils.FarmerTool.None && _task.RequiredTool != currentlyMountedTool)
            {
                // this task requires a tool that is not currently mounted. Mounting tool first. 
                // Exceptions may arise. Exceptions will stop next executions
                SmartFarmerLog.Information("mounting tool " + _task.RequiredTool);
                await toolManager.MountTool(_task.RequiredTool, token);
            }
            else
            {
                var message = 
                    _task.RequiredTool != Utils.FarmerTool.None ?
                        _task.RequiredTool + " already mounted" :
                        "this task does not require any tool";

                SmartFarmerLog.Debug(message);
            }

            await _task.Execute(parameters ?? BuildParameters, token);
        }

    }
}
