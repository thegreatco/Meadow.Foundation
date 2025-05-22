using Meadow.Hardware;
using Meadow.Modbus;

namespace Meadow.Foundation.IOExpanders;

public partial class T322ai
    : T3xxx,
    ICurrentInputController,
    IVoltageInputController
{
    public PinDefinitions Pins { get; }

    public T322ai(ModbusRtuClient modbusRtuClient, byte moduleAddress)
    : base(modbusRtuClient, moduleAddress)
    {
        Pins = new PinDefinitions(this);
    }

    public T322ai(ModbusTcpClient modbusTcpClient)
        : base(modbusTcpClient)
    {
        Pins = new PinDefinitions(this);
    }

    public ICurrentInputPort CreateCurrentInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Current_4_20).Wait();
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0).Wait();
        // set it as an analog input 
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1).Wait();

        return new T3xxx.CurrentInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }

    public IVoltageInputPort CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        WriteHoldingRegister((ushort)(T322aiRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Voltage_0_10).Wait();
        // mode must be set to 0 (auto) in the T322i.  No idea why.
        WriteHoldingRegister((ushort)(T322aiRegisters.AutoManual0 + offset), 0).Wait();
        // set it as an analog input 
        WriteHoldingRegister((ushort)(T322aiRegisters.AiDiAi0 + offset), 1).Wait();

        return new T3xxx.VoltageInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T322aiRegisters.AiChannel0Hi + (offset * 2 + 1))
            );
    }
}
