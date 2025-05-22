using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

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
            ushort valueRegister)
        {
            _module = module;
            Pin = pin;
            _valueRegister = valueRegister;
        }

        public async ValueTask<Current> Read()
        {
            var register = await _module.ReadHoldingRegister(_valueRegister);

            return new Current(register / 100d);
        }
    }
}
