
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    public string TaskClassFullName { get; set; }

    public TimeSpan Delay { get; set; }

    public object[] BuildParameters { get; set; }

    [JsonIgnore]
    public FarmerPlan Plan { get; set;}

    [JsonIgnore]
    public string PlanId { get; set; }

    [JsonIgnore]
    public bool IsInProgress { get; set; }

    [JsonIgnore]
    public Exception LastException { get; set; }

    public string ID { get; set; }

    public Task Execute(object[] parameters, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}