using Meadow.Hardware;
using Meadow.Modbus;
using System.Collections.Generic;

namespace Meadow.Foundation;

public class T3xxxPin : IPin
{
    public IPinController? Controller { get; }

    public IList<IChannelInfo>? SupportedChannels { get; }

    public string Name { get; }

    public object Key { get; }

    internal T3xxxPin(IPinController device, string name, int key, IChannelInfo channelInfo)
    {
        Controller = device;
        Name = name;
        Key = key;
        SupportedChannels = new List<IChannelInfo>([channelInfo]);
    }

    public bool Equals(IPin other)
    {
        throw new System.NotImplementedException();
    }
}

public partial class T38i8o6do : T3xxx, IDigitalInputOutputController
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

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        throw new System.NotImplementedException();
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var offset = (int)pin.Key;

        return new T3xxx.DigitalOutputPort(this, pin,
            (ushort)(T38i806doRegisters.AutoManualOut0 + offset),
            (ushort)(T38i806doRegisters.OutputRange0 + offset),
            (ushort)(T38i806doRegisters.DoChannel0 + offset)
            );
    }
}