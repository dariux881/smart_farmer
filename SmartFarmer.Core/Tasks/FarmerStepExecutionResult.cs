namespace SmartFarmer.Tasks;

public class FarmerStepExecutionResult
{
    public string StepId { get; set; }

    public object Result { get; set; }

    public string PlantInstanceId { get; set; }

    public string TaskInterfaceFullName { get; set; }
}