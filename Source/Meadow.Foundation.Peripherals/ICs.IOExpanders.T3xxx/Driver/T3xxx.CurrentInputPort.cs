using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation;

public abstract partial class T3xxx
{
    public class CurrentInputPort : ICurrentInputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _valueRegister;

        public IPin Pin { get; }

        internal CurrentInputPort(
            T3xxx module,
            IPin pin,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort valueRegister)
        {
            _module = module;
            Pin = pin;
            _valueRegister = valueRegister;

            // 13 == 4-20 input
            _module.WriteHoldingRegister(rangeRegister, (ushort)AnalogInputRange.Current_4_20).Wait();

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).Wait();
        }

        public async ValueTask<Current> Read()
        {
            var register = await _module.ReadHoldingRegister(_valueRegister);

            return new Current(register / 100d);
        }
    }
}
