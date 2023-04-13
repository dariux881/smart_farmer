
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
    public string TaskInterfaceFullName { get; set; }

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
    
    /// <summary>
    /// Executes the plan step. Gathers the task, configures it with the parameters, waits the Delay, then runs the task. 
    /// </summary>
    /// <throws>Exception if task fails</throws>
    public async Task Execute(object[] parameters, CancellationToken token)
    {
        var task = GetTask();        
        if (task == null) throw new InvalidOperationException("task implementor not found");

        if (Delay.TotalSeconds > 0)
        {
            SmartFarmerLog.Debug($"waiting {Delay.TotalSeconds} seconds for next task");
            await Task.Delay(Delay, token);
        }
        
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

    /// <summary>
    /// Returns an instance of desired task type with priority: <class> - <interface>
    /// </summary>
    /// <returns>An instance of the desired task</returns>
    /// <throws>InvalidOperationException if task instance is not found</throws>
    private IFarmerTask GetTask()
    {
        return 
            GetTaskByClass() ?? // Prio 1 - configured class
            GetTaskByInterface() ??  // Prio 2 - configured interface
            throw new InvalidOperationException("task not properly configured"); // no task found, raising exception
    }

    /// <summary>
    /// Returns an instance of desired task type by given class
    /// </summary>
    /// <returns>An instance of the desired task</returns>
    /// <throws>Exception if task instance cannot be created</throws>
    private IFarmerTask GetTaskByClass()
    {
        if (string.IsNullOrEmpty(TaskClassFullName)) return null;

        try {
            SmartFarmerLog.Debug($"Getting instance of {TaskClassFullName}");
            return _taskProvider.GetTaskDelegateByClassFullName(TaskClassFullName, null, null, _ground);
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw;
        }
    }

    /// <summary>
    /// Returns an instance of desired task type by given interface
    /// </summary>
    /// <returns>An instance of the implementor of the desired task</returns>
    /// <throws>Exception if task instance cannot be created</throws>
    private IFarmerTask GetTaskByInterface()
    {
        if (string.IsNullOrEmpty(TaskInterfaceFullName)) return null;

        try {
            SmartFarmerLog.Debug($"Getting implementor of {TaskInterfaceFullName}");
            return _taskProvider.GetTaskDelegateByInterfaceFullName(TaskInterfaceFullName, null, null, _ground);
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw;
        }
    }
}