namespace SmartFarmer.Tasks.Generic
{
    public interface IFarmerRecurrentTask : IFarmerTask
    {
        int Frequency { get; }
    }
}
