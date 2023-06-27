using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Detection;

public class FarmerTakePictureTask : FarmerBaseTask, IFarmerTakePictureTask
{
    public FarmerTakePictureTask()
    {
        RequiredTool = FarmerTool.Camera;
    }

    public override string TaskName => "Take Picture Task";

    public string FilePath { get; private set; }

    public async override Task<object> Execute(CancellationToken token)
    {
        await Task.CompletedTask;
        
        throw new NotImplementedException();
    }
}