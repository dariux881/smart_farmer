namespace SmartFarmer.Tasks
{
    public interface IFarmerRecurrentTask : IFarmerTask
    {
        int Frequency { get; }
    }
}
