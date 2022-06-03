namespace SmartFarmer
{
    public interface IFarmerRecurrentTask : IFarmerTask
    {
        int Frequency { get; }
    }
}
