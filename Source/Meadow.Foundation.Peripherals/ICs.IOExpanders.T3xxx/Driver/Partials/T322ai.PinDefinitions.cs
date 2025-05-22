using Meadow.Hardware;

namespace Meadow.Foundation.IOExpanders;

public partial class T322ai
{
    public class PinDefinitions : PinDefinitionBase
    {
        internal PinDefinitions(T322ai module)
        {
            Controller = module;
        }

        public IPinController Controller { get; set; }

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
        public IPin AI9 => new T3xxxPin(Controller, "AI9", 8,
            new VoltageInputChannelInfo("AI9"),
            new CurrentInputChannelInfo("AI9"));
        public IPin AI10 => new T3xxxPin(Controller, "AI10", 9,
            new VoltageInputChannelInfo("AI10"),
            new CurrentInputChannelInfo("AI10"));
        public IPin AI11 => new T3xxxPin(Controller, "AI11", 10,
            new VoltageInputChannelInfo("AI11"),
            new CurrentInputChannelInfo("AI11"));
        public IPin AI12 => new T3xxxPin(Controller, "AI12", 11,
            new VoltageInputChannelInfo("AI12"),
            new CurrentInputChannelInfo("AI12"));
        public IPin AI13 => new T3xxxPin(Controller, "AI13", 12,
            new VoltageInputChannelInfo("AI13"),
            new CurrentInputChannelInfo("AI13"));
        public IPin AI14 => new T3xxxPin(Controller, "AI14", 13,
            new VoltageInputChannelInfo("AI14"),
            new CurrentInputChannelInfo("AI14"));
        public IPin AI15 => new T3xxxPin(Controller, "AI15", 14,
            new VoltageInputChannelInfo("AI15"),
            new CurrentInputChannelInfo("AI15"));
        public IPin AI16 => new T3xxxPin(Controller, "AI16", 15,
            new VoltageInputChannelInfo("AI16"),
            new CurrentInputChannelInfo("AI16"));
        public IPin AI17 => new T3xxxPin(Controller, "AI17", 16,
            new VoltageInputChannelInfo("AI17"),
            new CurrentInputChannelInfo("AI17"));
        public IPin AI18 => new T3xxxPin(Controller, "AI18", 17,
            new VoltageInputChannelInfo("AI18"),
            new CurrentInputChannelInfo("AI18"));
        public IPin AI19 => new T3xxxPin(Controller, "AI19", 18,
            new VoltageInputChannelInfo("AI19"),
            new CurrentInputChannelInfo("AI19"));
        public IPin AI20 => new T3xxxPin(Controller, "AI20", 19,
            new VoltageInputChannelInfo("AI20"),
            new CurrentInputChannelInfo("AI20"));
        public IPin AI21 => new T3xxxPin(Controller, "AI21", 20,
            new VoltageInputChannelInfo("AI21"),
            new CurrentInputChannelInfo("AI21"));
        public IPin AI22 => new T3xxxPin(Controller, "AI22", 21,
            new VoltageInputChannelInfo("AI22"),
            new CurrentInputChannelInfo("AI22"));
    }
}
