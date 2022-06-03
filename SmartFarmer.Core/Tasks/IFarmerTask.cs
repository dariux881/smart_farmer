using SmartFarmer.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer
{
    public interface IFarmerTask
    {
        FarmerTool RequiredTool { get; }
        Task Execute(object[] parameters, CancellationToken token);
    }
}
