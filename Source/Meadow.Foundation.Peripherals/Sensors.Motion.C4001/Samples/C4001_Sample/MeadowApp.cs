using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using System.Threading.Tasks;

namespace Sensors.Motion.C4001_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        private C4001 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new C4001(Device.CreateI2cBus(), (byte)C4001.Addresses.Default);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            var status = sensor.GetStatus();
            Resolver.Log.Info($"C4001 Status: {status.WorkStatus} {status.WorkMode} {status.InitStatus}");

            return base.Run();
        }

        //<!=SNOP=>
    }
}