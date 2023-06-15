namespace SmartFarmer.OperationalManagement;

public enum AppOperation
{
    RunPlan,
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
