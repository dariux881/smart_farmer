namespace SmartFarmer.Alerts;

public enum AlertCode
{
    Unknown,
    InvalidProgramConfiguration,
    SerialCommunicationException,
    BlockedArm,
    BlockedOnGrid,
    BlockedTurningArm,
    BlockedPointingTarget
}