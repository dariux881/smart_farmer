namespace SmartFarmer.OperationalManagement;

public enum AppOperation
{
    RunPlan,
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
