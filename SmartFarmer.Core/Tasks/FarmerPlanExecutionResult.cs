using System;
using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerPlanExecutionResult
{
    private bool _isSuccess = true;
    private Exception _lastException;

    public FarmerPlanExecutionResult()
    {
        TaskResults = new Dictionary<string, object>();
    }

    public string PlanId { get; set; }
    public bool IsSuccess 
    { 
        get => _isSuccess; 
        set
        {
            _isSuccess = value;
        }
    }

    public Exception LastException 
    { 
        get => _lastException; 
        set
        {
            _lastException = value;
            if (_lastException != null) IsSuccess = false;
        }
    }

    public Dictionary<string, object> TaskResults { get; set; }
}
