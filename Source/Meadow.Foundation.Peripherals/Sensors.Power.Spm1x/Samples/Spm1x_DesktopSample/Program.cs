using Meadow;
using Meadow.Foundation.Sensors.Power;
using Meadow.Logging;
using Meadow.Modbus;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Resolver.Services.Add(new Logger(new ConsoleLogProvider()));
        Resolver.Log.LogLevel = LogLevel.Debug;
        var port = new SerialPortShim("COM8", 19200, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var bus = new ModbusRtuClient(port);
        var sensor = new Spm1x(bus, 1);

        Resolver.Log.Info("Sensor info");
        Resolver.Log.Info("-----------------");
        Resolver.Log.Info($"SN:      {sensor.SerialNumber:D8}");
        Resolver.Log.Info($"Address: {sensor.ModbusAddress}");
        Resolver.Log.Info($"SW Vers: {sensor.SoftwareVersion}");
        Resolver.Log.Info($"Model:   {sensor.ProductModel}");
        Resolver.Log.Info($"HW Rev:  {sensor.HardwareRevision}");

        while (true)
        {
            var current = await sensor.ReadCurrent();
            var voltage = await sensor.ReadVoltage();

            Resolver.Log.Info($"{current.Amps:N2}A @ {voltage.Volts:N1}");

            await Task.Delay(2000);
        }
    }
}