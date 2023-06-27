namespace SmartFarmer.Tasks.Generic;

public interface IHasTargetGridPosition
{
    double TargetXInCm { get; }
    double TargetYInCm { get; }
}