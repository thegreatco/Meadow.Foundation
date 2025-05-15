using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using System.Threading.Tasks;

namespace DeepSleep_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>
    private DeepSleep module;

    public override async Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        var i2cBus = Device.CreateI2cBus();

        module = new DeepSleep(i2cBus);
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");
        //module.ResetInterrupts();

        /*
        var time = module.GetTime();
        Resolver.Log.Info($"{time}");

        module.SetTime(time.AddDays(5));
        time = module.GetTime();
        Resolver.Log.Info($"{time}");
        */

        //module.ScheduleWakeUp(time.AddSeconds(25));
        // module.SetSleepDelay(5, DelayTimeUnit.Seconds);

        // module.SetTimerA(2, Meadow.Foundation.RTCs.DelayTimeUnit.Seconds);
        // module.SetTimerB(5, Meadow.Foundation.RTCs.DelayTimeUnit.Seconds);

        for (int i = 0; i < 50; i++)
        {
            Resolver.Log.Info($"{module.IsAlarmInterruptGenerated}, {module.IsTimerInterruptAGenerated}, {module.IsTimerInterruptBGenerated} ... {i}");
            await Task.Delay(1000);
        }
    }

    //<!=SNOP=>
}