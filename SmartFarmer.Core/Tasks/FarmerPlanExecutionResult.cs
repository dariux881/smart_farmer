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

    public bool IsSuccess => LastException == null;

    public string ErrorMessage => LastException?.InnerException?.Message ?? LastException?.Message;

    public Exception LastException { private get; set; }

    public Dictionary<string, object> TaskResults { get; set; }
}
