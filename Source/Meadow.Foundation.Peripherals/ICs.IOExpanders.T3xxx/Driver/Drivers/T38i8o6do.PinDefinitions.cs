using Meadow.Hardware;

namespace Meadow.Foundation;

public partial class T38i8o6do
{
    public class PinDefinitions : PinDefinitionBase
    {
        internal PinDefinitions(T38i8o6do module)
        {
            Controller = module;
        }

        public IPinController? Controller { get; set; }

        public IPin DO1 => new T3xxxPin(Controller, "DO1", 0, new DigitalChannelInfo("DO1", false, true, false, false, false));
        public IPin DO2 => new T3xxxPin(Controller, "DO2", 1, new DigitalChannelInfo("DO2", false, true, false, false, false));

    }
}