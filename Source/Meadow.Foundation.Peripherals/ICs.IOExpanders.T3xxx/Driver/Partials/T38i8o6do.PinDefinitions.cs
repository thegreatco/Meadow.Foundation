using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.IOExpanders;

public partial class T38i8o6do
{
    public class PinDefinitions : PinDefinitionBase
    {
        internal PinDefinitions(T38i8o6do module)
        {
            Controller = module;
        }

        public IPinController Controller { get; set; }

        public IPin DO1 => new T3xxxPin(Controller, "DO1", 0, new DigitalChannelInfo("DO1", false, true, false, false, false));
        public IPin DO2 => new T3xxxPin(Controller, "DO2", 1, new DigitalChannelInfo("DO2", false, true, false, false, false));
        public IPin DO3 => new T3xxxPin(Controller, "DO3", 2, new DigitalChannelInfo("DO3", false, true, false, false, false));
        public IPin DO4 => new T3xxxPin(Controller, "DO4", 3, new DigitalChannelInfo("DO4", false, true, false, false, false));
        public IPin DO5 => new T3xxxPin(Controller, "DO5", 4, new DigitalChannelInfo("DO5", false, true, false, false, false));
        public IPin DO6 => new T3xxxPin(Controller, "DO6", 5, new DigitalChannelInfo("DO6", false, true, false, false, false));

        public IPin AI1 => new T3xxxPin(Controller, "AI1", 0,
            new VoltageInputChannelInfo("AI1"),
            new CurrentInputChannelInfo("AI1"));
        public IPin AI2 => new T3xxxPin(Controller, "AI2", 1,
            new VoltageInputChannelInfo("AI2"),
            new CurrentInputChannelInfo("AI2"));
        public IPin AI3 => new T3xxxPin(Controller, "AI3", 2,
            new VoltageInputChannelInfo("AI3"),
            new CurrentInputChannelInfo("AI3"));
        public IPin AI4 => new T3xxxPin(Controller, "AI4", 3,
            new VoltageInputChannelInfo("AI4"),
            new CurrentInputChannelInfo("AI4"));
        public IPin AI5 => new T3xxxPin(Controller, "AI5", 4,
            new VoltageInputChannelInfo("AI5"),
            new CurrentInputChannelInfo("AI5"));
        public IPin AI6 => new T3xxxPin(Controller, "AI6", 5,
            new VoltageInputChannelInfo("AI6"),
            new CurrentInputChannelInfo("AI6"));
        public IPin AI7 => new T3xxxPin(Controller, "AI7", 6,
            new VoltageInputChannelInfo("AI7"),
            new CurrentInputChannelInfo("AI7"));
        public IPin AI8 => new T3xxxPin(Controller, "AI8", 7,
            new VoltageInputChannelInfo("AI8"),
            new CurrentInputChannelInfo("AI8"));

        public IPin AO1 => new T3xxxPin(Controller, "AO1", 0,
            new VoltageOutputChannelInfo("AO1", 10.Volts()));
        public IPin AO2 => new T3xxxPin(Controller, "AO1", 1,
            new VoltageOutputChannelInfo("AO2", 10.Volts()));
        public IPin AO3 => new T3xxxPin(Controller, "AO1", 2,
            new VoltageOutputChannelInfo("AO3", 10.Volts()));
        public IPin AO4 => new T3xxxPin(Controller, "AO1", 3,
            new VoltageOutputChannelInfo("AO4", 10.Volts()));
        public IPin AO5 => new T3xxxPin(Controller, "AO1", 4,
            new VoltageOutputChannelInfo("AO5", 10.Volts()));
        public IPin AO6 => new T3xxxPin(Controller, "AO1", 5,
            new VoltageOutputChannelInfo("AO6", 10.Volts()));
        public IPin AO7 => new T3xxxPin(Controller, "AO1", 6,
            new VoltageOutputChannelInfo("AO7", 10.Volts()));
        public IPin AO8 => new T3xxxPin(Controller, "AO1", 7,
            new VoltageOutputChannelInfo("AO8", 10.Volts()));
    }
}