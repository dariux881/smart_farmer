using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks
{

    public abstract class FarmerPlan : IFarmerPlan
    {
        protected FarmerPlan()
        {
            ID = Misc.Extensions.RandomString(10);
            Name = Misc.Extensions.RandomString(10);
            EditableSteps = new List<IFarmerPlanStep>();
        }

        public FarmerPlan(string id, string name) 
            : this() 
        {
            ID = id;
            Name = name;
        }

        public string ID { get; private set; }
        public string Name { get; private set; }

        public int Priority { get; set; }
        public DateTime? ValidFromDt { get; set; }
        public DateTime? ValidToDt { get; set; }

        public string CronSchedule { get; set; }

        [JsonIgnore]
        public List<IFarmerPlanStep> EditableSteps { get; protected init; }
        public IReadOnlyList<string> StepIds => EditableSteps.Select(x => x.ID).ToList().AsReadOnly();
        [JsonIgnore]
        public IReadOnlyList<IFarmerPlanStep> Steps => EditableSteps.AsReadOnly();

        public bool IsInProgress { get; private set; }
        public Exception LastException { get; private set; }

        public async Task Execute(CancellationToken token)
        {
            // resetting last exception, related to previous executions
            LastException = null;
            IsInProgress = true;

            // starting new plan execution
            SmartFarmerLog.Information("starting plan \"" + Name + "\"");
            
            try
            {
                foreach (var step in Steps)
                {
                    await step.Execute(null, token);
                }
            }
            catch (TaskCanceledException taskCanceled)
            {
                LastException = taskCanceled;
                SmartFarmerLog.Exception(taskCanceled);
            }
            catch (Exception ex)
            {
                LastException = ex;
                SmartFarmerLog.Exception(ex);
            }
            finally
            {
                IsInProgress = false;
                SmartFarmerLog.Information("stopping plan \"" + Name + "\"");
            }
        }
    }
}
