
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json; don't use this namespace
using SmartFarmer.Tasks.Generic;
using System.Linq;
using System.Text.Json;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private object[] _buildParameters;

    public string TaskClassFullName { get; set; }

    public TimeSpan Delay { get; set; }

    [JsonIgnore]
    public object[] BuildParameters 
    { 
        get => _buildParameters;
        set {
            _buildParameters = value;
            SerializeParameters();
        }
    }

    [JsonIgnore]
    public string BuildParametersSerialized { get; set; }

    public string[] BuildParametersString 
    { 
        get {
            if (string.IsNullOrEmpty(BuildParametersSerialized))
            {
                return null;
            }

            return JsonSerializer.Deserialize<string[]>(BuildParametersSerialized);
        }
    }

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