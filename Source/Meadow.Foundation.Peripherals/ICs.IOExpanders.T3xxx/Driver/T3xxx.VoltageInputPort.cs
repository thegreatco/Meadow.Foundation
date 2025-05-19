using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation;

public abstract partial class T3xxx
{
    public class VoltageInputPort : IVoltageInputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _valueRegister;

        public IPin Pin { get; }

        internal VoltageInputPort(
            T3xxx module,
            IPin pin,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort valueRegister)
        {
            _module = module;
            Pin = pin;
            _valueRegister = valueRegister;

            // 19 == 0-10V input, 11 == 0-5V
            // not sure the utility of 0-5, since 0-10 will also do 0-5...
            _module.WriteHoldingRegister(rangeRegister, (ushort)AnalogInputRange.Voltage_0_10).Wait();

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).Wait();
        }

        public async ValueTask<Voltage> Read()
        {
            var register = await _module.ReadHoldingRegister(_valueRegister);

            return new Voltage(register / 100d);
        }
    }
}
