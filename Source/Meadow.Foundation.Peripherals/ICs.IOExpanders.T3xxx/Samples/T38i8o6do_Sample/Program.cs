using Meadow;
using Meadow.Foundation;
using Meadow.Modbus;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var port = new SerialPortShim("COM8", 19200, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var modbusClient = new ModbusRtuClient(port);

        await modbusClient.Connect();

        var t3 = new T38i8o6do(modbusClient, 254);
        var sn = await t3.ReadSerialNumber();

        var d01 = t3.Pins.DO1.CreateDigitalOutputPort(false);
        var d02 = t3.Pins.DO2.CreateDigitalOutputPort(false);

        while (true)
        {
            d01.State = true;
            d02.State = !d01.State;
            await Task.Delay(1000);
            d01.State = false;
            d02.State = !d01.State;
            await Task.Delay(1000);
        }
    }
}