
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json; don't use this namespace
using SmartFarmer.Tasks.Generic;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using SmartFarmer.Tasks;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private IDictionary<string, string> _buildParameters;

    public string TaskClassFullName { get; set; }
    public string TaskInterfaceFullName { get; set; }

    public TimeSpan Delay { get; set; }

    [JsonIgnore]
    public IDictionary<string, string> BuildParameters 
    { 
        get => _buildParameters;
        set {
            _buildParameters = value;
            SerializeParameters();
        }
    }

    public string BuildParametersSerialized { get; set; }

    [JsonIgnore]
    public FarmerPlan Plan { get; set;}

    [JsonIgnore]
    public string PlanId { get; set; }

    [JsonIgnore]
    public bool IsInProgress { get; set; }

    [JsonIgnore]
    public Exception LastException { get; set; }

    public string ID { get; set; }

    public Task<object> Execute(
        IDictionary<string, string> parameters, 
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private void SerializeParameters()
    {
        if (BuildParameters == null) {
            BuildParametersSerialized = null;
            return;
        }

        BuildParametersSerialized = 
            JsonSerializer
                .Serialize(
                    BuildParameters
                        .Select(x => x.ToString())
                        .ToArray());
    }
}