using System;

namespace SmartFarmer.OperationalManagement.Jobs;

public class SchedulerJobEventArgs : EventArgs
{
    public AutoAppOperation Operation { get; private set; }
    public object[] Data { get; private set; }

    public SchedulerJobEventArgs(AutoAppOperation operation, object[] data)
    {
        Operation = operation;
        Data = data;
    }
}
