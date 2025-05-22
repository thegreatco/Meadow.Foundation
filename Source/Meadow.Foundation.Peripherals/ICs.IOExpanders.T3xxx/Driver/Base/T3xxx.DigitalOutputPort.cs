using Meadow.Hardware;
using System.Linq;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    public class DigitalOutputPort : IDigitalOutputPort
    {
        private readonly ushort _outputRegister;
        private readonly T3xxx _module;
        private bool? _lastSetState = null;

        public bool InitialState { get; }

        public IDigitalChannelInfo Channel { get; }

        public IPin Pin { get; }

        internal DigitalOutputPort(
            T3xxx module,
            IPin pin,
            bool initialState,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort outputRegister)
        {
            _module = module;
            Pin = pin;
            _outputRegister = outputRegister;

            Channel = (IDigitalChannelInfo)pin.SupportedChannels.First(c => c is IDigitalChannelInfo);

            // range set to on/off
            _module.WriteHoldingRegister(rangeRegister, 1).Wait();

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).Wait();

            State = InitialState = initialState;
        }

        public bool State
        {
            set
            {
                _module.WriteHoldingRegister(_outputRegister, (ushort)(value ? 1 : 0)).GetAwaiter().GetResult();
                _lastSetState = value;
            }
            get
            {
                if (_lastSetState == null)
                {
                    _lastSetState = _module.ReadHoldingRegister(_outputRegister).GetAwaiter().GetResult() != 0;
                }
                return _lastSetState.Value;
            }
        }

        public void Dispose()
        {
        }
    }
}
