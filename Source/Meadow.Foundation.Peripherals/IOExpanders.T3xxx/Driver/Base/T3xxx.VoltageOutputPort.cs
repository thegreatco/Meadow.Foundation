using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public class VoltageInputChannelInfo : IChannelInfo
{
    public string Name { get; }

    public VoltageInputChannelInfo(string name)
    {
        Name = name;
    }
}

public class VoltageOutputChannelInfo : IChannelInfo
{
    public string Name { get; }
    public Voltage MaxVoltage { get; }

    public VoltageOutputChannelInfo(string name, Voltage maxVoltage)
    {
        Name = name;
        MaxVoltage = maxVoltage;
    }
}

public class CurrentInputChannelInfo : IChannelInfo
{
    public string Name { get; }

    public CurrentInputChannelInfo(string name)
    {
        Name = name;
    }
}

public interface IVoltageInputController
{
    IVoltageInputPort CreateVoltageInputPort(IPin pin);
}

public interface IVoltageOutputController
{
    IVoltageOutputPort CreateVoltageOutputPort(IPin pin);
    IVoltageOutputPort CreateVoltageOutputPort(IPin pin, Voltage initialVoltage);
}

public interface ICurrentInputController
{
    ICurrentInputPort CreateCurrentInputPort(IPin pin);
}

public interface ICurrentInputPort
{
    IPin Pin { get; }
    ValueTask<Current> Read();
}

public interface IVoltageInputPort
{
    IPin Pin { get; }
    ValueTask<Voltage> Read();
}

public interface IVoltageOutputPort
{
    IPin Pin { get; }
    Task SetOutput(Voltage value);
}

public abstract partial class T3xxx
{
    internal enum AnalogOutputRange
    {
        Voltage_0_10 = 1
    }

    internal enum AnalogInputRange
    {
        Voltage_0_5 = 11,
        Current_4_20 = 13,
        Voltage_0_10 = 19
    }

    public class VoltageOutputPort : IVoltageOutputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _outputRegister;

        public IPin Pin { get; }

        public VoltageOutputPort(
            T3xxx module,
            IPin pin,
            Voltage initialVoltage,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort outputRegister)
        {
            _module = module;
            Pin = pin;

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).Wait();
            _module.WriteHoldingRegister(rangeRegister, (ushort)AnalogOutputRange.Voltage_0_10).Wait();

            _outputRegister = outputRegister;

            SetOutput(initialVoltage);
        }

        public Task SetOutput(Voltage value)
        {
            return _module.WriteHoldingRegister(_outputRegister, (ushort)value.Millivolts);
        }
    }
}
