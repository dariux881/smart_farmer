
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;
using SmartFarmer.Misc;
using SmartFarmer.Exceptions;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private object[] _buildParameters;
    private IFarmerTaskProvider _taskProvider = FarmerServiceLocator.GetService<IFarmerTaskProvider>(true);
    private IFarmerGround _ground;

    public FarmerPlanStep() 
    {

    }

    [JsonConstructor]
    public FarmerPlanStep(string[] buildParametersString)
        : this()
    {
        BuildParameters = buildParametersString;
    }

    public string TaskClassFullName { get; set; }

    public TimeSpan Delay { get; set; }

    public object[] BuildParameters 
    { 
        get => _buildParameters;
        set {
            _buildParameters = value;
        }
    }

    public bool IsInProgress { get; set; }

    public Exception LastException { get; set; }

    public string ID { get; set; }

    public IFarmerPlanStep PropagateGround(IFarmerGround ground)
    {
        _ground = ground;
        return this;
    }

    public async Task Execute(object[] parameters, CancellationToken token)
    {
        var task = GetTask();        
        if (task == null) throw new InvalidOperationException("task implementor not found");

        if (Delay.TotalSeconds > 0)
        {
            SmartFarmerLog.Debug($"waiting {Delay.TotalSeconds} seconds for next task");
            await Task.Delay(Delay, token);
        }
        
        SmartFarmerLog.Information("preparing task " + TaskClassFullName);

        try
        {
            IsInProgress = true;
            await task.Execute(parameters ?? BuildParameters, token);
        }
        catch(FarmerTaskExecutionException taskEx)
        {
            LastException = taskEx;
            throw;
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw new FarmerTaskExecutionException(
                task.ID, 
                null, //TODO check if the task can provide plantId
                ex.Message ?? ex.InnerException?.Message,
                ex);
        }
        finally
        {
            IsInProgress = false;
        }
    }

    private IFarmerTask GetTask()
    {
        if (string.IsNullOrEmpty(TaskClassFullName)) throw new InvalidOperationException("task not properly configured");

        try {
            return _taskProvider.GetTaskDelegateByClassFullName(TaskClassFullName, null, null, _ground);
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw;
        }
    }
}