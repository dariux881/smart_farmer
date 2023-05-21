namespace SmartFarmer.OperationalManagement;

public enum AppOperation
{
    RunPlan,
    RunAutoIrrigationPlan,
    SchedulePlans,
    StopRunningPlan,
    MarkAlert,
    UpdateGround,
    UpdateAllGrounds,
    RestartSerialCom,
    CliCommand,
    TestPosition,
    MoveToPosition,
    StopCurrentOperation,
    TakePicture
}
