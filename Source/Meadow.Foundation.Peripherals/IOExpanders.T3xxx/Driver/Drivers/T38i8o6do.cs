using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Units;

namespace Meadow.Foundation.IOExpanders;

public partial class T38i8o6do
    : T3xxx,
    IDigitalOutputController,
    ICurrentInputController,
    IVoltageInputController,
    IVoltageOutputController
{
    public PinDefinitions Pins { get; }

    public T38i8o6do(ModbusRtuClient modbusRtuClient, byte moduleAddress)
        : base(modbusRtuClient, moduleAddress)
    {
        Pins = new PinDefinitions(this);
    }

    public T38i8o6do(ModbusTcpClient modbusTcpClient)
        : base(modbusTcpClient)
    {
        Pins = new PinDefinitions(this);
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var offset = (int)pin.Key;

        return new T3xxx.DigitalOutputPort(this, pin,
            initialState,
            (ushort)(T38i806doRegisters.AutoManualOut0 + offset),
            (ushort)(T38i806doRegisters.OutputRange0 + offset),
            (ushort)(T38i806doRegisters.DoChannel0 + offset)
            );
    }

    public ICurrentInputPort CreateCurrentInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        // 13 == 4-20 input
        WriteHoldingRegister((ushort)T38i806doRegisters.AiRange0, (ushort)AnalogInputRange.Current_4_20).Wait();

        // mode must be set to 1 (manual)
        WriteHoldingRegister((ushort)(T38i806doRegisters.AiDiAi0 + offset), 1).Wait();

        return new T3xxx.CurrentInputPort(this, pin,
            // each input is 2 registers, the data for current is in the low
            (ushort)(T38i806doRegisters.AiChannel0 + (offset * 2 + 1))
            );
    }

    public IVoltageInputPort CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        // 19 == 0-10V input, 11 == 0-5V
        // not sure the utility of 0-5, since 0-10 will also do 0-5...
        WriteHoldingRegister((ushort)(T38i806doRegisters.AiRange0 + offset), (ushort)AnalogInputRange.Voltage_0_10).Wait();
        // mode must be set to 1 (manual) in the T38o.  No idea why.
        WriteHoldingRegister((ushort)(T38i806doRegisters.AiDiAi0 + offset), 0).Wait();

        return new T3xxx.VoltageInputPort(this, pin,
            // each input is 2 registers, the data for voltage is in the low
            (ushort)(T38i806doRegisters.AiChannel0 + (offset * 2 + 1))
            );
    }

    public IVoltageOutputPort CreateVoltageOutputPort(IPin pin, Voltage initialVoltage)
    {
        var offset = (int)pin.Key;

        return new T3xxx.VoltageOutputPort(this, pin,
            initialVoltage,
            (ushort)(T38i806doRegisters.AutoManualOut6 + offset),
            (ushort)(T38i806doRegisters.OutputRange6 + offset),
            (ushort)(T38i806doRegisters.AoChannel0 + offset)
            );
    }

    public IVoltageOutputPort CreateVoltageOutputPort(IPin pin)
    {
        return CreateVoltageOutputPort(pin, Voltage.Zero);
    }
}