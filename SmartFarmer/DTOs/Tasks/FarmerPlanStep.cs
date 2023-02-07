
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json; don't use this namespace
using SmartFarmer.Tasks.Generic;
using System.Linq;
using System.Text.Json;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private object[] _buildParameters;
    private string _buildParametersSerialized;

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
    public string BuildParametersSerialized {
        get => _buildParametersSerialized;

        set {
            _buildParametersSerialized = value;
            DeserializeParameters();
        }
    }

    public string[] BuildParametersString { get; private set; }

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

    private void DeserializeParameters()
    {
        if (string.IsNullOrEmpty(BuildParametersSerialized))
        {
            BuildParametersString = null;
            return;
        }

        BuildParametersString = JsonSerializer.Deserialize<string[]>(BuildParametersSerialized);
    }
}