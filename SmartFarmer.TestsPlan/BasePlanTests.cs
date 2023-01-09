using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFarmer.Misc;

namespace SmartFarmer.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task LoadingPlan_ExpectedSuccess()
    {
        var moveToTargetPlan = new MoveToTargetPlan();

        Assert.IsNotNull(moveToTargetPlan);

        try 
        {
            await moveToTargetPlan.Execute(System.Threading.CancellationToken.None);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            Assert.Fail(ex.Message);
        }
    }
}