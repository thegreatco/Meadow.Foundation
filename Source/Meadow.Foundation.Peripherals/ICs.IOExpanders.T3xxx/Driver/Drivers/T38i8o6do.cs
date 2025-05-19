using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Units;

namespace Meadow.Foundation;

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

        return new T3xxx.CurrentInputPort(this, pin,
            (ushort)(T38i806doRegisters.AiDiAi0 + offset),
            (ushort)(T38i806doRegisters.AiRange0 + offset),
            // each input is 2 registers, the data for current is in the low
            (ushort)(T38i806doRegisters.AiChannel0 + (offset * 2 + 1))
            );
    }

    public IVoltageInputPort CreateVoltageInputPort(IPin pin)
    {
        var offset = (int)pin.Key;

        return new T3xxx.VoltageInputPort(this, pin,
            (ushort)(T38i806doRegisters.AiDiAi0 + offset),
            (ushort)(T38i806doRegisters.AiRange0 + offset),
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
}