
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Exceptions;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlanStep : IFarmerPlanStep
{
    private IDictionary<string, string> _buildParameters;

    private IFarmerGarden _garden;
    private IFarmerTask _task;

    public FarmerPlanStep() 
    {

    }

    public string TaskClassFullName { get; set; }
    public string TaskInterfaceFullName { get; set; }

    public TimeSpan Delay { get; set; }

    public IDictionary<string, string> BuildParameters 
    { 
        get => _buildParameters;
        set {
            _buildParameters = value;
        }
    }

    public bool IsInProgress { get; set; }

    public Exception LastException { get; set; }

    public string ID { get; set; }

    public IFarmerPlanStep PropagateGarden(IFarmerGarden garden)
    {
        _garden = garden;
        return this;
    }
    
    /// <summary>
    /// Executes the plan step. Gathers the task, configures it with the parameters, waits the Delay, then runs the task. 
    /// </summary>
    /// <throws>Exception if task fails</throws>
    public async Task<object> Execute(
        IDictionary<string, string> parameters,
        CancellationToken token)
    {
        _task = GetTask();        
        if (_task == null) throw new InvalidOperationException("task implementor not found");

        if (Delay.TotalSeconds > 0)
        {
            SmartFarmerLog.Debug($"waiting {Delay.TotalSeconds} seconds for next task");
            await Task.Delay(Delay, token);
        }
        
        try
        {
            IsInProgress = true;

            ConfigureTask(_task, parameters ?? BuildParameters);

            return await _task.Execute(token);
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
                _task.ID, 
                null,
                ex.Message ?? ex.InnerException?.Message,
                ex);
        }
        finally
        {
            IsInProgress = false;
        }
    }

    private void ConfigureTask(IFarmerTask task, IDictionary<string, string> dictionary)
    {
        if (task == null) return;

        task.ConfigureTask(dictionary);
        // if (dictionary == null || !dictionary.Any()) return;

        // Type taskType = task.GetType();
        // IList<PropertyInfo> props = new List<PropertyInfo>(taskType.GetProperties().Where(x => dictionary.ContainsKey(x.Name)));

        // foreach (PropertyInfo prop in props)
        // {
        //     try
        //     {
        //         if (!dictionary.ContainsKey(prop.Name))
        //         {
        //             continue;
        //         }

        //         //get value in sourceObj
        //         object propValue = dictionary[prop.Name];

        //         //set value in resultObj
        //         PropertyInfo propResult = task.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                
        //         if (propResult != null && propResult.CanWrite)
        //         {
        //             propResult.SetValue(task, propValue, null);
        //         }
        //     }
        //     catch (Exception ex)
        //     {  
        //         SmartFarmerLog.Exception(ex);
        //     }
        // }
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
            return FarmerServiceLocator.GetServiceByFullName(TaskClassFullName, _garden.ID, true) as IFarmerTask;
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
            return FarmerServiceLocator.GetServiceByFullName(TaskInterfaceFullName, _garden.ID, true) as IFarmerTask;
        }
        catch(Exception ex)
        {
            LastException = ex;
            throw;
        }
    }
}