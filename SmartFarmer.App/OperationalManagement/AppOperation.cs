namespace SmartFarmer.OperationalManagement;

public enum AppOperation
{
    RunPlan,
    RunVolatilePlan,
    RunAutoIrrigationPlan,
    SchedulePlans,
    StopRunningPlan,
    MarkAlert,
    UpdateGarden,
    UpdateAllGardens,
    RestartSerialCom,
    CliCommand,
    TestPosition,
    MoveToPosition,
    StopCurrentOperation,
    TakePicture
}
