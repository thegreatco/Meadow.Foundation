using Meadow;
using Meadow.Devices;
using Meadow.Foundation.MBus.RelayMBus;
using Meadow.Foundation.mikroBUS.Sensors.MBus;
using System;
using System.Threading.Tasks;

namespace CMBusMaster_ProjectLab_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private CMBusMaster master;
        private IProjectLabHardware projectLab;
        private PadPulsM2 pulseCounter;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            projectLab = ProjectLab.Create();

            master = new CMBusMaster(
                projectLab.MikroBus1.CreateSerialPort(
                    baudRate: 9600,
                    parity: Meadow.Hardware.Parity.Even)
                );

            pulseCounter = new PadPulsM2(master);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            pulseCounter.StartMonitoring();

            Resolver.Log.Info($"Ports: {pulseCounter.Ports[0].ID:X8} {pulseCounter.Ports[1].ID:X8}");

            while (true)
            {
                Resolver.Log.Info($"Counts: {pulseCounter.Ports[0].CurrentCount:X8} {pulseCounter.Ports[1].CurrentCount:X8}");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        //<!=SNOP=>
    }
}