
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json; don't use this namespace
using SmartFarmer.Tasks.Generic;
using System.Linq;
using System.Text.Json;
using SmartFarmer.Utils;
using SmartFarmer.Misc;
using SmartFarmer.Exceptions;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private object[] _buildParameters;
    private IFarmerTaskProvider _taskProvider = FarmerServiceLocator.GetService<IFarmerTaskProvider>(true);

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
    public bool IsInProgress { get; set; }

    [JsonIgnore]
    public Exception LastException { get; set; }

    public string ID { get; set; }

    public async Task Execute(object[] parameters, CancellationToken token)
    {
        if (string.IsNullOrEmpty(TaskClassFullName)) throw new InvalidOperationException("task not properly configured");

        IFarmerTask task;
        try {
            task = _taskProvider.GetTaskDelegateByClassFullName(TaskClassFullName);
        }
        catch(Exception ex)
        {
            LastException = ex;
            await Task.CompletedTask;
            throw;
        }

        if (task == null) throw new InvalidOperationException("task implementor not found");
        
        await Task.Delay(Delay, token);
        
        SmartFarmerLog.Information("preparing task " + TaskClassFullName);

        try
        {
            IsInProgress = true;
            await task.Execute(parameters ?? BuildParameters, token);
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw;
        }
        finally
        {
            IsInProgress = false;
        }
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